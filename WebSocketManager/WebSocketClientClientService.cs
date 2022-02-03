using Serilog;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public class WebSocketClientClientService<TMessageHandler> : WebSocketServiceBase, IWebSocketClientService
        where TMessageHandler : class, IWebSocketReceivingMessageHandler
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Type, ClientWebSocket> _sockets = new ConcurrentDictionary<Type, ClientWebSocket>();

        public WebSocketClientClientService(ConnectionManager webSocketConnectionManager, ILogger logger, IWebSocketReceivingMessageHandler webSocketReceivingMessageHandler) :
            base(logger, webSocketReceivingMessageHandler)
        {
            _logger = logger.ForContext<WebSocketClientClientService<TMessageHandler>>();
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public async Task InitialSocketWithRetry(Endpoint options, CancellationToken stoppingToken)
        {
            if (options?.ServerEndpoint == null)
            {
                throw new System.Configuration.ConfigurationErrorsException("ServerEndpoints cannot be null");
            }

            var retryAfter = TimeSpan.FromSeconds(options.RetryConnectInSeconds ?? 15);

            while (!stoppingToken.IsCancellationRequested) // retry control while loop
            {
                _logger.Information("Trying to connect to {Endpoint}", options.ServerEndpoint);

                try
                {
                    var socket = _sockets.AddOrUpdate(typeof(TMessageHandler), type => new ClientWebSocket(), (type, foundSocket) => new ClientWebSocket());
                    await socket.ConnectAsync(new Uri(options.ServerEndpoint), stoppingToken);

                    Log.Information("I am Connected to {Endpoint}!", options.ServerEndpoint);

                    await InitializeSocket(socket, stoppingToken);
                }
                catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.Faulted)
                {
                    _logger.Warning("Cannot establish connection. Retrying after {RetryInSeconds} seconds", options.RetryConnectInSeconds);
                    Thread.Sleep(retryAfter);
                }
                catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    _logger.Warning("Connection Lost at Client. . Retrying after a while");
                    Thread.Sleep(retryAfter);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Cannot connect");
                }
            }
        }

        public async Task SendMessageToServerAsync(string message)
        {
            if (_sockets.TryGetValue(typeof(TMessageHandler), out var clientWebSocket))
            {
                await SendMessageAsync(clientWebSocket, message);
            }
        }

        public async Task<WebSocketClientConfiguration> GetConfiguration()
        {
            return await Task.FromResult(new WebSocketClientConfiguration());
        }
    }
}

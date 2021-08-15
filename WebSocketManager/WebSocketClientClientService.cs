using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;

namespace WebSocketManager
{
    public class WebSocketClientClientService<TMessageHandler> : WebSocketServiceBase, IWebSocketClientService
        where TMessageHandler : class, IWebSocketReceivingMessageHandler
    {
        private readonly ILogger _logger;
        private readonly IOptions<WebSocketClientConfiguration> _options;

        private readonly ConcurrentDictionary<Type, ClientWebSocket> _sockets = new ConcurrentDictionary<Type, ClientWebSocket>();

        public WebSocketClientClientService(ConnectionManager webSocketConnectionManager, ILogger logger, IOptions<WebSocketClientConfiguration> options, IWebSocketReceivingMessageHandler webSocketReceivingMessageHandler):
            base(logger, webSocketReceivingMessageHandler)
        {
            _logger = logger.ForContext<WebSocketClientClientService<TMessageHandler>>();
            _options = options;
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public async Task InitialSocketWithRetry(CancellationToken stoppingToken)
        {
            if (_options?.Value.ServerEndpoint == null)
            {
                throw new System.Configuration.ConfigurationErrorsException("ServerEndpoint cannot be null");
            }

            var options = _options.Value;

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
            return await Task.FromResult(_options.Value);
        }
    }
}

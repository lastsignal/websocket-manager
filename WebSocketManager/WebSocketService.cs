using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

namespace WebSocketManager
{
    public abstract class WebSocketServiceBase
    {
        private readonly ILogger _logger;
        private readonly IWebSocketReceivingMessageHandler _webSocketReceivingMessageHandler;

        protected WebSocketServiceBase(ILogger logger, IWebSocketReceivingMessageHandler webSocketReceivingMessageHandler)
        {
            _logger = logger;
            _webSocketReceivingMessageHandler = webSocketReceivingMessageHandler;
        }

        protected ConnectionManager WebSocketConnectionManager { get; set; }

        public async Task InitializeSocket(WebSocket socket, CancellationToken stoppingToken)
        {
            try
            {
                await OnConnected(socket);

                var buffer = new ArraySegment<byte>(new byte[1024 * 4]);

                while (!stoppingToken.IsCancellationRequested)
                {
                    // wait for the next ws message to arrive
                    var result = await socket.ReceiveAsync(buffer, stoppingToken);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            await ReceiveAsync(socket, result, buffer, stoppingToken);
                            break;
                        case WebSocketMessageType.Close:
                            await OnDisconnected(socket, stoppingToken);
                            break;
                        case WebSocketMessageType.Binary:
                            break;
                        default:
                            _logger.Debug("{MessageType}", result.MessageType);
                            break;
                    }
                }
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                _logger.Warning(e, "Connection Lost");
                throw;
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.InvalidState)
            {
                _logger.Warning(e, "Invalid State");
                throw;
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e, "Operation Cancelled");
                await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket), stoppingToken);
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Error Receiving Message");
                throw;
            }
        }

        protected async Task SendMessageAsync(WebSocket webSocket, string message)
        {
            // ReSharper disable once ArgumentsStyleLiteral
            using (LogContext.PushProperty("socket", webSocket, destructureObjects: true))
            using (LogContext.PushProperty("message", message))
            {
                if (webSocket.State != WebSocketState.Open)
                {
                    _logger.Warning("Socket is not open!");
                    return;
                }

                try
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(message),
                            0,
                            message.Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Problem with sending message!");
                }
            }
        }

        private async Task OnConnected(WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socket);
            var socketId = WebSocketConnectionManager.GetId(socket);
            _logger.Information("A connection is established in server with {SocketId}", socketId);
            await Task.CompletedTask;
        }

        private async Task OnDisconnected(WebSocket socket, CancellationToken stoppingToken)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            _logger.Information("A connection is closed in server with {SocketId}", socketId);
            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", stoppingToken);
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket), stoppingToken);
        }

        private async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, ArraySegment<byte> buffer, CancellationToken stoppingToken)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            var message = $"{Encoding.UTF8.GetString(buffer.Array ?? Array.Empty<byte>(), 0, result.Count)}";

            var msg = $"{Extensions.GetMachineName()} [{socketId}]: Received: {message}";

            _logger.Debug("Received {Message}", msg);

            if (_webSocketReceivingMessageHandler != null)
            {
                await _webSocketReceivingMessageHandler.HandleAsync(message, stoppingToken);
            }

            await Task.CompletedTask;
        }
    }
}

using Serilog;
using System.Threading;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketServer
{
    public class ServerSideWebSocketReceivingMessageHandler : IWebSocketReceivingMessageHandler
    {
        private readonly ILogger _logger;

        public ServerSideWebSocketReceivingMessageHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(string message, CancellationToken stoppingToken)
        {
            _logger.Information("Message Received: {Message}", message);
            await Task.CompletedTask;
        }
    }
}

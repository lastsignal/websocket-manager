using Serilog;
using System.Threading;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketClient
{
    public class WebSocketMessageHandler : IWebSocketReceivingMessageHandler
    {
        private readonly ILogger _logger;

        public WebSocketMessageHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(string message, CancellationToken stoppingToken)
        {
            if (message == "cache-reset")
                _logger.Information("I am the handler");

            await Task.CompletedTask;
        }
    }
}

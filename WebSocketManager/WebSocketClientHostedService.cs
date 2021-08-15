using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WebSocketManager
{
    public class WebSocketClientHostedService<TMessageHandler> : BackgroundService
        where TMessageHandler : IWebSocketReceivingMessageHandler
    {
        private readonly ILogger _logger;
        private readonly IWebSocketClientService _webSocketClientService;

        public WebSocketClientHostedService(ILogger logger, IWebSocketClientService webSocketClientService)
        {
            _logger = logger;
            _webSocketClientService = webSocketClientService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"{nameof(WebSocketClientHostedService<TMessageHandler>)} started");

            await _webSocketClientService.InitialSocketWithRetry(stoppingToken);
        }
    }
}
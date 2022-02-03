using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WebSocketManager
{
    public class WebSocketClientHostedService1<TMessageHandler> : BackgroundService
        where TMessageHandler : IWebSocketReceivingMessageHandler
    {
        private readonly Endpoint _clientConfiguration;
        private readonly ILogger _logger;
        private readonly IWebSocketClientService _webSocketClientService;

        public WebSocketClientHostedService1(Endpoint clientConfiguration, ILogger logger, IWebSocketClientService webSocketClientService)
        {
            _clientConfiguration = clientConfiguration;
            _logger = logger;
            _webSocketClientService = webSocketClientService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"{nameof(WebSocketClientHostedService1<TMessageHandler>)} started");

            await _webSocketClientService.InitialSocketWithRetry(_clientConfiguration, stoppingToken);
        }
    }

    public class WebSocketClientHostedService2<TMessageHandler> : BackgroundService
        where TMessageHandler : IWebSocketReceivingMessageHandler
    {
        private readonly Endpoint _clientConfiguration;
        private readonly ILogger _logger;
        private readonly IWebSocketClientService _webSocketClientService;

        public WebSocketClientHostedService2(Endpoint clientConfiguration, ILogger logger, IWebSocketClientService webSocketClientService)
        {
            _clientConfiguration = clientConfiguration;
            _logger = logger;
            _webSocketClientService = webSocketClientService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"{nameof(WebSocketClientHostedService2<TMessageHandler>)} started");

            await _webSocketClientService.InitialSocketWithRetry(_clientConfiguration, stoppingToken);
        }
    }
}
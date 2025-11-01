using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WebSocketManager;

public class WebSocketClientHostedService<TMessageHandler>(ILogger logger, IWebSocketClientService webSocketClientService) : BackgroundService
    where TMessageHandler : IWebSocketReceivingMessageHandler
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Information($"{nameof(WebSocketClientHostedService<TMessageHandler>)} started");

        await webSocketClientService.InitialSocketWithRetry(stoppingToken);
    }
}

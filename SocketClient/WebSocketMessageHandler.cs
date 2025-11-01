using Serilog;
using System.Threading;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketClient;

public class WebSocketMessageHandler(ILogger logger) : IWebSocketReceivingMessageHandler
{
    public async Task HandleAsync(string message, CancellationToken stoppingToken)
    {
        if (message == "cache-reset")
            logger.Information("I am the handler");

        await Task.CompletedTask;
    }
}

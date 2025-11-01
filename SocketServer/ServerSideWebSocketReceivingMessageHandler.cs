using Serilog;
using System.Threading;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketServer;

public class ServerSideWebSocketReceivingMessageHandler(ILogger logger) : IWebSocketReceivingMessageHandler
{
    public async Task HandleAsync(string message, CancellationToken stoppingToken)
    {
        logger.Information("Message Received: {Message}", message);
        await Task.CompletedTask;
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager;

public interface IWebSocketClientService
{
    Task InitialSocketWithRetry(CancellationToken stoppingToken);

    Task SendMessageToServerAsync(string message);

    Task<WebSocketClientConfiguration> GetConfiguration();
}

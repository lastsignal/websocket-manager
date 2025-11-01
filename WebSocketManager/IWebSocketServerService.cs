using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager;

public interface IWebSocketServerService
{
    Task InitializeSocket(WebSocket socket, CancellationToken stoppingToken);

    Task SendMessageToAllClientsAsync(string message);
}

using System.Net.WebSockets;
using System.Threading.Tasks;
using Serilog;

namespace WebSocketManager
{
    public class WebSocketServerService : WebSocketServiceBase, IWebSocketServerService
    {

        public WebSocketServerService(ConnectionManager webSocketConnectionManager, ILogger logger, IWebSocketReceivingMessageHandler webSocketReceivingMessageHandler) :
            base(logger, webSocketReceivingMessageHandler)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public async Task SendMessageToAllClientsAsync(string message)
        {
            await foreach (var (_, value) in WebSocketConnectionManager.GetAllAsync())
            {
                if (value.State == WebSocketState.Open)
                    await SendMessageAsync(value, message);
            }
        }
    }
}

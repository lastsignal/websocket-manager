using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public interface IWebSocketReceivingMessageHandler
    {
        // ReSharper disable UnusedParameter.Global : using the parameters is up to the consumer implementer
        Task HandleAsync(string message, CancellationToken stoppingToken);
    }
}

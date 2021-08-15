using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace WebSocketManager
{
    // ReSharper disable once ClassNeverInstantiated.Global : Middleware get instantiated under the hood
    // ReSharper disable once UnusedParameter.Local : 'RequestDelegate' is mandatory in Middleware
    // ReSharper disable once UnusedMember.Global : InvokeAsync is called withing the middleware pipeline

    /// <summary>
    /// This is the WebSocket server side middleware. Note that it should not be registered as a normal middleware.
    /// Instead we use Map with the specific path, e.g.: /ws
    /// </summary>
    public class WebSocketServerMiddleware
    {
        private readonly IWebSocketServerService _webSocketServerService;
        private readonly ILogger _logger;

        public WebSocketServerMiddleware(RequestDelegate next, IWebSocketServerService webSocketServerService, ILogger logger)
        {
            _webSocketServerService = webSocketServerService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stoppingToken = context.RequestAborted;

            if (context.WebSockets.IsWebSocketRequest)
            {
                // create a websocket as a listener 
                var socket = await context.WebSockets.AcceptWebSocketAsync();

                try
                {
                    // assign the receiving action
                    await _webSocketServerService.InitializeSocket(socket, stoppingToken);
                }
                catch (WebSocketException)
                {
                    _logger.Warning("Connection Lost in the middleware");
                    throw;
                }
            }
        }
    }
}

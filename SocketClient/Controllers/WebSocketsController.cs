using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Globalization;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IWebSocketClientService _webSocketClientService;

        public WebSocketsController(ILogger logger, IWebSocketClientService webSocketClientService)
        {
            _logger = logger;
            _webSocketClientService = webSocketClientService;
        }

        [HttpGet("/status")]
        public async Task<IActionResult> GetStatus()
        {
            return Ok(await _webSocketClientService.GetConfiguration());
        }

        [HttpGet("/ping/{message?}")]
        public async Task Get(string message)
        {
            _logger.Information("/ping called");

            message ??= DateTime.Now.ToString(CultureInfo.InvariantCulture);
            await _webSocketClientService.SendMessageToServerAsync($"I am {WebSocketManager.Extensions.GetMachineName()}");
            await _webSocketClientService.SendMessageToServerAsync($"Sending {message}");
        }
    }
}

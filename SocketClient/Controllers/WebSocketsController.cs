using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Globalization;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketClient.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketsController(ILogger logger, IWebSocketClientService webSocketClientService) : ControllerBase
{
    [HttpGet("/status")]
    public async Task<IActionResult> GetStatus()
    {
        return Ok(await webSocketClientService.GetConfiguration());
    }

    [HttpGet("/ping/{message?}")]
    public async Task Get(string message)
    {
        logger.Information("/ping called");

        var port = HttpContext.Connection.LocalPort;

        message ??= DateTime.Now.ToString(CultureInfo.InvariantCulture);
        await webSocketClientService.SendMessageToServerAsync($"{message} from client at machine: '{WebSocketManager.Extensions.GetMachineName()}', port: {port}");
    }
}

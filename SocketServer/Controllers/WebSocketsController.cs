using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Globalization;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketServer.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketsController(ILogger logger, IWebSocketServerService messageServerService, ConnectionManager connectionManager) : ControllerBase
{
    [HttpGet("/status")]
    public async Task<IActionResult> GetInfo()
    {
        logger.Information("app information: {MachineName}", Environment.MachineName);
        var connections = connectionManager.GetAll();

        var result = new
        {
            count = connections.Count,
            connections
        };

        logger.Information("{Connections}", JsonConvert.SerializeObject(result));

        return Ok(await Task.FromResult(result));
    }

    [HttpGet("/ping/{message?}")]
    public async Task Get(string message)
    {
        logger.Information("/ping called");

        message ??= DateTime.Now.ToString(CultureInfo.InvariantCulture);

        await messageServerService.SendMessageToAllClientsAsync(message);
    }
}
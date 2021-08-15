using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Globalization;
using System.Threading.Tasks;
using WebSocketManager;

namespace SocketServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IWebSocketServerService _messageServerService;
        private readonly ConnectionManager _connectionManager;

        public WebSocketsController(ILogger logger, IWebSocketServerService messageServerService, ConnectionManager connectionManager)
        {
            _logger = logger;
            _messageServerService = messageServerService;
            _connectionManager = connectionManager;
        }

        [HttpGet("/status")]
        public async Task<IActionResult> GetInfo()
        {
            _logger.Information("app information: {MachineName}", Environment.MachineName);
            var connections = _connectionManager.GetAll();

            var result = new
            {
                count = connections.Count,
                connections
            };

            _logger.Information("{Connections}", JsonConvert.SerializeObject(result));

            return Ok(await Task.FromResult(result));
        }

        [HttpGet("/ping/{message?}")]
        public async Task Get(string message)
        {
            _logger.Information("/ping called");

            message ??= DateTime.Now.ToString(CultureInfo.InvariantCulture);

            await _messageServerService.SendMessageToAllClientsAsync(message);
        }
    }
}
using System.Collections.Generic;

namespace WebSocketManager;

public class WebSocketClientConfiguration
{
    public IEnumerable<string> ServerEndpoints { get; set; }
    public int RetryConnectInSeconds { get; set; } = 15;
}

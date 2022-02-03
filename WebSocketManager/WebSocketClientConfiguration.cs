using System.Collections.Generic;

namespace WebSocketManager
{
    public class WebSocketClientConfiguration
    {
        public IEnumerable<Endpoint> Endpoints { get; set; }
    }

    public class Endpoint
    {
        public string ServerEndpoint { get; set; }
        public int? RetryConnectInSeconds { get; set; }
    }
}

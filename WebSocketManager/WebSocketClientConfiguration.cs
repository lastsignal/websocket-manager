namespace WebSocketManager
{
    public class WebSocketClientConfiguration
    {
        public string ServerEndpoint { get; set; }
        public int? RetryConnectInSeconds { get; set; }
    }
}

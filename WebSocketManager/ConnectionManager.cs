using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public async IAsyncEnumerable<KeyValuePair<string, WebSocket>> GetAllAsync()
        {
            foreach (var webSocket in _sockets)
            {
                yield return webSocket;
            }

            await Task.CompletedTask;
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public void AddSocket(WebSocket socket)
        {
            _sockets.TryAdd(CreateConnectionId(), socket);
        }

        public async Task RemoveSocket(string id, CancellationToken stoppingToken)
        {
            _sockets.TryRemove(id, out var socket);

            if (socket != null && (socket.State == WebSocketState.CloseReceived || socket.State == WebSocketState.CloseSent || socket.State == WebSocketState.Open))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the ConnectionManager", stoppingToken);
            }
        }

        private static string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
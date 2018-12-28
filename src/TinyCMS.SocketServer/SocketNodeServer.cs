using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TinyCMS.Interfaces;

namespace TinyCMS
{
    /// <summary>
    /// A server for handling websocket connections for a container
    /// </summary>
    public class SocketNodeServer
    {
        private INodeTypeFactory factory;
        private IContainer container;
        private INodeSerializer serializer;

        private List<SocketConnectionHandler> activeConnections = new List<SocketConnectionHandler>();

        public SocketNodeServer(IContainer container, INodeTypeFactory factory, INodeSerializer serializer)
        {
            this.factory = factory;
            this.container = container;
            this.serializer = serializer;
        }

        public async Task HandleNodeRequest(HttpContext context, WebSocket webSocket)
        {
            var connection = new SocketConnectionHandler(container, webSocket, serializer, factory);
            activeConnections.Add(connection);
            Console.WriteLine("Connection made");
            await connection.ListenForCommands();
            Console.WriteLine("Connection lost");
            activeConnections.Remove(connection);
        }
    }
}

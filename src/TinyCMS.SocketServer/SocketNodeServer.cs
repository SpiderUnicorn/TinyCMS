using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TinyCMS.Base.Security;
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
        private readonly ITokenDecoder tokenValidator;

        private List<SocketConnectionHandler> activeConnections = new List<SocketConnectionHandler>();

        public SocketNodeServer(IContainer container, INodeTypeFactory factory, INodeSerializer serializer, ITokenDecoder tokenValidator)
        {
            this.factory = factory;
            this.container = container;
            this.serializer = serializer;
            this.tokenValidator = tokenValidator;
        }

        public async Task HandleNodeRequest(HttpContext context, WebSocket webSocket)
        {
            var connection = new SocketConnectionHandler(container, webSocket, serializer, factory, tokenValidator);
            activeConnections.Add(connection);
            Console.WriteLine("Connection made");
            await connection.ListenForCommands();
            Console.WriteLine("Connection lost");
            activeConnections.Remove(connection);
        }
    }
}

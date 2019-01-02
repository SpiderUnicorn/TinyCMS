using System;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using TinyCMS.Base.Security;
using TinyCMS.Interfaces;

namespace TinyCMS.SocketServer
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> to add TinyCMS Websocket requests to the request execution pipeline.
    /// </summary>
    public static class SocketServerApplicationBuilderExtensions
    {

        /// <summary>
        /// Adds TintCMS Socket Server to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        public static void UseSocketServer(this IApplicationBuilder app)
        {
            VerifyTinyCmsIsRegistered(app);
            // Using all defaults. Should this be set?
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);

            IServiceProvider services = app.ApplicationServices;

            var container = services.GetService(typeof(IContainer)) as IContainer;
            var nodeTypeFactory = services.GetService(typeof(INodeTypeFactory)) as INodeTypeFactory;
            var nodeSerializer = services.GetService(typeof(INodeSerializer)) as INodeSerializer;
            var tokenValidator = services.GetService(typeof(ITokenDecoder)) as ITokenDecoder;

            SocketNodeServer server = new SocketNodeServer(container, nodeTypeFactory, nodeSerializer, tokenValidator);
            app.Use(async(context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await server.HandleNodeRequest(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });
        }

        private static void VerifyTinyCmsIsRegistered(IApplicationBuilder app)
        {

            if (app.ApplicationServices.GetService(typeof(IContainer)) == null)
            {
                throw new InvalidOperationException("TinyCMS was not registered.");
            }
        }
    }
}

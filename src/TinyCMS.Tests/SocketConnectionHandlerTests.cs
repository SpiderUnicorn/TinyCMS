using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using TinyCMS.Base.Security;
using TinyCMS.Interfaces;
using Xunit;

namespace TinyCMS.Tests
{
    public class SocketConnectionHandlerTests
    {
        [Fact]
        public void can_be_created()
        {
            // Arrange
            var container = Substitute.For<IContainer>();
            var factory = Substitute.For<INodeTypeFactory>();
            var serializer = Substitute.For<INodeSerializer>();
            var decoder = Substitute.For<ITokenDecoder>();

            var socket = Substitute.For<WebSocket>();
            var context = Substitute.For<HttpContext>();

            // Act & Assert
            var handler = new SocketConnectionHandler(
                container,
                socket,
                serializer,
                factory,
                decoder
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NSubstitute;
using TinyCMS.Base.Security;
using TinyCMS.Data;
using TinyCMS.Data.Builder;
using TinyCMS.Data.Extensions;
using TinyCMS.Data.Nodes;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using TinyCMS.Security;
using TinyCMS.Serializer;
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

using System;
using Microsoft.AspNetCore.Builder;
using NSubstitute;
using TinyCMS.SocketServer;
using Xunit;

namespace TinyCMS.Tests
{
    public class SocketServerMiddlewaretests
    {
        [Fact]
        public static void can_register_socket_server_middleware_without_error()
        {
            // Arrange
            var serviceProviderMock = Substitute.For<IServiceProvider>();
            var applicationBuilderMock = Substitute.For<IApplicationBuilder>();

            // Act & Assert
            applicationBuilderMock.UseSocketServer();
        }
    }
}

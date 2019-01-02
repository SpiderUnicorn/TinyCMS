using System;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using NSubstitute;
using TinyCMS.SocketServer;
using Xunit;

namespace TinyCMS.Tests
{
    public class TinyCMSMiddlewareTests
    {
        [Fact]
        public static void using_tiny_cms_throws_if_it_was_not_registered()
        {
            // Arrange
            var serviceProviderMock = Substitute.For<IServiceProvider>();
            var applicationBuilderMock = Substitute.For<IApplicationBuilder>();
            applicationBuilderMock.ApplicationServices.Returns(serviceProviderMock);

            // Act
            Action use = () => applicationBuilderMock.UseTinyCms();

            // Assert
            use.Should().Throw<InvalidOperationException>();
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TinyCMS.Tests
{
    public class ServerTests : IClassFixture<WebApplicationFactory<Startup>>
        {
            private readonly WebApplicationFactory<Startup> _factory;

            public ServerTests(WebApplicationFactory<Startup> factory)
            {
                _factory = factory;
            }

            [Fact]
            public async Task can_call_server()
            {
                // Arrange
                var client = _factory.CreateClient();

                // Act
                var response = await client.GetAsync("/api/shop/cart");

                // Assert
                response.EnsureSuccessStatusCode();
            }

            [Fact]
            public async Task can_get_schema_for_type()
            {
                // Arrange
                var client = _factory.CreateClient();

                // Act
                var response = await client.GetAsync("/api/schema");

                // Assert
                response.EnsureSuccessStatusCode();
            }
        }
}

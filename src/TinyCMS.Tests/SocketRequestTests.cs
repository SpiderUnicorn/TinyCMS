using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using TinyCMS.Data;
using TinyCMS.Data.Builder;
using TinyCMS.Data.Extensions;
using TinyCMS.Data.Nodes;
using TinyCMS.FileStorage;
using TinyCMS.Interfaces;
using Xunit;
using static TinyCMS.RequestTypeEnum;

namespace TinyCMS.Tests
{
    public class SocketRequestTests
    {
        public class parsing_requests
        {
            [Theory]
            [InlineData("+", Add)]
            [InlineData("?", Get)]
            [InlineData("-", Remove)]
            [InlineData("!", Link)]
            [InlineData("=", Update)]
            [InlineData(">", Move)]
            [InlineData("_", Unknown)]
            [InlineData("##", AuthToken)]
            public void first_char_of_request_is_request_type(string requestTypeString, RequestTypeEnum requestTypeEnum)
            {
                // Arrange
                var request = requestTypeString + "foo";

                // Act
                var socketRequest = new SocketRequest(request);

                // Assert
                socketRequest.RequestType.Should().Be(requestTypeEnum);
            }

            [Fact]
            public void get_requests_parses_the_node_id()
            {
                // Arrange
                var request = "?root";

                // Act
                var socketRequest = new SocketRequest(request);

                // Assert
                Assert.Equal("root", socketRequest.Data);
            }

            [Fact]
            public void auth_requests_saves_the_auth_token()
            {
                // Arrange
                var request = "##any-token##";

                // Act
                var socketRequest = new SocketRequest(request);

                // Assert
                socketRequest.Data.Should().Be("any-token");
            }

            [Fact]
            public void add_requests_can_add_json()
            {
                // Arrange
                var request = "+" + "{\"foo\":\"bar\"}";

                // Act
                var socketRequest = new SocketRequest(request);

                // Assert
                socketRequest.Data.Should().Be("{\"foo\":\"bar\"}");
                socketRequest.JsonData.ToString().Should().MatchRegex("foo.*:.*bar");
            }

            [Fact]
            public void any_request_can_have_relations_query_with_required_data()
            {
                // Arrange
                var anyRequestType = "+";
                // note: the request MUST have an object at the end
                var request = anyRequestType + "from=me&neigh&to=you:{en_hest}";

                // Act
                var socketRequest = new SocketRequest(request);

                // Assert
                var query = socketRequest.QueryString;
                query["from"].Should().Be("me");
                query["to"].Should().Be("you");
                query["neigh"].Should().Be("1");
                socketRequest.Data.Should().Be("{en_hest}");
            }

        }
    }
}

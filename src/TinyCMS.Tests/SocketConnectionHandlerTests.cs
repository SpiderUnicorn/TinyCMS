using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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
        public static void hest()
        {
            // Arrange
            var node = new Page { Id = "root" };
            var container = new Container(node);
            var factory = new NodeTypeFactory();
            var serializer = new NodeSerializer(container);
            var settings = new JWTSettings("anykey");
            var decoder = new TokenDecoder(settings);

            var server = new SocketNodeServer(container, factory, serializer, decoder);

        }

    }
}

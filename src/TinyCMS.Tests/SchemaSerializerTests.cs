using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
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
using Xunit.Extensions;

namespace TinyCMS.Tests
{
    public class SchemaSerializerTests
    {
        [Fact]
        public void includes_properties_of_given_type()
        {
            // Arrange
            var serializer = new SchemaSerializer();

            // Act
            var result = GetSchema(serializer, typeof(INode));

            // Assert
            result.Should().Contain("id")
                .And.Contain("parentId")
                .And.Contain("type");
        }

        private string GetSchema(SchemaSerializer serializer, Type type)
        {
            var output = new MemoryStream();
            serializer.StreamSchema(typeof(INode), output);
            StreamReader reader = new StreamReader(output);
            output.Seek(0, SeekOrigin.Begin);
            return reader.ReadToEnd();
        }
    }
}

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

namespace TinyCMS.Tests
{
    public class NodeSerializerTests
    {
        private class TestNode : BaseNode
        {
            public override string Type => "TestNode";

            public TestNode(string id)
            {
                Id = id;
            }
        }

        [Fact]
        public void given_no_node_it_creates_empty_object()
        {
            // Arrange
            var serializer = new NodeSerializer(null, null);
            var output = new MemoryStream();

            // Act
            serializer.StreamSerialize(null, null, output);
            var res = Encoding.ASCII.GetString(output.ToArray());

            // Assert
            res.Should().Be("{}");
        }

        [Fact(Skip = "How should this work?")]
        public void given_no_token_it_creates_empty_object()
        {
            // Arrange
            var jwtSettings = new JWTSettings("any key");
            var tokenDecoder = new TokenDecoder(jwtSettings);
            var serializer = new NodeSerializer(null, tokenDecoder);
            var output = new MemoryStream();

            // Act
            serializer.StreamSerialize(new TestNode("foo"), null, output);
            var res = Encoding.ASCII.GetString(output.ToArray());

            // Assert
            res.Should().Be("{}");
        }

        [Fact]
        public void without_parsing_relations_it_parses_test_node()
        {
            // Arrange
            var serializer = GetSerializer();
            var output = new MemoryStream();

            // Act
            serializer.StreamSerialize(new TestNode("foo"), null, output, 99, 0, fetchRelations : false);
            var res = Encoding.ASCII.GetString(output.ToArray());

            // Assert
            res.Should().Be("{\"id\":\"foo\",\"type\":\"TestNode\"}");
        }

        [Fact]
        public void with_parsing_relations_it_parses_test_node()
        {
            // Arrange
            var node = new TestNode("foo");
            var container = new Container(node);
            var serializer = GetSerializer(container);
            var output = new MemoryStream();

            // Act
            serializer.StreamSerialize(node, null, output, 99, 0, fetchRelations : true);
            var res = Encoding.ASCII.GetString(output.ToArray());

            // Assert
            res.Should().Be("{\"id\":\"foo\",\"type\":\"TestNode\"}");
        }

        [Fact]
        public void parses_a_child()
        {
            // Arrange
            var root = new TestNode("foo");
            var child = new TestNode("bar");
            root.Add(child);
            var container = new Container(root);
            var serializer = GetSerializer(container);
            var output = new MemoryStream();

            // Act
            serializer.StreamSerialize(root, null, output, 99, 0, fetchRelations : true);
            var res = Encoding.ASCII.GetString(output.ToArray());

            // Assert
            var json = JsonConvert.DeserializeObject<Deserialized>(res);
            json.id.Should().Be("foo");
            json.children[0].parentId.Should().Be("foo");
            json.children[0].id.Should().Be("bar");
        }

        [Fact]
        public void parses_children()
        {
            // Arrange
            var root = new TestNode("foo")
                .Add(new TestNode("bar"))
                .Add(new TestNode("baz"));

            var container = new Container(root);
            var serializer = GetSerializer(container);
            var output = new MemoryStream();

            // Act
            serializer.StreamSerialize(root, null, output, 99, 0, fetchRelations : true);
            var res = Encoding.ASCII.GetString(output.ToArray());

            // Assert
            var json = JsonConvert.DeserializeObject<Deserialized>(res);
            json.children.Count.Should().Be(2);
        }

        [Fact]
        public void hasRelationsHest()
        {
            // Arrange
            var root = new TestNode("foo")
                .Add(new TestNode("bar"))
                .Add(new TestNode("baz"));

            var container = new Container(root);
            container.AddRelation(container.GetById("foo"), container.GetById("baz"));

            var serializer = GetSerializer(container);
            var output = new MemoryStream();

            // Act
            serializer.StreamSerialize(root, null, output, 99, 0, fetchRelations : true);
            var res = Encoding.ASCII.GetString(output.ToArray());

            // Assert
            var json = JsonConvert.DeserializeObject<Deserialized>(res);
            json.relations.Count.Should().Be(1);
        }

        private NodeSerializer GetSerializer(IContainer container = null)
        {
            var jwtSettings = new JWTSettings("any key");
            var tokenDecoder = new TokenDecoder(jwtSettings);
            return new NodeSerializer(container, tokenDecoder);
        }

        private class Deserialized
        {
            public string id;
            public string parentId;
            public List<Deserialized> children;
            public List<Deserialized> relations;
        }
    }
}

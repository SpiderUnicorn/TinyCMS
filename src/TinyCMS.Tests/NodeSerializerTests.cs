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

        public class parsing_different_data_types
        {
            private enum TestEnum
            {
                A,
                B
            }
            private class TestDataTypesNode : BaseNode
            {
                public override string Type => "TestDataTypesNode";

                public string StringProp { get; set; }
                public bool BoolProp { get; set; }
                public DateTime DateTimeProp { get; set; }
                public TestEnum EnumProp { get; set; }
                public int IntProp { get; set; }
                public float FloatProp { get; set; }
                public double DoubleProp { get; set; }
                public Dictionary<string, object> DictionaryProp { get; set; }
                public IEnumerable<string> EnumerableProp { get; set; }
                public IEnumerable<INode> EnumerableNodeProp { get; set; }
            }

            [Fact]
            public void parses_strings()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "stubId", StringProp = "foo" };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("stringProp");
            }

            [Fact]
            public void parses_bool()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "stubId", BoolProp = true };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("boolProp");
            }

            [Fact]
            public void parses_dateTime()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "stubId", DateTimeProp = DateTime.Now };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("dateTimeProp");
            }

            [Fact]
            public void parses_enum()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "stubId", EnumProp = TestEnum.A };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("enumProp");
            }

            [Fact]
            public void parses_int()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "stubId", IntProp = 42 };

                /// Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("intProp");
            }

            [Fact]
            public void parses_float()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "stubId", FloatProp = 10.5f };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("floatProp");
            }

            [Fact]
            public void parses_double()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "stubId", DoubleProp = 11.5 };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("doubleProp");
            }

            [Fact]
            public void parses_dictionaries()
            {
                // Arrange
                var dictionary = new Dictionary<string, object>
                    {
                        ["foo"] = "bar",
                        ["baz"] = "buz"
                    };
                var node = new TestDataTypesNode { Id = "stubId", DictionaryProp = dictionary };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("dictionaryProp");
            }

            [Fact]
            public void parses_enumerable()
            {
                // Arrange
                var enumerable = new [] { "foo", "bar" };
                var node = new TestDataTypesNode { Id = "stubId", EnumerableProp = enumerable };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("enumerableProp");
            }

            [Fact(Skip = "Why does this not work?")]
            public void parses_enumerable_of_nodes()
            {
                // Arrange
                var enumerable = new [] { new TestNode("foo") };
                var node = new TestDataTypesNode { Id = "stubId", EnumerableNodeProp = enumerable };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("enumerableNodeProp");
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
            var root = new TestNode("foo");

            // Act
            var result = Serialize(root);

            // Assert
            result.Should().Be("{\"id\":\"foo\",\"type\":\"TestNode\"}");
        }

        [Fact]
        public void with_parsing_relations_it_parses_test_node()
        {
            // Arrange
            var node = new TestNode("foo");
            var container = new Container(node);

            // Act
            var result = Serialize(node, container);

            // Assert
            result.Should().Be("{\"id\":\"foo\",\"type\":\"TestNode\"}");
        }

        [Fact]
        public void parses_a_child()
        {
            // Arrange
            var root = new TestNode("foo");
            var child = new TestNode("bar");
            root.Add(child);
            var container = new Container(root);

            // Act
            var result = Serialize(root, container);

            // Assert
            var json = JsonConvert.DeserializeObject<Deserialized>(result);
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

            // Act
            var result = Serialize(root, container);

            // Assert
            var json = JsonConvert.DeserializeObject<Deserialized>(result);
            json.children.Count.Should().Be(2);
        }

        [Fact]
        public void parses_node_with_null_children()
        {
            // Arrange
            var root = new TestNode("foo");

            // Act
            root.Children = null;
            var result = Serialize(root);

            // Assert
            result.Should().Be("{\"id\":\"foo\",\"type\":\"TestNode\"}");
        }

        [Fact]
        public void parses_single_relation()
        {
            // Arrange
            var root = new TestNode("foo")
                .Add(new TestNode("bar"))
                .Add(new TestNode("baz"));

            var container = new Container(root);
            container.AddRelation(container.GetById("foo"), container.GetById("baz"));

            // Act
            var result = Serialize(root, container, fetchRelations : true);

            // Assert
            var json = JsonConvert.DeserializeObject<Deserialized>(result);
            json.relations.Count.Should().Be(1);
        }

        [Fact]
        public void parses_multiple_relations()
        {
            // Arrange
            var root = new TestNode("foo")
                .Add(new TestNode("bar"))
                .Add(new TestNode("baz"));

            var container = new Container(root);
            container.AddRelation(container.GetById("foo"), container.GetById("bar"));
            container.AddRelation(container.GetById("foo"), container.GetById("baz"));

            // Act
            var result = Serialize(root, container, fetchRelations : true);

            // Assert
            var json = JsonConvert.DeserializeObject<Deserialized>(result);
            json.relations.Count.Should().Be(2);
        }

        private static string Serialize(INode node, IContainer container = null, bool fetchRelations = false)
        {
            var serializer = NodeSerializerTests.GetSerializer(container);
            var output = new MemoryStream();
            serializer.StreamSerialize(node, null, output, 99, 0, fetchRelations : fetchRelations);
            return Encoding.ASCII.GetString(output.ToArray());
        }

        private static NodeSerializer GetSerializer(IContainer container = null)
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

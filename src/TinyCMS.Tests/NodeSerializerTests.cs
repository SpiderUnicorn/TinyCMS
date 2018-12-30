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
                // Two unhandled properties to test parsing unknowns
                public char CharProp { get; set; }
                public StubComplexType ComplexProp { get; set; }
            }

            private class StubComplexType
            {
                public int A { get; set; }
                public string B { get; set; }
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
            public void parses_null_value_by_removing_the_key()
            {
                // Arrange
                var node = new TestDataTypesNode { Id = "foo" };
                node.StringProp = null;

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().NotContain("stringProp");
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
            public void parses_enums()
            {
                // Arrange
                var enumerable = new [] { "foo", "bar" };
                var node = new TestDataTypesNode { Id = "stubId", EnumerableProp = enumerable };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("enumerableProp");
            }

            [Fact]
            public void parses_complex_type()
            {
                // Arrange
                var complexType = new StubComplexType { A = 42, B = "foo" };
                var node = new TestDataTypesNode { Id = "stubId", ComplexProp = complexType };

                // Act
                var result = Serialize(node);

                // Assert
                result.Should().Contain("complexProp");
                result.Should().Contain("42");
                result.Should().Contain("foo");
            }

            [Fact]
            public void parses_complex_type_ignoring_null_value()
            {
                // Arrange
                var complexType = new StubComplexType { A = 42, B = null };
                var node = new TestDataTypesNode { Id = "stubId", ComplexProp = complexType };

                // Act
                var result = Serialize(node);

                // Assert
                var json = JsonConvert.DeserializeObject<StubComplexType>(result);
                json.B.Should().BeNull();
            }

            [Fact]
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
            INode noNode = null;

            // Act
            var result = Serialize(noNode);

            // Assert
            result.Should().Be("{}");
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
            if (container == null)
            {
                container = new Container();
            }
            var serializer = NodeSerializerTests.GetSerializer(container);
            var arraySegment = serializer.ToArraySegment(node, fetchRelations : fetchRelations);
            return Encoding.ASCII.GetString(arraySegment);
        }

        private static NodeSerializer GetSerializer(IContainer container = null) => new NodeSerializer(container);

        private class Deserialized
        {
            public string id;
            public string parentId;
            public List<Deserialized> children;
            public List<Deserialized> relations;
        }
    }
}

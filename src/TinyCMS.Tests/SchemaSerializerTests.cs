using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using TinyCMS.Tests.Extensions;
using Xunit;
using Xunit.Extensions;

namespace TinyCMS.Tests
{
    public class SchemaSerializerTests
    {
        [Description("included-class-description")]
        private class TypeWithProperties
        {
            public int Foo { get; set; }

            [EditorType("included-editor-type-property")]

            [Description("not-included-property-description")]
            public string Bar { get; set; }
        }

        private class WithSchemaAttribute
        {
            [SchemaType("included-schema-type-property")]
            public int Foo { get; set; }
        }

        private class Schema
        {
            public string id = "http://tinycms.com/schema/typeWithProperties.schema.json";
            public string type = "object";

            public Dictionary<string, dynamic> properties;
        }

        private struct PropertyType
        {
            private string type;
            public PropertyType(string type)
            {
                this.type = type;
            }
        }

        [Fact]
        public void includes_properties_of_given_type()
        {
            // Arrange
            var expected = new Dictionary<string, PropertyType>
                {
                    ["foo"] = new PropertyType("int32"),
                    ["bar"] = new PropertyType("string")
                };

            // Act
            var result = GetSchema<TypeWithProperties>().ToJson<Schema>();

            // Assert
            result.properties.Should().ContainKeys("foo", "bar");
        }

        [Fact]
        public void includes_description_attribute_on_types()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<TypeWithProperties>();

            // Assert
            result.Should().Contain("included-class-description");
        }

        [Fact]
        public void does_not_include_description_attribute_on_values()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<TypeWithProperties>();

            // Assert
            result.Should().NotContain("not-included-property-description");
        }

        [Fact]
        public void includes_editor_type_attribute_on_values()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<TypeWithProperties>();

            // Assert
            result.Should().Contain("included-editor-type-property");
        }

        [Fact]
        public void includes_schema_type_attribute_on_values()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<WithSchemaAttribute>();

            // Assert
            result.Should().Contain("included-schema-type-property");
        }

        private string GetSchema<T>()
        {
            var serializer = new SchemaSerializer();
            var output = new MemoryStream();
            serializer.StreamSchema(typeof(T), output);
            StreamReader reader = new StreamReader(output);
            output.Seek(0, SeekOrigin.Begin);
            return reader.ReadToEnd();
        }
    }

}

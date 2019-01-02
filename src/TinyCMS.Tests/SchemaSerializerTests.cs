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

        private class SimpleType
        {
            public string foo { get; set; }
            public string bar { get; set; }
        }

        [Fact]
        public void includes_properties_of_a_type()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<SimpleType>().ToJson<Schema>();

            // Assert
            result.properties.Should().ContainKeys("foo", "bar");
        }

        [Description("included-class-description")]
        private class TypeWithDescriptionAttribute { }

        [Fact]
        public void includes_description_attribute_on_types()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<TypeWithDescriptionAttribute>();

            // Assert
            result.Should().Contain("included-class-description");
        }

        private class TypeWithDescriptionOnProperty
        {
            [Description("not-included-property-description")]
            public string foo { get; set; }
        }

        [Fact]
        public void does_not_include_description_attribute_on_values()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<TypeWithDescriptionOnProperty>();

            // Assert
            result.Should().NotContain("not-included-property-description");
        }

        private class TypeWithEditorTypeOnProperty
        {

            [EditorType("included-editor-type-property")]

            public string AnyProperty { get; set; }
        }

        [Fact]
        public void includes_editor_type_attribute_on_values()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<TypeWithEditorTypeOnProperty>();

            // Assert
            result.Should().Contain("included-editor-type-property");
        }

        private class WithSchemaAttribute
        {
            [SchemaType("included-schema-type-property", false)]
            public int Foo { get; set; }
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

        [Fact]
        public void schema_type_attribute_not_a_URI_will_be_prefixed()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<WithSchemaAttribute>();

            // Assert
            result.Should().Contain("$ref\":\"" + SchemaTypeAttribute.SCHEMA_PREFIX);
        }

        private class WithSchemaAttributeAsURI
        {
            [SchemaType("included-schema-type-property", true)]
            public int Foo { get; set; }
        }

        [Fact]
        public void schema_type_attribute_with_a_URI_will_not_be_prefixed()
        {
            // Arrange
            // no op

            // Act
            var result = GetSchema<WithSchemaAttributeAsURI>();

            // Assert
            result.Should().NotContain("$ref\":\"" + SchemaTypeAttribute.SCHEMA_PREFIX);
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

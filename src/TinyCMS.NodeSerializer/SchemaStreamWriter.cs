using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TinyCMS.Data;
using TinyCMS.Data.Extensions;
using TinyCMS.Interfaces;
using static TinyCMS.Serializer.JsonTypeNotation;

namespace TinyCMS.Serializer
{
    /// <summary>
    /// Specialized StreamWriter for writing a Schema to a stream
    /// </summary>
    public class SchemaStreamWriter : JsonStreamWriter
    {
        public SchemaStreamWriter(Stream stream) : base(stream) { }

        public void WriteSchema(Type type)
        {
            Write(ObjectStart);
            WriteKeyAndValue("id", SchemaTypeAttribute.SCHEMA_PREFIX + type.Name.ToLowerFirst() + ".schema.json");
            if (type.GetCustomAttribute(typeof(System.ComponentModel.DescriptionAttribute)) is System.ComponentModel.DescriptionAttribute description)
            {
                Write(Comma);
                WriteKeyAndValue("description", description.Description);
            }
            Write(Comma);
            WriteKeyAndValue("type", "object");
            Write(Comma);
            WriteKey("properties");
            Write(ObjectStart);
            WriteSchemaProperties(type);
            Write(ObjectEnd);
            Write(ObjectEnd);
        }

        public void WriteSchemaProperties(Type type)
        {
            var isFirst = true;
            foreach (var property in type.GetProperties().Where(d => d.Name != "IsParsed"))
            {
                if (!isFirst)
                {
                    Write(Comma);
                }
                WriteKey(property.Name.ToLowerFirst());
                Write(ObjectStart);
                var keyAndValue = GetTypeName(property);
                WriteKeyAndValue(keyAndValue.Item1, keyAndValue.Item2);

                if (property.GetCustomAttribute(editorType) is EditorTypeAttribute editorAttr)
                {
                    Write(Comma);
                    WriteKeyAndValue("editor", editorAttr.Editor);
                }
                Write(ObjectEnd);
                isFirst = false;
            }
        }

        private static Type schemaType = typeof(SchemaTypeAttribute);
        private static Type editorType = typeof(EditorTypeAttribute);

        private Tuple<string, string> GetTypeName(PropertyInfo propertyInfo)
        {
            var key = "type";
            var value = propertyInfo.PropertyType.Name.ToLowerFirst();
            if (propertyInfo.GetCustomAttribute(schemaType) is SchemaTypeAttribute customSchema)
            {
                key = "$ref";
                value = customSchema.Schema;
            }
            return Tuple.Create<string, string>(key, value);
        }
    }
}

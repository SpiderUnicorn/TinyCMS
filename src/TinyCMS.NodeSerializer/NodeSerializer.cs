using System;
using System.Collections;
using System.IO;
using System.Text;
using TinyCMS.Data;
using TinyCMS.Data.Builder;
using TinyCMS.Data.Extensions;
using TinyCMS.Interfaces;
using TinyCMS.Serializer;

namespace TinyCMS.Serializer
{

    public class NodeSerializer : INodeSerializer
    {
        private MemoryStream stream;
        private JsonNodeStreamWriter output;
        public NodeSerializer(IContainer container)
        {
            stream = new MemoryStream();
            output = new JsonNodeStreamWriter(stream, container);
        }
        public ArraySegment<byte> ToArraySegment(INode node, bool fetchRelations = true)
        {

            output.WriteNode(node, fetchRelations);
            var arraySegment = new ArraySegment<byte>();
            output.Flush();
            stream.TryGetBuffer(out arraySegment);
            return arraySegment;
        }

        /*
        public ArraySegment<byte> ToArraySegment(INode node, string token, ISerializerSettings settings)
        {
            return ToArraySegment(node, token, settings.Depth, settings.Level, settings.IncludeRelations);
        }
        */

        /*
        public void StreamSchema(Type type, string token, Stream output)
        {
            output.WriteByte(ObjectStart);
            WriteKeyAndValue(output, token, "id", SchemaTypeAttribute.SCHEMA_PREFIX + type.Name.ToLowerFirst() + ".schema.json");
            if (type.GetCustomAttribute(typeof(System.ComponentModel.DescriptionAttribute)) is System.ComponentModel.DescriptionAttribute description)
            {
                output.WriteByte(CommaByte);
                WriteKeyAndValue(output, token, "description", description.Description);
            }
            output.WriteByte(CommaByte);
            WriteKeyAndValue(output, token, "type", "object");
            output.WriteByte(CommaByte);
            WriteKey(output, "properties");
            output.WriteByte(ObjectStart);
            WriteSchemaProperties(type, output);
            output.WriteByte(ObjectEnd);
            output.WriteByte(ObjectEnd);
        }

        private void WriteSchemaProperties(Type type, Stream output)
        {
            var isFirst = true;
            foreach (var property in type.GetProperties().Where(d => d.Name != "IsParsed"))
            {
                if (!isFirst)
                {
                    output.WriteByte(CommaByte);
                }
                WriteKey(output, property.Name.ToLowerFirst());
                output.WriteByte(ObjectStart);
                var keyAndValue = GetTypeName(property);
                WriteKeyAndValue(output, "", keyAndValue.Item1, keyAndValue.Item2);

                if (property.GetCustomAttribute(editorType) is EditorTypeAttribute editorAttr)
                {
                    output.WriteByte(CommaByte);
                    WriteKeyAndValue(output, "", "editor", editorAttr.Editor);
                }
                output.WriteByte(ObjectEnd);
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
        */
    }
}

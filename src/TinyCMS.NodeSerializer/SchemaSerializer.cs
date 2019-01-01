using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TinyCMS.Data;
using TinyCMS.Data.Builder;
using TinyCMS.Data.Extensions;
using TinyCMS.Interfaces;
using TinyCMS.Serializer;

namespace TinyCMS.Serializer
{

    public class SchemaSerializer
    {
        public void StreamSchema(Type type, Stream stream)
        {
            var output = new SchemaStreamWriter(stream);
            output.WriteSchema(type);
            output.Flush();
        }

        public void StreamTypes(Stream stream, IEnumerable<string> typeNames)
        {
            var output = new NodeStreamWriter(stream, new Container());
            output.WriteValue(typeNames);
        }
    }
}

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

    public class SchemaSerializer
    {
        public void StreamSchema(Type type, Stream stream)
        {
            var output = new SchemaStreamWriter(stream);
            output.WriteSchema(type);
            output.Flush();
        }
    }
}

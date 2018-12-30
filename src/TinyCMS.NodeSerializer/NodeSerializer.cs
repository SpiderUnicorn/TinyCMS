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
    }
}

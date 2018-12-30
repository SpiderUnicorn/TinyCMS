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
        private IContainer container;

        public NodeSerializer(IContainer container)
        {
            this.container = container;
        }
        public ArraySegment<byte> ToArraySegment(INode node, bool fetchRelations = true)
        {
            if (container == null)
            {
                container = new Container(node);
            }
            var stream = new MemoryStream();
            var output = new NodeStreamWriter(stream, container);
            output.WriteNode(node, fetchRelations);
            var arraySegment = new ArraySegment<byte>();
            output.Flush();
            stream.TryGetBuffer(out arraySegment);
            return arraySegment;
        }
    }
}

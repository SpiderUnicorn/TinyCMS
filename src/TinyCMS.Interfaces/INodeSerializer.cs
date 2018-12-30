using System;
using System.IO;
using TinyCMS.Interfaces;

namespace TinyCMS.Interfaces
{
    public interface INodeSerializer
    {
        /*
        void StreamSchema(Type type, string token, Stream output);
        void StreamSerialize(INode node, string token, Stream output, int depth = 99, int level = 0, bool fetchRelations = true, params string[] excludedProperties);
        */
        ArraySegment<byte> ToArraySegment(INode node, bool fetchRelations = true);
        // ArraySegment<byte> ToArraySegment(INode node, string token, ISerializerSettings settings);
    }
}

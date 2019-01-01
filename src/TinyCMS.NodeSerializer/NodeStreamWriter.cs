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
    /// Specialized StreamWriter for writing INode to a stream
    /// </summary>
    public class NodeStreamWriter : JsonStreamWriter
    {
        public IContainer Container { get; }

        public NodeStreamWriter(Stream stream, IContainer container) : base(stream)
        {
            Container = container;
        }

        /// <summary>
        /// Writes a node structure to the stream.
        /// Optionally includes relations.
        ///
        /// Maximum depth for relations is 2 nodes down.
        /// </summary>
        /// <param name="node">Node to serialize</param>
        /// <param name="shouldIncludeRelations">Toggle if relations should be serialized</param>
        /// <param name="excludedProperties">Properties to not serialize</param>
        public void WriteNode(INode node, bool shouldIncludeRelations = true, params string[] excludedProperties)
        {
            // delegate to recursive function to keep the public WriteNode free from the level property
            WriteNode(node, 0, true, excludedProperties);
        }

        private void WriteNode(INode node, int level = 0, bool shouldIncludeRelations = true, params string[] excludedProperties)
        {
            Write(ObjectStart);
            if (node is null)
            {
                Write(ObjectEnd);
                return;
            }

            WriteNodeProperties(node);

            if (node.HasChildren())
            {
                Write(Comma);
                WriteKey("children");
                WriteArray(node.Children, child => WriteNode(child, level + 1, level < 2, excludedProperties));
            }
            if (shouldIncludeRelations)
            {
                var relations = Container.GetRelations(node);
                if (relations.Any())
                {
                    Write(Comma);
                    WriteKey("relations");
                    WriteArray(relations, child => WriteNode(child, level + 1, false, excludedProperties));
                }
            }

            var extraProps = node.GetPropertyDictionary(excludedProperties);
            bool hasExtra = extraProps.Keys.Any();
            if (hasExtra)
            {
                Write(Comma);
                WriteCommaSeparated(extraProps, WriteKeyAndValue);
            }

            Write(ObjectEnd);
        }

        private void WriteNodeProperties(INode node)
        {
            WriteKeyAndValue(nameof(node.Id).ToLowerFirst(), node.Id);
            Write(Comma);
            WriteKeyAndValue(nameof(node.Type).ToLowerFirst(), node.Type);

            if (!string.IsNullOrEmpty(node.ParentId))
            {
                Write(Comma);
                WriteKeyAndValue(nameof(node.ParentId).ToLowerFirst(), node.ParentId);
            }
        }

        public override void WriteValue(object value)
        {
            if (value is INode node)
            {
                WriteNode(node, 2);
            }
            else
            {
                base.WriteValue(value);
            }
        }
    }
}

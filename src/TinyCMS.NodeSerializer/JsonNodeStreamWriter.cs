using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinyCMS.Interfaces;
using static TinyCMS.Serializer.JsonTypeNotation;

namespace TinyCMS.Serializer
{
    /// <summary>
    /// Specialized StreamWriter for writing INode to a stream
    /// </summary>
    public class JsonNodeStreamWriter : StreamWriter
    {
        public IContainer Container { get; }

        public JsonNodeStreamWriter(Stream stream, IContainer container) : base(stream)
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

            var nodePropertiesToSerialize = new Dictionary<string, string>
                {
                    ["id"] = node.Id,
                    ["type"] = node.Type
                };

            if (!string.IsNullOrEmpty(node.ParentId))
            {
                nodePropertiesToSerialize.Add("parentId", node.ParentId);
            }

            WriteStringDictionary(nodePropertiesToSerialize);

            if (HasChildren(node))
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

        /// <summary>
        /// Does appropriate type checks on the value and calls
        /// a matching write function.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        private void WriteValue(object value)
        {
            switch (value)
            {
                case INode node:
                    WriteNode(node, 2);
                    break;
                case string valueString:
                    WriteString(EscapeString(valueString));
                    break;
                case bool b:
                    WriteBool(b);
                    break;
                case DateTime dateTime:
                    WriteDateTime(dateTime);
                    break;
                case Enum @enum:
                    WriteEnum(@enum);
                    break;
                case int intNumber:
                    WriteNumber(intNumber);
                    break;
                case float floatNumber:
                    WriteNumber(floatNumber);
                    break;
                case double doubleNumber:
                    WriteNumber(doubleNumber);
                    break;
                case Dictionary<string, object> dictionary:
                    WriteObject(dictionary);
                    break;
                case IEnumerable<object> array:
                    WriteArray<object>(array, WriteValue);
                    break;
                default:
                    WriteObject(value.GetPropertyDictionary());
                    break;
            }
        }

        private string EscapeString(string value)
        {
            return value
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\"", "\\\"");
        }

        private void WriteKeyAndValue(string key, object value)
        {
            WriteKey(key);
            WriteValue(value);
        }

        private void WriteString(string value)
        {
            Write(Quote);
            Write(value);
            Write(Quote);
        }

        private void WriteBool(bool value) => Write(value ? True : False);
        private void WriteDateTime(DateTime dateTime) => Write(dateTime.Ticks.ToString());
        private void WriteEnum(Enum @enum) => WriteString(@enum.ToString());
        private void WriteNumber(int value) => Write(value.ToString());
        private void WriteNumber(float value) => Write(value.ToString());
        private void WriteNumber(double value) => Write(value.ToString());
        private void WriteKeyValueStrings(string key, string value)
        {
            WriteKey(key);
            WriteString(value);
        }

        private void WriteKey(string key)
        {
            WriteString(key);
            Write(Colon);
        }
        private void WriteArray<T>(IEnumerable<T> values, Action<T> fn)
        {
            Write(ArrayStart);
            WriteCommaSeparated(values, fn);
            Write(ArrayEnd);
        }

        private void WriteCommaSeparated<T>(IEnumerable<T> values, Action<T> fn)
        {
            IterateCommaSeparated(values.GetEnumerator(), fn);
        }

        private void IterateCommaSeparated<T>(IEnumerator<T> enumerator, Action<T> fn)
        {
            var isFirst = true;
            while (enumerator.MoveNext())
            {
            if (!isFirst)
            {
            Write(Comma);
                }
                fn(enumerator.Current);
                isFirst = false;
            }
        }

        private void WriteCommaSeparated<TKey, TValue>(Dictionary<TKey, TValue> values, Action<TKey, TValue> fn)
        {
            IterateCommaSeparated(values.GetEnumerator(), entry => fn(entry.Key, entry.Value));
        }

        private void WriteStringDictionary(Dictionary<string, string> dictionary)
        {
            WriteCommaSeparated(dictionary, WriteKeyValueStrings);
        }

        private void WriteObject(Dictionary<string, object> propertyDictionary)
        {
            Write(ObjectStart);
            WriteCommaSeparated(propertyDictionary, WriteKeyAndValue);
            Write(ObjectEnd);
        }

        private bool HasChildren(INode node) => node.Children?.Any() ?? false;
    }
}

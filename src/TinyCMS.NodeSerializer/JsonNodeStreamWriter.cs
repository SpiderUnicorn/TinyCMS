using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinyCMS.Interfaces;
using static TinyCMS.Serializer.JsonTypeNotation;

namespace TinyCMS.Serializer
{
    public class JsonNodeStreamWriter : StreamWriter
    {
        public IContainer Container { get; }

        public JsonNodeStreamWriter(Stream stream, IContainer container) : base(stream)
        {
            Container = container;
        }
        public void WriteValue(object value)
        {
            if (value is INode node)
            {
                WriteNode(node, 2);
            }
            else if (value is string valueString)
            {
                WriteString(EscapeString(valueString));
            }
            else if (value is bool b)
            {
                WriteBool(b);
            }
            else if (value is DateTime dateTime)
            {
                WriteDateTime(dateTime);
            }
            else if (value is Enum @enum)
            {
                WriteEnum(@enum);
            }
            else if (value is int intNumber)
            {
                WriteNumber(intNumber);
            }
            else if (value is float floatNumber)
            {
                WriteNumber(floatNumber);
            }
            else if (value is double doubleNumber)
            {
                WriteNumber(doubleNumber);
            }
            else if (value is Dictionary<string, object> dictionary)
            {
                Write(ObjectStart);
                WriteCommaSeparated(dictionary, WriteKeyAndValue);
                Write(ObjectEnd);
            }
            else if (value is IEnumerable<object> array)
            {
                WriteArray<object>(array, WriteValue);
            }
            else
            {
                Write(ObjectStart);
                WriteCommaSeparated(value.GetPropertyDictionary(), WriteKeyAndValue);
                Write(ObjectEnd);
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

        public void WriteKeyAndValue(string key, object value)
        {
            WriteKey(key);
            WriteValue(value);
        }

        public void WriteString(string value)
        {
            Write(Quote);
            Write(value);
            Write(Quote);
        }

        public void WriteBool(bool value) => Write(value ? True : False);
        public void WriteDateTime(DateTime dateTime) => Write(dateTime.Ticks.ToString());
        public void WriteEnum(Enum @enum) => WriteString(@enum.ToString());
        public void WriteNumber(int value) => Write(value.ToString());
        public void WriteNumber(float value) => Write(value.ToString());
        public void WriteNumber(double value) => Write(value.ToString());
        public void WriteKeyValueStrings(string key, string value)
        {
            WriteKey(key);
            WriteString(value);
        }

        public void WriteKey(string key)
        {
            WriteString(key);
            Write(Colon);
        }
        public void WriteArray<T>(IEnumerable<T> values, Action<T> fn)
        {
            Write(ArrayStart);
            WriteCommaSeparated(values, fn);
            Write(ArrayEnd);
        }

        public void WriteCommaSeparated<T>(IEnumerable<T> values, Action<T> fn)
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

        public void WriteCommaSeparated<TKey, TValue>(Dictionary<TKey, TValue> values, Action<TKey, TValue> fn)
        {
            IterateCommaSeparated(values.GetEnumerator(), entry => fn(entry.Key, entry.Value));
        }

        public void WriteStringDictionary(Dictionary<string, string> dictionary)
        {
            WriteCommaSeparated(dictionary, WriteKeyValueStrings);
        }

        public void WriteNode(INode node, bool fetchRelations = true, params string[] excludedProperties)
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

        private bool HasChildren(INode node) => node.Children?.Any() ?? false;
    }
}

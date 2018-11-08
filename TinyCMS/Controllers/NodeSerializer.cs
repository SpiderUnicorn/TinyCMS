﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using TinyCMS.Data;
using TinyCMS.Data.Builder;

namespace TinyCMS.Controllers
{
    public static class Helper
    {
        public static string ToLowerFirst(this string s)
        {
            return Char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
        private static string[] NODE_PROPERTIES = { "Id", "ParentId", "Children", "Tags", "Type", "IsParsed" };

        private static Dictionary<Type, Dictionary<string, PropertyInfo>> props =
            new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        public static Dictionary<string, object> GetProperties(this object o, params string[] excludedProperties)
        {
            Dictionary<string, PropertyInfo> dict = null;
            var ret = new Dictionary<string, object>();
            var t = o.GetType();
            if (props.ContainsKey(t))
                dict = props[t];
            else
            {
                dict = new Dictionary<string, PropertyInfo>();
                var isNode = o is INode;
                var prps = t.GetProperties().Where(d => d.CanRead);
                if (isNode)
                    prps = prps.Where(d => !NODE_PROPERTIES.Contains(d.Name));
                if (isNode && excludedProperties.Any())
                    prps = prps.Where(d => !excludedProperties.Contains(d.Name));
                foreach (var prp in prps)
                {
                    var key = prp.Name.ToLowerFirst();
                    dict.Add(key, prp);
                }
                props.Add(t, dict);
            }
            foreach (var kv in dict)
            {
                var val = kv.Value.GetValue(o, null);
                if (val != null)
                {
                    ret.Add(kv.Key, val);
                }
            }
            return ret;
        }
    }

    public class NodeSerializer
    {
        private Container _container;
        public NodeSerializer(Container cont)
        {
            _container = cont;
        }

        private const byte FnuttByte = (byte)'"';
        private const byte ColonByte = (byte)':';
        private const byte TrueByte = (byte)'1';
        private const byte FalseByte = (byte)'0';
        private const byte CommaByte = (byte)',';
        private const byte ObjectStart = (byte)'{';
        private const byte ObjectEnd = (byte)'}';
        private const byte ArrayStart = (byte)'[';
        private const byte ArrayEnd = (byte)']';

        public ArraySegment<byte> ToArraySegment(INode node, int depth = 99, int level = 0, bool fetchRelations = true)
        {
            var stream = new MemoryStream();
            StreamSerialize(node, stream, depth, level, fetchRelations);
            var ret = new ArraySegment<byte>();
            stream.TryGetBuffer(out ret);
            return ret;
        }

        public ArraySegment<byte> ToArraySegment(INode node, ISerializerSettings settings)
        {
            return ToArraySegment(node, settings.Depth, settings.Level, settings.IncludeRelations);

        }

        public void StreamSerialize(INode node, Stream output, int depth = 99, int level = 0, bool fetchRelations = true, params string[] excludedProperties)
        {
            output.WriteByte(ObjectStart);
            if (node != null)
            {
                WriteKey(output, "id", node.Id);
                output.WriteByte(CommaByte);
                WriteKey(output, "type", node.Type);
                bool hasChildren = node.Children != null && node.Children.Any();
                bool useParentId = !string.IsNullOrEmpty(node.ParentId) && level < 1;
                bool hasTags = node.Tags != null && node.Tags.Any();
                var extraPrps = node.GetProperties(excludedProperties);
                bool hasRelations = fetchRelations;
                IEnumerable<INode> relations = null;
                if (hasRelations)
                {
                    relations = _container.GetRelations(node);
                    hasRelations = relations.Any();
                }
                bool hasExtra = extraPrps.Keys.Any();

                if (useParentId)
                {
                    output.WriteByte(CommaByte);
                    WriteKey(output, "parentId", node.ParentId);
                }
                if (hasChildren)
                {
                    output.WriteByte(CommaByte);
                    output.WriteByte(FnuttByte);
                    WriteString(output, "children");
                    output.WriteByte(FnuttByte);
                    output.WriteByte(ColonByte);
                    output.WriteByte(ArrayStart);
                    var isFirst = true;
                    foreach (var child in node.Children)
                    {
                        if (!isFirst)
                        {
                            output.WriteByte(CommaByte);
                        }
                        StreamSerialize(child, output, depth, level + 1, level < 2, excludedProperties);
                        isFirst = false;
                    }
                    output.WriteByte(ArrayEnd);
                }
                if (hasRelations)
                {
                    output.WriteByte(CommaByte);
                    output.WriteByte(FnuttByte);
                    WriteString(output, "relations");
                    output.WriteByte(FnuttByte);
                    output.WriteByte(ColonByte);
                    output.WriteByte(ArrayStart);
                    var isFirst = true;
                    foreach (var child in relations)
                    {
                        if (!isFirst)
                        {
                            output.WriteByte(CommaByte);
                        }
                        StreamSerialize(child, output, depth, level + 1, false, excludedProperties);
                        isFirst = false;
                    }
                    output.WriteByte(ArrayEnd);
                }
                if (hasExtra)
                {
                    output.WriteByte(CommaByte);
                    var isFirst = true;
                    foreach (var p in extraPrps)
                    {

                        if (!isFirst)
                        {
                            output.WriteByte(CommaByte);
                        }
                        WriteKey(output, p.Key, p.Value);
                        isFirst = false;

                    }
                }
            }
            output.WriteByte(ObjectEnd);
        }

        public void WriteValue(Stream output, object value)
        {
            if (value is string valueString)
            {
                output.WriteByte(FnuttByte);
                WriteString(output, valueString.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t").Replace("\"", "\\\""));
                output.WriteByte(FnuttByte);
            }
            else if (value is bool b)
            {
                output.WriteByte(b ? TrueByte : FalseByte);
            }
            else if (value is DateTime dt)
            {
                WriteString(output, dt.Ticks.ToString());
            }
            else if (value is int || value is float || value is double)
            {
                WriteString(output, value.ToString());
            }
            else if (value is Dictionary<string, object> dictionary)
            {
                output.WriteByte(ObjectStart);
                var isFirst = true;
                foreach (var kv in dictionary)
                {
                    if (!isFirst)
                    {
                        output.WriteByte(CommaByte);
                    }
                    WriteKey(output, kv.Key, kv.Value);
                    isFirst = false;
                }
                output.WriteByte(ObjectEnd);
            }
            else if (value is IEnumerable array)
            {
                var isFirst = true;
                output.WriteByte(ArrayStart);
                foreach (object item in array)
                {
                    if (!isFirst)
                    {
                        output.WriteByte(CommaByte);
                    }
                    if (item is INode node)
                    {
                        StreamSerialize(node, output, 2);
                    }
                    else
                        WriteKey(output, null, item);
                    isFirst = false;
                }
                output.WriteByte(ArrayEnd);
            }
            else
            {
                var prps = value.GetProperties();
                output.WriteByte(ObjectStart);
                var isFirst = true;
                foreach (var p in prps)
                {

                    if (!isFirst)
                    {
                        output.WriteByte(CommaByte);
                    }
                    WriteKey(output, p.Key, p.Value);
                    isFirst = false;

                }
                output.WriteByte(ObjectEnd);
            }
        }

        private void WriteKey(Stream output, string str, object value)
        {
            if (value == null)
                return;
            if (str != null)
            {
                output.WriteByte(FnuttByte);
                WriteString(output, str);
                output.WriteByte(FnuttByte);
                output.WriteByte(ColonByte);
            }

            WriteValue(output, value);
        }

        private void WriteString(Stream s, string v)
        {
            var sendData = Encoding.UTF8.GetBytes(v);
            s.Write(sendData, 0, sendData.Length);
        }


    }
}

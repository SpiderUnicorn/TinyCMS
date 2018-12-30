using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyCMS.Interfaces;

namespace TinyCMS.Serializer
{
    public static class Helper
    {
        public static string ToLowerFirst(this string s)
        {
            return Char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        private static string[] NODE_PROPERTIES = { "Id", "ParentId", "Children", "Tags", "Type", "IsParsed" };

        private static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> props =
            new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();

        public static Dictionary<string, PropertyInfo> FilterExcluded(this Dictionary<string, PropertyInfo> dict, bool isNode, params string[] excludedProperties)
        {
            var ret = new Dictionary<string, PropertyInfo>();
            foreach (var item in dict)
            {
                bool add = true;
                if (isNode)
                    add = !NODE_PROPERTIES.Contains(item.Value.Name);
                if (isNode && excludedProperties.Any())
                    add = add && !excludedProperties.Contains(item.Value.Name);
                if (add)
                    ret.Add(item.Key, item.Value);
            }
            return ret;
        }

        public static Dictionary<string, object> GetPropertyDictionary(this object o, params string[] excludedProperties)
        {
            var ret = new Dictionary<string, object>();
            var dict = o.GetPropertyInfoList().FilterExcluded(o is INode, excludedProperties);
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

        public static Dictionary<string, PropertyInfo> GetPropertyInfoList(this object @this)
        {
            Dictionary<string, PropertyInfo> dict = null;
            var type = @this.GetType();
            if (props.ContainsKey(type))
                dict = props[type];
            else
            {
                dict = new Dictionary<string, PropertyInfo>();
                var isNode = @this is INode;
                var properties = type.GetProperties().Where(d => d.CanRead);

                foreach (var property in properties)
                {
                    var key = property.Name.ToLowerFirst();
                    dict.Add(key, property);
                }

                props.TryAdd(type, dict);
            }

            return dict;
        }
    }

}

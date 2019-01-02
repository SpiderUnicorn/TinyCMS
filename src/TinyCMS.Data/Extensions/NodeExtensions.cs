using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyCMS.Data.Nodes;
using TinyCMS.Interfaces;

namespace TinyCMS.Data.Extensions
{
    public static class ObjectExtensions
    {
        public static object Apply(this object that, IDictionary<string, object> data)
        {
            if (that == null)
                return null;
            var nt = that.GetType();
            var prps = nt.GetProperties().ToList();
            foreach (var key in data.Keys)
            {
                var prp = prps.FirstOrDefault(d => d.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                var val = data[key];
                if (prp != null && val != null && prp.CanWrite)
                {
                    try
                    {
                        if (val is JObject jObject)
                        {
                            val = jObject.ToObject<Dictionary<string, object>>();
                        }
                        prp.SetValue(that, Convert.ChangeType(val, prp.PropertyType), null);
                    }
                    catch (Exception)
                    {

                        // ignore
                    }
                }
            }
            return that;
        }
    }

    public static class NodeExtensions
    {
        public static INode Add(this INode that, INode child)
        {
            if (that.Children == null)
                that.Children = new ObservableCollection<INode>();
            if (string.IsNullOrEmpty(child.Id))
            {
                child.Id = Guid.NewGuid().ToString();
            }
            child.ParentId = that.Id;
            that.Children.Add(child);
            return that;
        }

        public static INode Add<T>(this INode @this) where T : INode
        {
            var child = Activator.CreateInstance<T>();
            @this.Add(child);
            return @this;
        }

        public static bool HasChildren(this INode that) =>
            that.Children?.Any() ?? false;
    }
}

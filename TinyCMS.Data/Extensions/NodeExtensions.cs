﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TinyCMS.Data.Nodes;

namespace TinyCMS.Data.Extensions
{
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

        public static INode Add<T>(this INode that) where T : INode
        {
            var ne = Activator.CreateInstance<T>();
            that.Add(ne);
            return that;
        }

        public static INode Add(this INode that, INode node, IDictionary<string, object> data)
        {
            that.Add(node.Apply(data));
            return that;
        }

        public static INode Apply(this INode that, IDictionary<string, object> data)
        {
            var nt = that.GetType();
            var prps = nt.GetProperties().ToList();
            foreach (var key in data.Keys)
            {
                var prp = prps.FirstOrDefault(d => d.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                var val = data[key];
                if (prp != null && val != null)
                {
                    prp.SetValue(that, val);
                }
            }
            return that;
        }

        public static INode AddBlogPage(this INode that, string id = "")
        {
            that.Add(new Page()
            {
                Id = id,
                Name = "Blogpost"
            }.Add(new Image()
            {
                Path = "/image.jpg"
            }).Add(new Text()
            {
                Value = "Blog title"
            }).Add(new Text()
            {
                Value = "Blog text, lorem ipsum"
            }));
            return that;
        }
    }
}

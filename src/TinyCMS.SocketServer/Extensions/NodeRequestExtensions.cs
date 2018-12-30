using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyCMS.Data.Extensions;
using TinyCMS.Interfaces;

namespace TinyCMS.SocketServer
{
    public static class NodeRequestExtensions
    {
        public static(INode, INode) GetRelation(this INodeRequest request, IContainer container)
        {
            var fromId = request.QueryString.GetString("from");
            var toId = request.QueryString.GetString("to");
            INode from = container.GetById(fromId);
            INode to = container.GetById(toId);
            if (from != null && to != null)
            {
                return (from, to);
            }
            return (null, null);
        }

        public static string GetString(this JObject jobj, string propertyName)
        {
            return jobj.Property(propertyName).Value.ToString();
        }

        public static string GetId(this JObject jobj)
        {
            return jobj.GetString("id");
        }

        public static INode RemoveNode(this INodeRequest request, IContainer container)
        {
            var nodeId = GetId(request.JsonData);
            if (!string.IsNullOrEmpty(nodeId))
            {
                var nodeToRemove = container.GetById(nodeId);
                var parent = container.GetById(nodeToRemove.ParentId);
                container.RemoveNode(nodeToRemove);
                return parent;
            }
            return null;
        }

        public static INode GetUpdatedNode(this INodeRequest request, IContainer container)
        {
            var nodeId = GetId(request.JsonData);
            if (!string.IsNullOrEmpty(nodeId))
            {
                return container.GetById(nodeId).Apply(request.JsonData);
            }
            return null;
        }

        public static INode GetParent(this INodeRequest request, IContainer container)
        {
            var parentId = request.QueryString.GetString("parentId", request.JsonData.GetString("parentId"));
            return container.GetById(parentId) ?? container.RootNode;
        }

        public static INode Move(this INodeRequest request, IContainer container)
        {
            var moveData = new MoveData();
            JsonSerializer.CreateDefault().Populate(request.JsonData.CreateReader(), moveData);
            if (!string.IsNullOrEmpty(moveData.ParentId) && !string.IsNullOrEmpty(moveData.OldParentId))
            {
                if (moveData.ParentId.Equals(moveData.OldParentId))
                {
                    return container
                        .GetById(moveData.OldParentId)
                        .SetNodePosition(
                            container.GetById(moveData.Id),
                            moveData.NewIndex);
                }
                else
                {
                    return container
                        .GetById(moveData.OldParentId)
                        .ChangeParent(
                            container.GetById(moveData.Id),
                            container.GetById(moveData.ParentId),
                            moveData.NewIndex);
                }
            }
            return null;
        }

        public static INode SetNodePosition(this INode parent, INode childToMove, int newPosition)
        {
            parent.Children.Move(parent.Children.IndexOf(childToMove), newPosition);
            return parent;
        }

        public static INode ChangeParent(this INode oldParent, INode childToMove, INode newParent, int newPosition)
        {
            if (!newParent.Children.Any())
                newParent.Children.Add(childToMove);
            else
                newParent.Children.Insert(newPosition, childToMove);
            oldParent.Children.Remove(childToMove);
            return newParent;
        }

        public static INode GetNewNode(this INodeRequest request, IContainer container, INodeTypeFactory factory)
        {
            if (factory == null)
                throw new ArgumentException("Node type factory is needed to create new nodes", nameof(factory));
            var parent = request.GetParent(container);
            var type = request.QueryString.GetString("type", request.JsonData.GetString("type"));
            var newNode = factory.GetNew(type);
            if (request.JsonData != null)
            {
                newNode.Apply(request.JsonData);
            }
            parent.Add(newNode);
            return newNode;
        }

        public static INode Add(this INode that, INode node, IDictionary<string, object> data)
        {
            that.Add(node.Apply(data));
            return that;
        }

        public static INode Apply(this INode that, JObject jObject)
        {
            using(var sr = jObject.CreateReader())
            {
                JsonSerializer.CreateDefault().Populate(sr, that); // Uses the system default JsonSerializerSettings
            }
            return that;
        }

        public static INode Apply(this INode that, IDictionary<string, object> data)
        {
            if (that == null)
                return null;
            var nt = that.GetType();
            var prps = nt.GetProperties().ToList();
            foreach (var key in data.Keys)
            {
                var prp = prps.FirstOrDefault(d => d.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                var val = data[key];
                if (prp != null && val != null && prp.CanWrite && prp.Name != "type")
                {
                    try
                    {
                        if (val is JObject jobj)
                        {
                            val = jobj.ToObject<Dictionary<string, object>>();
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
}

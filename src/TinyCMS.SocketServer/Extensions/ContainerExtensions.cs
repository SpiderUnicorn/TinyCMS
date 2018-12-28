using TinyCMS.Interfaces;

namespace TinyCMS.SocketServer
{
    public static class ContainerExtensions
    {

        public static INode MatchRequest(this IContainer container, INodeRequest request, INodeTypeFactory typeFactory)
        {
            INode ret = null;
            switch (request.RequestType)
            {
                case RequestTypeEnum.Move:
                    return request.Move(container);
                case RequestTypeEnum.Get:
                    return container.GetById(request.Data);
                case RequestTypeEnum.Add:
                    return request.GetNewNode(container, typeFactory);
                case RequestTypeEnum.Update:
                    return request.GetUpdatedNode(container);
                case RequestTypeEnum.Remove:
                    return request.RemoveNode(container);
                case RequestTypeEnum.Link:
                    var rel = request.GetRelation(container);
                    if (rel.Item1 != null && rel.Item2 != null)
                    {
                        container.AddRelation(rel.Item1, rel.Item2);
                        ret = rel.Item1;
                    }
                    return ret;
                default:
                    return ret;
            }
        }
    }
}

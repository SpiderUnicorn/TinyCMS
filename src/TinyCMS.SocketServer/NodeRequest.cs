using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyCMS.Interfaces;

namespace TinyCMS
{
    public sealed class NodeRequest : INodeRequest
    {
        public RequestTypeEnum RequestType { get; }

        public string Data { get; }

        public JObject JsonData { get; }

        public Dictionary<string, string> QueryString { get; }

        private NodeRequest(RequestTypeEnum type, string data, JObject json = null, Dictionary<string, string> query = null)
        {
            RequestType = type;
            Data = data;
            JsonData = json;
            QueryString = query;
        }
        public static INodeRequest AuthRequestWithToken(string token) =>
            new NodeRequest(RequestTypeEnum.AuthToken, token);

        public static INodeRequest JsonRequest(RequestTypeEnum type, string data, JObject json) =>
            new NodeRequest(type, data, json);

        public static INodeRequest QueryRequest(RequestTypeEnum type, string data, Dictionary<string, string> query) =>
            new NodeRequest(type, data, null, query);

    }
}

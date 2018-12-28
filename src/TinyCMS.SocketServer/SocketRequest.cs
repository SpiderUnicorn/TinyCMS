using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyCMS.Interfaces;
using static System.Text.Encoding;

namespace TinyCMS
{
    /// <summary>
    /// Parses client requests which consists of a request type
    /// and either a node ID or a node object as JSON.
    /// </summary>
    public static class SocketRequestParser
    {
        public static INodeRequest Parse(byte[] data, int length) =>
            Parse(UTF8.GetString(data, 0, length));

        public static INodeRequest Parse(string request)
        {
            if (request.Length <= 1)
                throw new ArgumentException($"Request '{request}' too short");

            if (isAuthRequest(request))
            {
                var token = request.Substring(2, request.Length - 4);
                return NodeRequest.AuthRequestWithToken(token);
            }

            var requestType = RequestTypeEnum.Unknown;
            if (RequestTypeMap.ContainsKey(request[0]))
            {
                requestType = RequestTypeMap[request[0]];
            }

            var data = request.Substring(1);

            var splitIndex = data.IndexOf(':');
            var firstJsonObj = data.IndexOf('{');
            var isQuery = firstJsonObj > splitIndex && splitIndex > 0;
            if (isQuery)
            {
                var query = data.Substring(0, splitIndex);
                var queryParts = query.Split('&');

                var queryDict = new Dictionary<string, string>();
                foreach (var queryPart in queryParts)
                {
                    var keyAndValue = queryPart.Split('=');
                    string value = "1";
                    if (keyAndValue.Length > 1)
                    {
                        value = keyAndValue[1];
                    }

                    queryDict.Add(keyAndValue[0], value);
                }

                var strippedData = data.Substring(splitIndex + 1);
                return NodeRequest.QueryRequest(requestType, strippedData, queryDict);
            }
            else
            {
                var json = default(JObject);
                try
                {
                    json = JObject.Parse(data);
                }
                catch { /* corrupted data but we don't care */ }

                return NodeRequest.JsonRequest(requestType, data, json);
            }
        }

        /// <summary>
        /// Auth requests have the format ##token}##
        /// </summary>
        /// <param name="request">Any request</param>
        /// <returns>True if the request is an auth request</returns>
        private static bool isAuthRequest(string request) => request.StartsWith("##");

        private static Dictionary<char, RequestTypeEnum> RequestTypeMap = new Dictionary<char, RequestTypeEnum>
            {
                ['>'] = RequestTypeEnum.Move,
                ['?'] = RequestTypeEnum.Get,
                ['='] = RequestTypeEnum.Update,
                ['+'] = RequestTypeEnum.Add,
                ['-'] = RequestTypeEnum.Remove,
                ['!'] = RequestTypeEnum.Link,
            };

    }
}

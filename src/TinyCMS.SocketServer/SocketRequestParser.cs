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
    ///
    /// Auth requests have the format ##token}##
    /// </summary>
    public static class SocketRequestParser
    {
        public static INodeRequest Parse(byte[] data, int numberOfBytesToDecode) =>
            Parse(UTF8.GetString(data, 0, numberOfBytesToDecode));

        public static INodeRequest Parse(string request)
        {
            if (request.Length <= 1)
                throw new ArgumentException($"Request '{request}' too short");

            return Parse(
                GetRequestType(request),
                GetRequestData(request)
            );
        }

        private static INodeRequest Parse(RequestTypeEnum type, string data)
        {
            if (type == RequestTypeEnum.AuthToken)
                return ParseAuthRequest(data);

            if (IsQueryRequest(data))
                return ParseQueryRequest(type, data);
            else
                return ParseJsonRequest(type, data);
        }

        private static string GetRequestData(string request)
        {
            if (request.StartsWith("##"))
            {
                return request.Substring(2, request.Length - 4);
            }
            return request.Substring(1);
        }

        private static INodeRequest ParseAuthRequest(string token) =>
            NodeRequest.AuthRequestWithToken(token);

        private static RequestTypeEnum GetRequestType(string request) =>
            RequestTypeMap.ContainsKey(request[0])
            ? RequestTypeMap[request[0]]
            : RequestTypeEnum.Unknown;

        private static Dictionary<char, RequestTypeEnum> RequestTypeMap = new Dictionary<char, RequestTypeEnum>
            {
                ['#'] = RequestTypeEnum.AuthToken,
                ['>'] = RequestTypeEnum.Move,
                ['?'] = RequestTypeEnum.Get,
                ['='] = RequestTypeEnum.Update,
                ['+'] = RequestTypeEnum.Add,
                ['-'] = RequestTypeEnum.Remove,
                ['!'] = RequestTypeEnum.Link,
            };

        private static bool IsQueryRequest(string data)
        {
            var splitIndex = data.IndexOf(':');
            var firstJsonObj = data.IndexOf('{');

            return firstJsonObj > splitIndex && splitIndex > 0;
        }

        private static INodeRequest ParseQueryRequest(RequestTypeEnum requestType, string data)
        {
            var splitIndex = data.IndexOf(':');
            var query = data.Substring(0, splitIndex);
            var queryParts = query.Split('&');

            var queryDict = new Dictionary<string, string>();
            foreach (var queryPart in queryParts)
            {
                var keyAndValue = queryPart.Split('=');
                var value = "1";
                if (keyAndValue.Length > 1)
                {
                    value = keyAndValue[1];
                }

                queryDict.Add(keyAndValue[0], value);
            }

            var strippedData = data.Substring(splitIndex + 1);
            return NodeRequest.QueryRequest(requestType, strippedData, queryDict);
        }

        private static INodeRequest ParseJsonRequest(RequestTypeEnum requestType, string data)
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
}

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyCMS.Interfaces;

namespace TinyCMS
{
    public class SocketRequest : INodeRequest
    {
        public SocketRequest() { }

        public SocketRequest(string request) => Parse(request);

        public SocketRequest(byte[] data, int length) =>
            Parse(System.Text.Encoding.UTF8.GetString(data, 0, length));

        public Dictionary<string, string> QueryString { get; internal set; } = new Dictionary<string, string>();

        public JObject JsonData { get; internal set; }

        public RequestTypeEnum RequestType { get; internal set; }

        public string Data { get; internal set; }

        // TODO: Unused
        public ISerializerSettings GetSerializerSettings()
        {
            var settings = new SerializerSettings();
            settings.FromRequest(this);
            return settings;
        }

        private Dictionary<char, RequestTypeEnum> RequestTypeMap = new Dictionary<char, RequestTypeEnum>
            {
                ['>'] = RequestTypeEnum.Move,
                ['?'] = RequestTypeEnum.Get,
                ['='] = RequestTypeEnum.Update,
                ['+'] = RequestTypeEnum.Add,
                ['-'] = RequestTypeEnum.Remove,
                ['!'] = RequestTypeEnum.Link,
            };

        /// <summary>
        /// Parses client requests which consists of a request type
        /// and either a node ID or a node object as JSON.
        ///
        /// <see cref="RequestTypeEnum" />
        ///
        /// Sample requests:
        ///
        /// Get root:
        /// ?root
        ///
        /// Add data:
        /// +"{"foo": "bar"}"
        ///
        /// Update data:
        /// ="{"id": "baz", "foo": "bar"}"
        /// </summary>
        /// <param name="request"></param>
        private void Parse(string request)
        {
            if (request.Length > 1)
            {
                if (isAuthRequest(request))
                {
                    var token = request.Substring(2, request.Length - 4);
                    RequestType = RequestTypeEnum.AuthToken;
                    Data = token;
                    return;
                }

                if (RequestTypeMap.ContainsKey(request[0]))
                {
                    RequestType = RequestTypeMap[request[0]];
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

                    QueryString = queryDict;
                    Data = data.Substring(splitIndex + 1);
                }
                else
                {
                    // it's probably json
                    Data = data;
                    try
                    {
                        JsonData = JObject.Parse(Data);
                    }
                    catch
                    {
                        // corrupted data
                    }
                }
            }
        }

        /// <summary>
        /// Auth requests have the format ##token}##
        /// </summary>
        /// <param name="request">Any request</param>
        /// <returns>True if the request is an auth request</returns>
        private bool isAuthRequest(string request) => request.StartsWith("##");
    }
}

﻿using System.Collections.Generic;
using Newtonsoft.Json;
using MessagePack.Formatters;
using System;

namespace TinyCMS
{
    public class SocketRequest : INodeRequest
    {
        public SocketRequest()
        {

        }

        public SocketRequest(string request)
        {
            Parse(request);
        }

        public SocketRequest(byte[] data, int length)
        {
            Parse(System.Text.Encoding.UTF8.GetString(data, 0, length));
        }

        public Dictionary<string, string> QueryString { get; internal set; } = new Dictionary<string, string>();

        public Dictionary<string, object> JsonData { get; internal set; }

        public RequestTypeEnum RequestType { get; internal set; }

        public string Data { get; internal set; }

        public ISerializerSettings GetSerializerSettings()
        {
            var settings = new SerializerSettings();
            settings.FromRequest(this);
            return settings;
        }

        private void Parse(string request)
        {
            if (request.Length > 1)
            {
                switch (request[0])
                {
                    case '?':
                        RequestType = RequestTypeEnum.Get;
                        break;
                    case '=':
                        RequestType = RequestTypeEnum.Update;
                        break;
                    case '+':
                        RequestType = RequestTypeEnum.Add;
                        break;
                    case '-':
                        RequestType = RequestTypeEnum.Remove;
                        break;
                    case '!':
                        RequestType = RequestTypeEnum.Link;
                        break;
                }
                var data = request.Substring(1);
                var splitIdx = data.IndexOf(':');
                var firstJsonObj = data.IndexOf('{');
                if (firstJsonObj > splitIdx && splitIdx > 0)
                {
                    var query = data.Substring(0, splitIdx);
                    var content = data.Substring(splitIdx + 1);
                    var queryDict = new Dictionary<string, string>();
                    var queryParts = query.Split('&');

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

                    Data = content;
                }
                else
                {
                    Data = data;
                }
                if (!string.IsNullOrEmpty(Data) && Data.Contains("{"))
                {
                    try
                    {
                        JsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(Data);
                    }
                    catch (Exception ex)
                    {
                        var i = 2;
                    }
                }
            }
        }
    }

    public class SerializerSettings : ISerializerSettings
    {
        public void FromRequest(INodeRequest request)
        {
            FromQueryString(request.QueryString);
        }

        public void FromQueryString(Dictionary<string, string> queryString)
        {
            if (queryString.ContainsKey("rel"))
            {
                IncludeRelations = queryString["rel"] == "1";
            }
            if (queryString.ContainsKey("depth"))
            {
                int levels = 3;
                if (int.TryParse(queryString["depth"], out levels))
                {
                    Depth = levels;
                }
            }
            if (queryString.ContainsKey("level"))
            {
                int level = 0;
                if (int.TryParse(queryString["level"], out level))
                {
                    Level = level;
                }
            }
        }

        public int Depth { get; set; } = 3;

        public int Level { get; set; } = 0;

        public bool IncludeRelations { get; set; } = true;
    }

    public interface ISerializerSettings
    {
        int Depth { get; }
        int Level { get; }
        bool IncludeRelations { get; }
    }
}

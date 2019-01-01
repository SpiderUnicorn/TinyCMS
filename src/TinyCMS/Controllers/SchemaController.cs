using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TinyCMS.Data.Builder;
using TinyCMS.Interfaces;
using TinyCMS.Serializer;

namespace TinyCMS.Controllers
{
    [Route("/api/schema")]
    [Produces("application/json")]
    public class SchemaController : Controller
    {

        readonly INodeTypeFactory _factory;
        readonly SchemaSerializer _serializer;

        private void SendOkJson()
        {
            Response.ContentType = "application/json";
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public SchemaController(INodeTypeFactory factory, SchemaSerializer serializer)
        {
            this._factory = factory;
            this._serializer = serializer;
        }

        private string GetToken()
        {
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return authHeader;
            }
            return string.Empty;
        }

        [HttpGet("{type}")]
        public void GetSchema(string type)
        {
            SendOkJson();
            _serializer.StreamSchema(_factory.GetTypeByName(type), Response.Body);
        }

        [HttpGet]
        public void GetAll()
        {
            SendOkJson();
            _serializer.StreamTypes(Response.Body, _factory.RegisterdTypeNames());
        }
    }
}

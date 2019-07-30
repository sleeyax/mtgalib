using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace mtgalib.Server
{
    public class JsonRpcResponse
    {
        public class JsonRpcResponseResult
        {
            public string method { get; set; }
            public JToken payload { get; set; }

            public T GetPayloadValue<T>(string key)
            {
                return payload.Value<T>(key);
            }

            public string GetPayloadValue()
            {
                return payload.ToString();
            }
        }

        public class JsonRpcResponseError
        {
            public GameServerErrorCode code { get; set; }
            public string message { get; set; }
            public Dictionary<string, object> data { get; set; }
        }

        public string jsonrpc { get; set; }
        public int id { get; set; }
        public JsonRpcResponseResult result { get; set; }
        public JsonRpcResponseError error { get; set; }
    }
}
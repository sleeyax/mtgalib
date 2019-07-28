using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace mtgalib.Server
{
    public class JsonRpcResponseResult
    {
        public string method { get; set; }
        public JToken payload { get; set; }

        public T GetPayloadValue<T>(string key)
        {
            return payload.Value<T>(key);
        }
    }

    public class JsonRpcResponseError
    {
        public ServerErrorCode code { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> data { get; set; }
    }

    public class JsonRpcResponse
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public JsonRpcResponseResult result { get; set; }
        public JsonRpcResponseError error { get; set; }

    }
}
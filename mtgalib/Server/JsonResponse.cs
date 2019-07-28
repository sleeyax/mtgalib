using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace mtgalib.Server
{
    public class JsonResponseResult
    {
        public string method { get; set; }
        public JToken payload { get; set; }
    }

    public class JsonResponseError
    {
        public ServerErrorCode code { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> data { get; set; }
    }

    public class JsonResponse
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public JsonResponseResult result { get; set; }
        public JsonResponseError error { get; set; }
    }
}
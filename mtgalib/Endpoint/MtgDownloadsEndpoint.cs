using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mtgalib
{
    internal class MtgDownloadsEndpoint : BaseEndpoint
    {
        public const string PLATFORM_WINDOWS = "Windows32";

        public MtgDownloadsEndpoint()
        {
            _url = "https://mtgarena.downloads.wizards.com";
        }

        public async Task<string> GetClientVersionAsyncTask(string platform)
        {
            string response = await _wc.DownloadStringTaskAsync($"{_url}/Live/{platform}/version");
            VersionResponseJson json = JsonConvert.DeserializeObject<VersionResponseJson>(response);
            return json.Versions.Keys.Max();
        }

        private class VersionResponseJson
        {
            public Dictionary<string, string> Versions { get; set; }
            public string Name { get; set; }
            public string Flavor { get; set; }
            public string VersionURL { get; set; }
            public string ProductCode { get; set; }
            public string BinaryURL { get; set; }
            public string FileCount { get; set; }
        }
    }
}
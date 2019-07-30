using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace mtgalib.Endpoint
{
    internal class MtgDownloadsEndpoint : BaseEndpoint
    {
        public const string PLATFORM_WINDOWS = "Windows32";

        public MtgDownloadsEndpoint()
        {
            _url = "https://mtgarena.downloads.wizards.com";
        }

        public async Task<string> GetLatestGameClientVersionAsyncTask(string platform)
        {
            string response = await _wc.DownloadStringTaskAsync($"{_url}/Live/{platform}/version");
            VersionResponseJson json = JsonConvert.DeserializeObject<VersionResponseJson>(response);
            // Format will be: 0.1.1595.718832 where 1595.718832 is the version we need
            return json.Versions.Keys.Max();
        }

        internal class VersionResponseJson
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
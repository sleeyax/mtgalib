using System.Net;
using System.Threading.Tasks;

namespace mtgalib
{
    public class MtgAssetsEndpoint
    {
        private WebClient _wc;
        private readonly string _endpointUrl = "https://assets.mtgarena.wizards.com";

        public MtgAssetsEndpoint()
        {
            _wc = new WebClient();
        }

        public async Task<string> GetProdUriAsyncTask()
        {
            string prodUri = await _wc.DownloadStringTaskAsync($"{_endpointUrl}/prodUri.txt");
            return prodUri;
        }
    }
}
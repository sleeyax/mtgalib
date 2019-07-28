using System.Threading.Tasks;

namespace mtgalib.Endpoint
{
    internal class MtgAssetsEndpoint : BaseEndpoint
    {
        
        public MtgAssetsEndpoint()
        {
            _url = "https://assets.mtgarena.wizards.com";
        }

        public async Task<string> GetProdUriAsyncTask()
        {
            string prodUri = await _wc.DownloadStringTaskAsync($"{_url}/prodUri.txt");
            return prodUri;
        }
    }
}
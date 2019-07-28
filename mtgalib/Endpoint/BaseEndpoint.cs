using System.Net;

namespace mtgalib.Endpoint
{
    internal abstract class BaseEndpoint
    {
        protected string _url;
        protected WebClient _wc;
        protected BaseEndpoint()
        {
            _wc = new WebClient();
        }
    }
}
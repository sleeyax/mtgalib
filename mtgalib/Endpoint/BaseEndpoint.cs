using System.Net;

namespace mtgalib
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
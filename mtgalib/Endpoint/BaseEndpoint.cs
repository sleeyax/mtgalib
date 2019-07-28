using System.Net;

namespace mtgalib
{
    public abstract class BaseEndpoint
    {
        protected string _url;
        protected WebClient _wc;
        protected BaseEndpoint()
        {
            _wc = new WebClient();
        }
    }
}
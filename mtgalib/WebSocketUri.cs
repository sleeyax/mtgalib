using System;

namespace mtgalib
{
    internal class WebSocketUri : Uri
    {
        public bool IsSecure { get; set; }

        public WebSocketUri(string uri) : base(uri)
        {
            IsSecure = Scheme == "wss";
        }
    }
}
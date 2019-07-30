using System;

namespace mtgalib
{
    internal class Version
    {
        public string Major { get; set; }
        public string Minor { get; set; }
        public string Patch { get; set; }
        public string Meta { get; set; }

        public Version(string version)
        {
            string[] splitted = version.Split('.');

            if (splitted.Length > 4)
                throw new ArgumentException("Version string can't contain more than 4 dots");

            Major = splitted[0];
            Minor = splitted[1];
            Patch = splitted[2];
            Meta  = splitted[3];
        }

    }
}
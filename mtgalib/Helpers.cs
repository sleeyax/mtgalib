using System;
using System.Text;

namespace mtgalib
{
    internal class Helpers
    {
        public static string Base64Encode(string plaintext)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            return Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(string encoded)
        {
            byte[] bytes = Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
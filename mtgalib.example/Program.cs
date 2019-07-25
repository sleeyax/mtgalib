using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtgalib.example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Credentials credentials = new Credentials("token");
            Credentials credentials = new Credentials("username", "password");
            bool verified = credentials.VerifyAsync().GetAwaiter().GetResult();
            if (verified)
            {
                Console.WriteLine(credentials.RefreshToken);
                Console.WriteLine(credentials.AccessToken);
                Console.WriteLine(credentials.DisplayName);
                Console.WriteLine(credentials.PersonaId);
            }
            else
            {
                Console.WriteLine("Invalid credentials!");
            }

            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using mtgalib.Player;
using mtgalib.Server;

namespace mtgalib.example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Credentials credentials = new Credentials("token");
            /*Credentials credentials = new Credentials("username", "password");
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
            */

           // TcpConnTest();
          DoYourThing();
         // Console.ReadKey();
        }

        static async Task DoYourThing()
        {
            PlayerEnvironment environment = PlayerEnvironment.GetEnvironment();
            MtgaServer server = new MtgaServer(environment);

            Console.WriteLine("Connecting to server...");
            bool connected = await server.ConnectTask();
            if (connected)
                Console.WriteLine("Connected!");

            string cmd = "hello";
            server.Send(cmd);
            Console.WriteLine("-> " + cmd);

            string response = await server.ReadResponseTask();
            Console.WriteLine("<- " + response);

            Console.ReadKey();
        }
    }
}

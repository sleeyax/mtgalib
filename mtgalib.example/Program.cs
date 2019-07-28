using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using mtgalib.Player;
using mtgalib.Server;

namespace mtgalib.example
{
    class Program
    {

        private static string _ticket;

        static void Main(string[] args)
        {
            // PlayerCredentials credentials = new PlayerCredentials("token");
            PlayerCredentials credentials = new PlayerCredentials("username", "password");
            Console.WriteLine("Authenticating...");
            bool verified = credentials.VerifyAsyncTask().GetAwaiter().GetResult();
            if (verified)
            {
                Console.WriteLine("Logged in!");
                _ticket = credentials.AccessToken;
            }
            else
            {
                Console.WriteLine("Invalid credentials!");
                Console.ReadKey();
                return;
            }

            DoYourThing();
            Console.ReadKey();
        }

        static async Task DoYourThing()
        {
            PlayerEnvironment environment = PlayerEnvironment.GetEnvironment(PlayerEnvironmentType.ProdB);
            MtgaServer server = new MtgaServer(environment);

            // Register event handlers
            server.SetOnMsgSentAction((bytes, offset, count) => Console.WriteLine("-> " + Encoding.UTF8.GetString(bytes, offset, count)));
            server.SetOnMsgReceivedAction((bytes, offset, count) => Console.WriteLine("<- " + Encoding.UTF8.GetString(bytes, offset, count)));


            // Connect to server
            Console.WriteLine("Connecting to server...");
            bool connected = await server.ConnectTask();
            if (connected)
                Console.WriteLine("Connected!");

            // Authenticate user
            await server.AuthenticateAsyncTask(_ticket);
            JsonRpcResponse response = await server.ReadResponseTask();
            Console.WriteLine("Session id: " + response.result.GetPayloadValue<string>("sessionId"));

        }

    }
}

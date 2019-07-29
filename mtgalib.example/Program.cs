using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using mtgalib.Local;
using mtgalib.Player;
using mtgalib.Server;

namespace mtgalib.example
{
    class Program
    {

        private static string _ticket;

        static void Main(string[] args)
        {
            InstalledGame installedGame = new InstalledGame("D:\\Games\\MTGA");
            PlayerCredentials credentials = new PlayerCredentials(installedGame.GetRefreshToken());
            // PlayerCredentials credentials = new PlayerCredentials("username", "password");
            Console.WriteLine("Authenticating...");
            bool verified = credentials.VerifyAsyncTask().GetAwaiter().GetResult();
            if (verified)
            {
                Console.WriteLine("Logged in!");
                _ticket = credentials.AccessToken;
                installedGame.SaveRefreshToken(credentials.RefreshToken);
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
            server.SetOnMsgSentEvent((bytes, offset, count) => Console.WriteLine("-> " + Encoding.UTF8.GetString(bytes, offset, count)));
            server.SetOnMsgReceivedEvent((bytes, offset, count) => Console.WriteLine("<- " + Encoding.UTF8.GetString(bytes, offset, count)));
            server.SetOnConnectedEvent(b => Console.WriteLine("Connected"));
            server.SetOnConnectionClosedEvent(s => Console.WriteLine("Disconnected. Reason: " + s));

            // Connect to server
            Console.WriteLine("Connecting to server...");
            await server.ConnectTask();

            // Authenticate user
            JsonRpcResponse response = await server.AuthenticateAsyncTask(_ticket);
            Console.WriteLine("Session id: " + response.result.GetPayloadValue<string>("sessionId"));

            response = await server.PingAsyncTask();
            Console.WriteLine(response.result.GetPayloadValue());

            response = await server.DerpAsyncTask();
            Console.WriteLine(response.error.message);

        }

    }
}

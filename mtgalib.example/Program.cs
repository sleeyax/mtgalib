using System;
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
                Console.WriteLine(credentials.RefreshToken);
                Console.WriteLine(credentials.AccessToken);
                Console.WriteLine(credentials.DisplayName);
                Console.WriteLine(credentials.PersonaId);
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

            // Connect to server
            Console.WriteLine("Connecting to server...");
            bool connected = await server.ConnectTask();
            if (connected)
                Console.WriteLine("Connected!");

            // Authenticate user
            await server.AuthenticateAsyncTask(_ticket);
            string response = await server.ReadResponseTask();
            Console.WriteLine("<- " + response);

        }

    }
}

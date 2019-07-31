using System;
using System.Threading.Tasks;
using mtgalib.Endpoint;

namespace mtgalib.Player
{
    public class PlayerEnvironment
    {
        public string Name { get; set; }
        public string NameHumanReadable { get; set; }
        public PlayerEnvironmentType Type { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public static PlayerEnvironment GetEnvironment(PlayerEnvironmentType environmentType = PlayerEnvironmentType.ProdB)
        {
            switch (environmentType)
            {
                case PlayerEnvironmentType.Beta:
                    return new PlayerEnvironment
                    {
                        Name = "QATrunk",
                        NameHumanReadable = "Beta",
                        Type = environmentType,
                        Host = "client-arena-deva-game.mtgabetasfexplorer.com",
                        Port = 9411,
                        ClientId = "YBGWRYBSNLGIJGNOISDN",
                        ClientSecret = "FQEKGNFIBBNQYMOMDSIMMAOMBIUDISD"
                    };
                case PlayerEnvironmentType.Developer:
                    return new PlayerEnvironment
                    {
                        Name = "QADev",
                        NameHumanReadable = "Developer",
                        Type = environmentType,
                        Host = "devgamesfc.mtgabeta.com",
                        Port = 9411,
                        ClientId = "YBGWRYBSNLGIJGNOISDN",
                        ClientSecret = "FQEKGNFIBBNQYMOMDSIMMAOMBIUDISD"
                    };
                case PlayerEnvironmentType.ProdA:
                    return new PlayerEnvironment
                    {
                        Name = "ProdA",
                        NameHumanReadable = "Production A",
                        Type = environmentType,
                        Host = "client.arenagame-a.east.magic-the-gathering-arena.com",
                        Port = 9405,
                        ClientId = "N8QFG8NEBJ5T35FB",
                        ClientSecret = "VMK1RE8YK6YR4EABJU91"
                    };
                case PlayerEnvironmentType.ProdB:
                    return new PlayerEnvironment
                    {
                        Name = "ProdB",
                        NameHumanReadable = "Production B",
                        Type = environmentType,
                        Host = "client.arenagame-b.east.magic-the-gathering-arena.com",
                        Port = 9405,
                        ClientId = "N8QFG8NEBJ5T35FB",
                        ClientSecret = "VMK1RE8YK6YR4EABJU91"
                    };
                default:
                    throw new ArgumentException("Unknown PlayerEnvironmentType specified!");
            }
        }

        public static async Task<PlayerEnvironment> GetDefaultEnvironmentAsyncTask()
        {
            // Download default ProdUri from the MTGA assets endpoint
            MtgAssetsEndpoint assetsEndpoint = new MtgAssetsEndpoint();
            string prodUri = await assetsEndpoint.GetProdUriAsyncTask();
            prodUri = prodUri.Replace("wss://", "").Replace("ws://", "").ToLower();
            string[] parts = prodUri.Split('/');

            // It should be Prod A or B
            if (parts[0].Contains("arenagame-b") && parts[1] == "prod")
            {
                return GetEnvironment(PlayerEnvironmentType.ProdB);
            }
            else if (parts[0].Contains("arenagame-a") && parts[1] == "prod")
            {
                return GetEnvironment(PlayerEnvironmentType.ProdA);
            }
            else
            {
                throw new Exception($"No environment found for prodUri '{prodUri}'");
            }

        }
    }
}
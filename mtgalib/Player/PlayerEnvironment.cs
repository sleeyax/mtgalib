﻿using System;
using System.Threading.Tasks;
using mtgalib.Endpoint;
using mtgalib.Local;

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

        private static PlayerEnvironment GetEnvironment(string prodUri)
        {
            var uri = new WebSocketUri(prodUri);

            // It should be Prod A or B
            if (uri.Host.Contains("arenagame-b"))
            {
                return GetEnvironment(PlayerEnvironmentType.ProdB);
            }
            else if (uri.Host.Contains("arenagame-a"))
            {
                return GetEnvironment(PlayerEnvironmentType.ProdA);
            }
            else
            {
                throw new Exception($"No environment found for prodUri '{prodUri}'");
            }
        }

        /// <summary>
        /// Returns the default player environment from an online endpoint
        /// </summary>
        /// <returns></returns>
        public static async Task<PlayerEnvironment> GetDefaultEnvironmentAsyncTask()
        {
            // Download default ProdUri from the MTGA assets endpoint
            MtgAssetsEndpoint assetsEndpoint = new MtgAssetsEndpoint();
            string prodUri = await assetsEndpoint.GetProdUriAsyncTask();

            return GetEnvironment(prodUri);
        }

        /// <summary>
        /// Returns the current player environment from locally stored information.  
        /// </summary>
        /// <param name="defaultTo"></param>
        /// <returns></returns>
        public static PlayerEnvironment GetCurrentEnvironment(PlayerEnvironmentType defaultsTo = PlayerEnvironmentType.ProdB)
        {
            InstalledGame installedGame = new InstalledGame("");
            string prodUri = installedGame.GetProdUri();

            return prodUri == null ? GetEnvironment(defaultsTo) : GetEnvironment(prodUri);
        }
    }
}
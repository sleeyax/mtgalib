using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using mtgalib.Endpoint;
using Newtonsoft.Json;

namespace mtgalib.Local
{
    public class InstalledGame
    {

        private readonly string _gameDir;

        public InstalledGame(string installationPath)
        {
            _gameDir = installationPath;
        }

        /// <summary>
        /// Reads the refresh token from registry. Returns null if not found
        /// </summary>
        /// <returns></returns>
        public string GetRefreshToken()
        {
            object rawRefreshToken = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Wizards Of The Coast\\MTGA", "WAS-RefreshToken_h992839947", null);
            return rawRefreshToken == null ? null : Encoding.UTF8.GetString((byte[]) rawRefreshToken).TrimEnd('\0');
        }

        /// <summary>
        /// Writes the refresh token to registry. Returns a boolean on success or failure
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public bool SaveRefreshToken(string refreshToken)
        {
            try
            {
                byte[] rawRefreshToken = Encoding.UTF8.GetBytes(refreshToken);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Wizards Of The Coast\\MTGA", "WAS-RefreshToken_h992839947", rawRefreshToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads stored version information and returns the current version of the installed game client
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            try
            {
                string versionFile = Path.Combine(_gameDir, "version");
                var json = JsonConvert.DeserializeObject<MtgDownloadsEndpoint.VersionResponseJson>(File.ReadAllText(versionFile));
                return json.Versions.Keys.Max();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
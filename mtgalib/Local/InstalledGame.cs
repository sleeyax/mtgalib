using System;
using System.Text;
using Microsoft.Win32;

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
    }
}
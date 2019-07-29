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
            return rawRefreshToken == null ? null : Encoding.UTF8.GetString((byte[]) rawRefreshToken);
        }
    }
}
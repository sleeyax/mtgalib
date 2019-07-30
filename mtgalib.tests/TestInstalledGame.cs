using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mtgalib.Local;

namespace mtgalib.tests
{
    [TestClass]
    public class TestInstalledGame
    { 

        private InstalledGame _installedGame;

        public TestInstalledGame()
        {
            _installedGame = new InstalledGame("D:\\Games\\MTGA");
        }

        [TestMethod]
        public void ShouldReadRefreshTokenFromRegistryOrReturnNull()
        {
            string refreshToken = _installedGame.GetRefreshToken();
            try
            {
                Assert.IsNotNull(refreshToken);
            }
            catch
            {
                Assert.IsNull(refreshToken);
            }
        }

        [TestMethod]
        public void ShouldReadGameClientVersionOrReturnNull()
        {
            string version = _installedGame.GetVersion();
            try
            {
                Assert.IsNotNull(version);
            }
            catch
            {
                Assert.IsNull(version);
            }
        }
    }
}

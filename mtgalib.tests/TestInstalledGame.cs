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
        public void ShouldReadRefreshTokenFromRegistry()
        {
            string refreshToken = _installedGame.GetRefreshToken();
            Assert.IsNotNull(refreshToken, refreshToken);
        }
    }
}

using mtgalib.Player;

namespace mtgalib.Server
{
    public class MtgaServer
    {
        private TcpConnection _tcpConnection;
        private string _host;
        private int _port;

        public MtgaServer(PlayerEnvironment playerEnvironment)
        {
            _tcpConnection = new TcpConnection();
            _host = playerEnvironment.Host;
            _port = playerEnvironment.Port;
        }

        public void Connect()
        {
            _tcpConnection.Connect(_host, _port);
        }
    }
}
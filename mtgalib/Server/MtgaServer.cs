using System.Text;
using System.Threading.Tasks;
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

        public Task<bool> ConnectTask()
        {
            _tcpConnection.Connect(_host, _port);
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            _tcpConnection.OnConnected += b => taskCompletionSource.SetResult(b);

            return taskCompletionSource.Task;
        }

        public Task<string> ReadResponseTask()
        {
            TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
            _tcpConnection.OnMsgReceived += (bytes, offset, length) =>
            {
                string msg = Encoding.UTF8.GetString(bytes, offset, length);
                taskCompletionSource.SetResult(msg);
            };

            return taskCompletionSource.Task;
        }

        public void Disconnect()
        {
            if (_tcpConnection.Connected)
                _tcpConnection.Close();
        }

        public void Send(string message)
        {
            _tcpConnection.SendAsync(Encoding.UTF8.GetBytes(message));
        }
    }
}
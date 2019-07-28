using System;
using System.Text;
using System.Threading.Tasks;
using mtgalib.Endpoint;
using mtgalib.Player;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace mtgalib.Server
{
    public class MtgaServer
    {
        private TcpConnection _tcpConnection;
        private string _host;
        private int _port;
        private int _messagesSentCounter;

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
            _tcpConnection.OnConnected = b => taskCompletionSource.SetResult(b);

            return taskCompletionSource.Task;
        }

        public void SetOnMsgSentAction(Action<byte[], int, int> action)
        {
            _tcpConnection.OnMsgSent = action;
        }

        public Task<string> ReadResponseTask()
        {
            TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
            _tcpConnection.OnMsgReceived = (bytes, offset, length) =>
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

        private void Send(string message)
        {
            _tcpConnection.SendAsync(Encoding.UTF8.GetBytes(message));
        }

        public void Authenticate(string ticket, string clientVersion)
        {
            SendRPC("Authenticate", new JObject
            {
                {"ticket", ticket},
                {"clientVersion", clientVersion}
            });
        }

        public void Authenticate(string ticket, Version clientVersion)
        {
            SendRPC("Authenticate", new JObject
            {
                {"ticket", ticket},
                {"clientVersion", $"{clientVersion.Patch}.{clientVersion.Meta}"}
            });
        }

        public async Task AuthenticateAsyncTask(string ticket)
        {
            string clientVersion = await new MtgDownloadsEndpoint().GetClientVersionAsyncTask(MtgDownloadsEndpoint.PLATFORM_WINDOWS);
            Authenticate(ticket, new Version(clientVersion));
        }

        private void SendRPC(string method, JToken parameters)
        {
            JObject rpcRequest = new JObject
            {
                {"jsonrpc", "2.0"},
                {"method", method},
                {"params", parameters},
                {"id", (_messagesSentCounter++).ToString()}
            };
            Send(JsonConvert.SerializeObject(rpcRequest));
        }
    }
}
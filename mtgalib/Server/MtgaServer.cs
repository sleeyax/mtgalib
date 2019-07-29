using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
        private readonly TcpConnection _tcpConnection;
        private readonly string _host;
        private readonly int _port;
        private int _messagesSentCounter;

        public MtgaServer(PlayerEnvironment playerEnvironment)
        {
            _tcpConnection = new TcpConnection();
            _host = playerEnvironment.Host;
            _port = playerEnvironment.Port;
        }

        public void SetOnMsgSentEvent(Action<byte[], int, int> action)
        {
            _tcpConnection.OnMsgSent += action;
        }

        public void SetOnMsgReceivedEvent(Action<byte[], int, int> action)
        {
            _tcpConnection.OnMsgReceived += action;
        }

        public void SetOnConnectionClosedEvent(Action<string> action)
        {
            _tcpConnection.OnClose += action;
        }

        public void SetOnConnectedEvent(Action<bool> action)
        {
            _tcpConnection.OnConnected += action;
        }

        public Task<bool> ConnectTask()
        {
            _tcpConnection.Connect(_host, _port);
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            _tcpConnection.OnConnected += b => taskCompletionSource.SetResult(b);

            return taskCompletionSource.Task;
        }

        public void Disconnect()
        {
            if (_tcpConnection.Connected)
                _tcpConnection.Close();
        }

        public Task<JsonRpcResponse> ReadResponseTask()
        {
            TaskCompletionSource<JsonRpcResponse> taskCompletionSource = new TaskCompletionSource<JsonRpcResponse>();
            Action<byte[], int, int> action = (bytes, offset, length) =>
            {
                string msg = Encoding.UTF8.GetString(bytes, offset, length);
                taskCompletionSource.SetResult(JsonConvert.DeserializeObject<JsonRpcResponse>(msg));
            };
            _tcpConnection.OnMsgReceived += action;
            // Remove event handler when the task finishes
            taskCompletionSource.Task.ContinueWith(t => _tcpConnection.OnMsgReceived -= action);
            return taskCompletionSource.Task;
        }

        public void Send(string message)
        {
            _tcpConnection.SendAsync(Encoding.UTF8.GetBytes(message));
        }

        public void SendRPC(string method, JToken parameters)
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
            Authenticate(ticket, $"{clientVersion.Patch}.{clientVersion.Meta}");
        }

        public async Task AuthenticateAsyncTask(string ticket)
        {
            string clientVersion = await new MtgDownloadsEndpoint().GetClientVersionAsyncTask(MtgDownloadsEndpoint.PLATFORM_WINDOWS);
            Authenticate(ticket, new Version(clientVersion));
        }

        public void Ping()
        {
            SendRPC("FrontDoor.Ping", new JObject());
        }

        public void Derp()
        {
            SendRPC("Derp", new JObject());
        }
    }
}
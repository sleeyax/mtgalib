using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mtgalib.Endpoint;
using mtgalib.Local;
using mtgalib.Player;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace mtgalib.Server
{
    public class GameServer
    {
        private readonly TcpConnection _tcpConnection;
        private readonly string _host;
        private readonly int _port;
        private int _messagesSentCounter;

        public GameServer(PlayerEnvironment playerEnvironment)
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
            _tcpConnection.Close();
        }

        private Task<JsonRpcResponse> ReadResponseTask()
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

        private void Send(string message)
        {
            _tcpConnection.SendAsync(Encoding.UTF8.GetBytes(message));
        }

        public async Task<JsonRpcResponse> SendAsyncTask(string message)
        {
            Send(message);
            return await ReadResponseTask();
        }

        public async Task<JsonRpcResponse> SendRpcJsonAsyncTask(string method, JToken parameters)
        {
            JObject rpcRequest = new JObject
            {
                {"jsonrpc", "2.0"},
                {"method", method},
                {"params", parameters},
                {"id", (_messagesSentCounter++).ToString()}
            };
            Send(JsonConvert.SerializeObject(rpcRequest));

            return await ReadResponseTask();
        }

        public async Task<JsonRpcResponse> AuthenticateAsyncTask(string ticket, string clientVersion)
        {
            if (clientVersion.Split('.').Length > 2)
            {
                Version version = new Version(clientVersion);
                clientVersion = $"{version.Patch}.{version.Meta}";
            }

            return await SendRpcJsonAsyncTask("Authenticate", new JObject
            {
                {"ticket", ticket},
                {"clientVersion", clientVersion}
            });
        }

        public async Task<JsonRpcResponse> AuthenticateAsyncTask(string ticket)
        {
            string clientVersion = await new MtgDownloadsEndpoint().GetLatestGameClientVersionAsyncTask(MtgDownloadsEndpoint.PLATFORM_WINDOWS);
            return await AuthenticateAsyncTask(ticket, clientVersion);
        }

        public async Task<JsonRpcResponse> PingAsyncTask()
        {
            return await SendRpcJsonAsyncTask("FrontDoor.Ping", new JObject());
        }

        public async Task<JsonRpcResponse> DerpAsyncTask()
        {
            return await SendRpcJsonAsyncTask("Derp", new JObject());
        }
    }
}
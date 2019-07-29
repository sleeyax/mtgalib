using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace mtgalib.Server
{
    internal class TcpConnection
    {
        public event Action<string> OnClose;
        public event Action<bool> OnConnected;
        public event Action<byte[], int, int> OnMsgReceived;
        public event Action<byte[], int, int> OnMsgSent;
        public string ConnectionStatus { get; private set; }
        public bool Connected => _socket != null && _socket.Connected;

        private Socket _socket;
        private SslStream _ssl;
        private byte[] _recvBuffer = new byte[5120];
        private byte[] _sendBuffer = new byte[5120];
        private int _incomingMsgBytesReceived;
        private byte _incomingMsgVersion;
        private int _incomingMsgLen;
        private ConcurrentQueue<byte[]> _msgSendQueue;
        private byte[] _outgoingMessage;
        private int _outgoingMessageRemainingBytes;
        private bool _canSend;
        private SocketAsyncEventArgs _socketAsyncEventArgs;
        private string _host;
        private int _port;
        private bool _stopSendThread;

        public TcpConnection()
        {
            ResetPending();
            ConnectionStatus = "Disconnected";
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += OnSocketConnectedEventHandler;
            _socketAsyncEventArgs.UserToken = this;
            _msgSendQueue = new ConcurrentQueue<byte[]>();
        }

        /// <summary>
        /// Connect to a remote server
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Connect(string host, int port)
        {
            _host = host;
            _port = port;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TranslateHostToIP(Dns.GetHostEntry(_host));
        }

        /// <summary>
        /// Close all active connections
        /// </summary>
        /// <param name="closeType"></param>
        public void Close(TcpConnectionCloseType closeType = TcpConnectionCloseType.Expected, string reason = "Normal close event")
        {
            if (_socket != null)
            {
                if (_socket.Connected && _ssl != null)
                {
                    _ssl.Flush();
                    _ssl.Close();
                }
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();

                _ssl?.Dispose();
                _socket.Dispose();
                _socket = null;
            }
            
            _stopSendThread = true;

            OnClose?.Invoke(reason);
        }

        private void TranslateHostToIP(IPHostEntry entry)
        {
            IPAddress ip = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            if (ip == null)
            {
                string reason = "Failed to translate server hostname to IP address";
                Close(TcpConnectionCloseType.Unexpected, reason);
                throw new Exception(reason);
            }

            // Connect to server using socket
            _socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(ip, _port);
            bool isPending = _socket.ConnectAsync(_socketAsyncEventArgs);
            ConnectionStatus = "Connecting";
            
            // Socket is already connected
            if (!isPending)
                ProcessSocketConnection(_socketAsyncEventArgs);
        }

        private static void OnSocketConnectedEventHandler(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Connect)
                throw new ArgumentException("Unexpected socket operation");

            ((TcpConnection) e.UserToken).ProcessSocketConnection(e);
        }

        private void ProcessSocketConnection(SocketAsyncEventArgs connectEventArgs)
        {
            // Double-check if the connection was successful
            if (connectEventArgs.SocketError != SocketError.Success)
            {
                Close(TcpConnectionCloseType.Unexpected, "Connection closed for no reason");
                throw new Exception("Connection closed for no reason");
            }

            // Try to establish a SSL connection 
            try
            {
                _ssl = new SslStream(new NetworkStream(_socket, false));
                ConnectionStatus = "AuthenticatingSSL";
                _ssl.BeginAuthenticateAsClient(_host, new X509CertificateCollection(), SslProtocols.Tls12, true, ar =>
                {
                    ((TcpConnection) ar.AsyncState).ProcessSslAuthentication(ar);
                }, this);
            }
            catch (Exception ex)
            {
                Close(TcpConnectionCloseType.Unexpected, "SSL authentication failed");
                throw new Exception("SSL authentication failed", ex);
            }
        }

        private void ProcessSslAuthentication(IAsyncResult ar)
        {
            try
            {
                _ssl.EndAuthenticateAsClient(ar);
            }
            catch (Exception)
            {
                Close(TcpConnectionCloseType.Unexpected, "Unknown error occurred while authenticating SSL connection");
                throw new Exception("Unknown error occurred while authenticating SSL connection");
            }

            if (!_ssl.IsEncrypted || !_ssl.IsSigned)
            {
                Close(TcpConnectionCloseType.Unexpected, "SSL connection not encrypted or signed!");
                throw new Exception("SSL connection not encrypted or signed!");
            }

            ConnectionStatus = "Connected";
            OnConnected?.Invoke(true);
            TrySendNextMessageThread();
            StartRead(0);
        }

        public void SendAsync(byte[] msg)
        {
            _msgSendQueue.Enqueue(msg);
        }

        private void AddHeader(int payloadLength)
        {
            _sendBuffer[0] = 1;
            Array.Copy(BitConverter.GetBytes(payloadLength), 0, _sendBuffer, 1, 4);
        }

        private void TrySendNextMessageThread()
        {
            new Thread(() =>
            {
                while (!_stopSendThread)
                {
                    try
                    {
                        _canSend = _msgSendQueue.TryDequeue(out _outgoingMessage);
                        if (_canSend)
                        {
                            // NOTE: resulting format will be : version (1) + outgoing message length + outgoing message
                            _outgoingMessageRemainingBytes = _outgoingMessage.Length;
                            AddHeader(_outgoingMessage.Length);
                            int num = 0;
                            while (_outgoingMessageRemainingBytes > 0)
                            {
                                int num2 = Math.Min(_outgoingMessageRemainingBytes, (num == 0) ? 5115 : 5120);
                                Array.Copy(_outgoingMessage, num, _sendBuffer, (num == 0) ? 5 : 0, num2);
                                _ssl.Write(_sendBuffer, 0, (num == 0) ? (num2 + 5) : num2);
                                OnMsgSent?.Invoke(_sendBuffer, 0, (num == 0) ? (num2 + 5) : num2);
                                _outgoingMessageRemainingBytes -= num2;
                                num += num2;
                            }
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Close(TcpConnectionCloseType.Unexpected, "Unexpected error while trying to send next message in queue");
                        throw new Exception("Unexpected error while trying to send next message in queue", ex);
                    }
                }
            }).Start();
        }

        private void TrySendNextMsg()
        {
            
            if (_stopSendThread)
                return;

            if (_outgoingMessageRemainingBytes == 0)
            {
                _canSend = _msgSendQueue.TryDequeue(out _outgoingMessage);
                while (!_canSend && !_stopSendThread)
                {
                    Thread.Sleep(1);
                    _canSend = _msgSendQueue.TryDequeue(out _outgoingMessage);
                }

                if (_stopSendThread)
                    return;
                
                if (_outgoingMessage != null)
                {
                    _outgoingMessageRemainingBytes = _outgoingMessage.Length;
                    AddHeader(_outgoingMessage.Length);
                    int num = Math.Min(_outgoingMessageRemainingBytes, 5120);
                    Array.Copy(_outgoingMessage, 0, _sendBuffer, 5, num);
                    StartWrite(0, num + 5);
                    _outgoingMessageRemainingBytes -= num;
                }
            }
            else
            {
                int sourceIndex = _outgoingMessage.Length - _outgoingMessageRemainingBytes;
                int num = Math.Min(_outgoingMessageRemainingBytes, 5120);
                Array.Copy(_outgoingMessage, sourceIndex, _sendBuffer, 0, num);
                StartWrite(0, num);
                _outgoingMessageRemainingBytes -= num;
            }
        }

        private void StartWrite(int offset, int count)
        {
            try
            {
                _ssl.Write(_sendBuffer, offset, count);
                OnMsgSent?.Invoke(_sendBuffer, offset, count);
                TrySendNextMsg();
            }
            catch (Exception ex)
            {
                Close(TcpConnectionCloseType.Unexpected, "Error when writing to SSL stream");
                throw new Exception("Error when writing to SSL stream", ex);
            }
        }

        private void StartRead(int offset)
        {
            try
            {
                _ssl.BeginRead(_recvBuffer, offset, _recvBuffer.Length - offset, ProcessRead, this);
            }
            catch (Exception ex)
            {
                Close(TcpConnectionCloseType.Unexpected, "Error when reading from SSL stream");
                throw new Exception("Error when reading from SSL stream", ex);
            }
        }

        private void ProcessRead(IAsyncResult ar)
        {
            try
            {
                int bytesRead = _ssl.EndRead(ar);
                ProcessReceive(bytesRead, _recvBuffer);
            }
            catch (Exception ex)
            {
                Close(TcpConnectionCloseType.Unexpected, "Error when reading from SSL stream");
                throw new Exception("Error when reading from SSL stream", ex);
            }
        }

        private void ResetPending()
        {
            _incomingMsgBytesReceived = 0;
            _incomingMsgVersion = 0;
            _incomingMsgLen = -1;
        }

        private void ProcessReceive(int bytesTransferred, byte[] buffer, bool isTest = false)
        {
            if (bytesTransferred == 0)
            {
                Close(TcpConnectionCloseType.Expected, "Closed by remote end");
                return;
            }

            if (_incomingMsgVersion == 0)
            {
                _incomingMsgVersion = _recvBuffer[0];
                _incomingMsgLen = -1;
            }

            _incomingMsgBytesReceived += bytesTransferred;
            while (true)
            {
                if (_incomingMsgLen == -1)
                {
                    byte incomingMsgVersion = _incomingMsgVersion;
                    if (incomingMsgVersion != 1)
                    {
                        Close(TcpConnectionCloseType.InvalidMessageFormat, "Received message has an invalid format");
                        return;
                    }

                    if (_incomingMsgBytesReceived > 4)
                    {
                        _incomingMsgLen = BitConverter.ToInt32(_recvBuffer, 1);
                        if (_recvBuffer.Length < _incomingMsgLen + 5)
                        {
                            Array.Resize(ref _recvBuffer, _incomingMsgLen + 5);
                        }
                    }

                    if (_incomingMsgLen == -1)
                    {
                        goto read;
                    }
                }

                int num = _incomingMsgLen + 5;
                if (_incomingMsgBytesReceived < num)
                {
                    goto read;
                }

                if (_incomingMsgBytesReceived >= num)
                {
                    OnMsgReceived?.Invoke(_recvBuffer, 5, _incomingMsgLen);
                    int num2 = 5 + _incomingMsgLen;
                    if (_incomingMsgBytesReceived <= num2)
                    {
                        goto reset;
                    }
                    int num3 = _incomingMsgBytesReceived - num2;
                    Array.Copy(_recvBuffer, num2, _recvBuffer, 0, num3);
                    ResetPending();
                    _incomingMsgVersion = _recvBuffer[0];
                    _incomingMsgBytesReceived = num3;
                }
            }
            reset:
            ResetPending();
            read:
            StartRead(_incomingMsgBytesReceived);
        }
    }
}
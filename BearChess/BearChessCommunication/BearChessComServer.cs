using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class BearChessComServer : IBearChessComServer
    {
        private TcpListener _listener;
        private readonly int _portNumber;
        private readonly ILogging _logging;
        private bool _stopServer;
        private bool _isRunning;
        private Thread _serverThread;

        public event EventHandler<string> ClientConnected;
        public event EventHandler<string> ClientDisconnected;
        public event EventHandler<BearChessServerMessage> ClientMessage;
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;
        public int CurrentPortNumber { get; set; }


        public bool IsRunning => _isRunning;

        public BearChessComServer(int portNumber, ILogging logging)
        {
            _portNumber = portNumber;
            _logging = logging;
            _listener = new TcpListener(IPAddress.Any, _portNumber);
            _stopServer = false;
            _isRunning = false;
        }



        public void StopServer()
        {
            if (_isRunning)
            {
                _stopServer = true;
            }
        }

        private void RunServerLoop()
        {
            try
            {
                _isRunning = true;
                _listener.Start();
                CurrentPortNumber = ((IPEndPoint)_listener.LocalEndpoint).Port;
                while (!_stopServer)
                {
                    if (!_listener.Pending())
                    {
                        Thread.Sleep(100);
                        if (_stopServer)
                        {
                            break;
                        }

                        continue;
                    }

                    var client = _listener.AcceptTcpClient();
                    var clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm))
                    {
                        IsBackground = true
                    };

                    clientThread.Start(client);
                }
            }
            finally
            {
                _listener.Stop();
                _isRunning = false;
                ServerStopped?.Invoke(this, null);
            }
        }

        public void RunServer()
        {
            if (!_isRunning)
            {
                _stopServer = false;
                _serverThread = new Thread(RunServerLoop)
                {
                    IsBackground = true
                };
                _serverThread.Start();
                ServerStarted?.Invoke(this, null);
            }
        }


        private void HandleClientComm(object client)
        {
            var addr = string.Empty;
            var tcpClient = (TcpClient)client;
            try
            {
                addr = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
                _logging.LogInfo($"Client connected: {addr}");
                ClientConnected?.Invoke(this, $"{addr}");
                using (var clientStream = tcpClient.GetStream())
                {

                    var message = new byte[4096];
                    int bytesRead;

                    while (!_stopServer)
                    {
                        bytesRead = 0;

                        try
                        {
                            //blocks until a client sends a message
                            bytesRead = clientStream.Read(message, 0, 4096);
                        }
                        catch
                        {
                            //a socket error has occured
                            break;
                        }

                        if (bytesRead == 0)
                        {
                            //the client has disconnected from the server
                            break;
                        }

                        //message has successfully been received
                        var msg = Encoding.Default.GetString(message, 0, bytesRead).Trim();
                        _logging?.LogDebug($"Received: {msg}");
                        var msgArray = msg.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < msgArray.Length; i++)
                        {
                            var clientMessage = JsonSerializer.Deserialize<BearChessServerMessage>(msgArray[i]);
                            if (clientMessage.ActionCode.Equals("CONNECT"))
                            {
                                var guid = Guid.NewGuid().ToString("N");
                                var connectMessage = new BearChessServerMessage() { Ack = "ACK", Address = guid, ActionCode = clientMessage.ActionCode, Message = clientMessage.Message };
                                var jsonString = JsonSerializer.Serialize(connectMessage);
                                var bufferConnect = Encoding.Default.GetBytes(jsonString);
                                clientStream.Write(bufferConnect, 0, bufferConnect.Length);
                                clientStream.Flush();
                                _logging?.LogDebug($"Send: {jsonString}");
                                ClientMessage?.Invoke(this, connectMessage);
                                continue;
                            }

                            ClientMessage?.Invoke(this, clientMessage);
                            var serverMessage = new BearChessServerMessage() { Ack = "ACK", ActionCode = clientMessage.ActionCode, Message = clientMessage.Message };
                            var serverString = JsonSerializer.Serialize(serverMessage);
                            var buffer = Encoding.Default.GetBytes(serverString);
                            clientStream.Write(buffer, 0, buffer.Length);
                            clientStream.Flush();
                            _logging?.LogDebug($"Send: {serverString}");
                        }
                    }
                }
            }
            finally
            {
                ClientDisconnected?.Invoke(this, $"{addr}");
                tcpClient.Dispose();

            }
        }
    }
}

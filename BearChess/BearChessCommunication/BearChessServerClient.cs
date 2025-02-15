using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class BearChessServerClient : IBearChessServerClient
    {
        private readonly string _hostName;
        private readonly int _portNumber;
        private readonly ILogging _logging;
        private readonly ConcurrentQueue<BearChessServerMessage> _messages = new ConcurrentQueue<BearChessServerMessage>();
        private readonly Thread _sendThread = null;
        private  Thread _receiveThread = null;
        private volatile bool _sending = true;
        private volatile bool _pauseSending = true;
        private  string _clientToken = string.Empty;

        public bool IsSending => !_pauseSending;
        public event EventHandler<BearChessServerMessage> ServerMessage;
        public event EventHandler Connected;
        public event EventHandler DisConnected;
        public volatile bool IsConnected = false;

        public BearChessServerClient(string hostName, int portNumber, ILogging logging)
        {
           
            _clientToken = "BearChess";
            _hostName = hostName;
            _portNumber = portNumber;
            _logging = logging;
            _sendThread = new Thread(SendKeepConnection)
            {
                IsBackground = true
            };
            _sendThread.Start();
        }

        public BearChessServerClient(ILogging logging)
        {
            _clientToken = "BearChess";
            _hostName = Configuration.Instance.GetConfigValue("BCServerHostname", "localhost");
            _portNumber = Configuration.Instance.GetIntValue("BCServerPortnumber", 8888);            
            _logging = logging;
            _sendThread = new Thread(SendKeepConnection)
            {
                IsBackground = true
            };
            _sendThread.Start();
        }


        public void SendToServer(BearChessServerMessage message)
        {
            if (_sending)
            {
                _messages.Enqueue(message);
            }
        }

        public void SendToServer(string action, string message)
        {
            if (_sending)
            {
                _messages.Enqueue(new BearChessServerMessage() {ActionCode = action, Message = message});
            }
        }

        public void StartSend(string clientName)
        {
            _sending = true;
            _pauseSending = false;
            _messages.Enqueue(new BearChessServerMessage() { ActionCode = "CONNECT", Message = clientName });
        }

        public void PauseSend()
        {
            _pauseSending = true;
        }

        public void StopSend()
        {
           
            while (_messages.TryDequeue(out _)) ;
            _messages.Enqueue(new BearChessServerMessage() { ActionCode = "DISCONNECT" });
        }

        private void Send()
        {
            try
            {
                while (_sending)
                {
                    Thread.Sleep(200);

                    if (_messages.Count == 0)
                    {
                        continue;
                    }

                    if (_pauseSending)
                    {
                        _messages.TryDequeue(out _);
                        continue;
                    }

                    if (_messages.TryDequeue(out BearChessServerMessage message))
                    {
                       

                        if (message.ActionCode.Equals("DISCONNECT"))
                        {
                            _pauseSending = true;
                            _sending = false;
                        }
                        try
                        {
                           
                            using (TcpClient client = new TcpClient(_hostName, _portNumber))
                            using (NetworkStream n = client.GetStream())
                            {
                                string jsonString = JsonSerializer.Serialize(message);
                                byte[] buffer = Encoding.Default.GetBytes(jsonString);
                                _logging?.LogDebug($"BCC: Try to send: {jsonString}");
                                n.Write(buffer, 0, buffer.Length);
                                n.Flush();
                                _logging?.LogDebug("BCC: Waiting for answer...");
                                var buffer2 = new byte[1024];
                                int received = n.Read(buffer2, 0, buffer2.Length);
                                if (received > 0)
                                {
                                    var msg = Encoding.Default.GetString(buffer2, 0, received).Trim();
                                    //Debug.WriteLine(System.Text.Encoding.Default.GetString(buffer2));
                                    _logging?.LogDebug($"BCC: Received: {msg}");
                                    var serverMessage = JsonSerializer.Deserialize<BearChessServerMessage>(msg);
                                    ServerMessage?.Invoke(this,serverMessage);
                                }

                                n.Close();
                                client.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logging?.LogError(ex);
                        }
                    }
                }
            }
            catch
            {
                //
            }
        }

        private void HandleReceiveMessages(object objectStream)
        {
            _logging?.LogDebug("BCC: Waiting for answers");
            NetworkStream netStream = objectStream as NetworkStream;
            try
            {
                while (true)
                {
                    var buffer2 = new byte[1024];
                    var received = netStream.Read(buffer2, 0, buffer2.Length);
                    if (received <= 0)
                    {
                        continue;
                    }
                    var msg = Encoding.Default.GetString(buffer2, 0, received).Trim();
                    _logging?.LogDebug($"BCC: Received: {msg}");
                    var serverMessage = JsonSerializer.Deserialize<BearChessServerMessage>(msg);
                    ServerMessage?.Invoke(this, serverMessage);
                    if (serverMessage.ActionCode.Equals("CONNECT"))
                    {
                        _clientToken = serverMessage.Address;
                        Configuration.Instance.SetConfigValue("BCClientToken", _clientToken);
                    }
                }
            }
            catch
            {
                //
            }
        }

        private void SendKeepConnection()
        {
            while (_sending)
            {
                Thread.Sleep(200);
                if (_pauseSending)
                {
                    continue;
                }
                try
                {
                    using (var client = new TcpClient(_hostName, _portNumber))
                    using (var netStream = client.GetStream())
                    {
                        _receiveThread = new Thread(new ParameterizedThreadStart(HandleReceiveMessages));
                        _receiveThread.IsBackground = true;
                        _receiveThread.Start(netStream);
                        IsConnected = true;
                        Connected?.Invoke(this,null);
                        while (_sending)
                        {
                            Thread.Sleep(200);
                            if (_messages.Count == 0)
                            {
                                continue;
                            }

                            if (_pauseSending)
                            {
                                _messages.TryDequeue(out _);
                                continue;
                            }

                            if (_messages.TryDequeue(out BearChessServerMessage message))
                            {
                                
                            }
                            if (message.ActionCode.Equals("DISCONNECT"))
                            {
                                _pauseSending = true;
                                _sending = false;
                            }

                            message.Address = _clientToken;
                            string jsonString = "#"+JsonSerializer.Serialize(message)+"#";
                            var buffer = Encoding.Default.GetBytes(jsonString);
                            netStream.Write(buffer, 0, buffer.Length);
                            netStream.Flush();
                            //_logging?.LogDebug("BCC: Waiting for answer...");
                            //var buffer2 = new byte[1024];
                            //var received = netStream.Read(buffer2, 0, buffer2.Length);
                            //if (received <= 0)
                            //{
                            //    continue;
                            //}
                            
                            //var msg = Encoding.Default.GetString(buffer2, 0, received).Trim();
                            //_logging?.LogDebug($"BCC: Received: {msg}");
                            //var serverMessage = JsonSerializer.Deserialize<BearChessServerMessage>(msg);
                            //ServerMessage?.Invoke(this, serverMessage);
                            //if (serverMessage.ActionCode.Equals("CONNECT"))
                            //{
                            //        _clientToken = serverMessage.Address;
                            //        Configuration.Instance.SetConfigValue("BCClientToken", _clientToken);
                            //}
                        }

                        netStream.Close();
                        client.Close();
                    }
                    IsConnected = false;
                    DisConnected?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    _logging?.LogError(ex);
                }
            }
            IsConnected = false;
            DisConnected?.Invoke(this, null);
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public interface IBearChessClient
    {
        void SendToServer(string message);
        void StopSend();
    }

    public class BearChessClient : IBearChessClient
    {
        private readonly string _hostName;
        private readonly int _portNumber;
        private readonly ILogging _logging;
        private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();
        private readonly Thread _sendThread = null;
        private volatile bool _sending = true;
        private volatile bool _pauseSending = true;

        public BearChessClient(string hostName, int portNumber, ILogging logging)
        {
            _hostName = hostName;
            _portNumber = portNumber;
            _logging = logging;
            _sendThread = new Thread(Send)
            {
                IsBackground = true
            };
            _sendThread.Start();
        }

        public BearChessClient(ILogging logging) : this("localhost", 51111, logging)
        {
        }

        public void SendToServer(string message)
        {
            if (_sending)
            {
                _messages.Enqueue(message);
            }
        }

        public void StartSend()
        {
            _sending = true;
            _pauseSending = false;
        }

        public void PauseSend()
        {
            _pauseSending = true;
        }

        public void StopSend()
        {
            _sending = false;
            while (_messages.TryDequeue(out _)) ;
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

                    if (_messages.TryPeek(out string message))
                    {
                        if (string.IsNullOrEmpty(message))
                        {
                            return;
                        }

                        try
                        {
                            _logging?.LogDebug($"Try to send: {message}");
                            using (TcpClient client = new TcpClient(_hostName, _portNumber))
                            using (NetworkStream n = client.GetStream())
                            {
                                byte[] buffer = Encoding.Default.GetBytes(message);
                                n.Write(buffer, 0, buffer.Length);
                                n.Flush();
                                var buffer2 = new byte[1024];
                                int received = n.Read(buffer2, 0, buffer2.Length);
                                if (received > 0)
                                {
                                    var msg = Encoding.Default.GetString(buffer2,0,received);
                                    //Debug.WriteLine(System.Text.Encoding.Default.GetString(buffer2));
                                    _logging?.LogDebug($"Received: {msg}");
                                    _messages.TryDequeue(out _);
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
    }
}
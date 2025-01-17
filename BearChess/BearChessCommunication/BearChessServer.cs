using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class BearChessServer : IBearChessServer
    {
        private TcpListener _server;
        private readonly int _portNumber;
        private readonly ILogging _logging;
        private bool _stopServer;
        private Thread _serverThread;

        public BearChessServer(int portNumber, ILogging logging)
        {
            _portNumber = portNumber;
            _logging = logging; 
            _server = new TcpListener(IPAddress.Any, portNumber);
            _stopServer = false;
        }

        public BearChessServer(ILogging logging) : this(51111, logging)
        {
        }


        public void StopServer()
        {
            _stopServer = true;
        }

        private void RunServerLoop()
        {
            try
            {
                _server.Start();
                while (!_stopServer)
                {
                    if (!_server.Pending())
                    {
                        Thread.Sleep(500);
                        if (_stopServer)
                        {
                            break;
                        }
                        continue;
                    }
                    TcpClient client = _server.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm))
                    {
                        IsBackground = true
                    };

                    clientThread.Start(client);
                }
            }
            finally
            {
                _server.Stop();
            }
        }

        public void RunServer()
        {
            _serverThread = new Thread(RunServerLoop)
            {
                IsBackground = true
            };
            _serverThread.Start();
        }


        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            try
            {               
                using (NetworkStream clientStream = tcpClient.GetStream())
                {

                    byte[] message = new byte[4096];
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


                    }
                }
            }
            finally
            {                
                tcpClient.Dispose();
            }
        }


    }
}
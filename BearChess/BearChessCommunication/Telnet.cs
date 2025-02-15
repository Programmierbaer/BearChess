using System;
using System.Net.Sockets;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    
    public  class Telnet 
    {
        private readonly string _hostname;
        private readonly int _port;

        enum Verbs
        {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }

        enum Options
        {
            SGA = 3
        }

        private TcpClient _tcpSocket;

        private int timeOutInMs = 100;

        public bool IsConnected => _tcpSocket != null && _tcpSocket.Connected;

        public Telnet(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
            _tcpSocket = null;

        }

        public bool Connect()
        {
            try
            {
                if (_tcpSocket == null)
                {
                    _tcpSocket = new TcpClient(_hostname, _port);
                    return true;
                }
                _tcpSocket.Connect(_hostname,_port);
                return true;
            }
            catch
            {
                //
            }

            return false;
        }

        public void DisConnect()
        {
            try
            {
                if (IsConnected)
                {
                    _tcpSocket.Close();
                    _tcpSocket.Dispose();
                    _tcpSocket = null;

                }
            }
            catch
            {
                //
            }
        }

        public string Login(string username, string password, int loginTimeOutInMs)
        {

            int oldTimeOutMs = timeOutInMs;
            timeOutInMs = loginTimeOutInMs;
            string s = Read();
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no login prompt");
            SendCommand(username + "\n");

            s += Read();
            if (!username.Equals(Constants.Guest,StringComparison.OrdinalIgnoreCase))
            {
                if (!s.TrimEnd().EndsWith(":"))
                    throw new Exception("Failed to connect : no password prompt");
                SendCommand(password + "\n");

                s += Read();
            }

            timeOutInMs = oldTimeOutMs;
            return s;
        }

        public void SendRaw(byte[] data)
        {
            try
            {
                if (!IsConnected)
                {
                    return;
                }

                _tcpSocket.GetStream().Write(data, 0, data.Length);
            }
            catch
            {
                DisConnect();
            }
        }

        public void SendCommand(string cmd)
        {
            try
            {
                if (!IsConnected)
                {
                    return;
                }

                cmd += "\n";
                byte[] buf = Encoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
                _tcpSocket.GetStream().Write(buf, 0, buf.Length);
            }
            catch 
            {
                DisConnect();
            }
        }

        public string Read()
        {
            try
            {
                if (!IsConnected)
                {
                    return null;
                }

              
                var sb = new StringBuilder();
                do
                {
                    ParseTelnet(sb);
                    System.Threading.Thread.Sleep(timeOutInMs);
                } while (_tcpSocket!=null && _tcpSocket.Available > 0);

                return sb.ToString();
            }
            catch 
            {
                DisConnect();
            }

            return null;
        }

        

        private void ParseTelnet(StringBuilder sb)
        {
            try
            {
                while (_tcpSocket.Available > 0)
                {
                    int input = _tcpSocket.GetStream().ReadByte();
                    switch (input)
                    {
                        case -1:
                            break;
                        case (int)Verbs.IAC:
                            // interpret as command
                            int inputVerb = _tcpSocket.GetStream().ReadByte();
                            if (inputVerb == -1) break;
                            switch (inputVerb)
                            {
                                case (int)Verbs.IAC:
                                    //literal IAC = 255 escaped, so append char 255 to string
                                    sb.Append(inputVerb);
                                    break;
                                case (int)Verbs.DO:
                                case (int)Verbs.DONT:
                                case (int)Verbs.WILL:
                                case (int)Verbs.WONT:
                                    // reply to all commands with "WONT", unless it is SGA (suppress go ahead)
                                    int inputOption = _tcpSocket.GetStream().ReadByte();
                                    if (inputOption == -1) break;
                                    _tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                    if (inputOption == (int)Options.SGA)
                                        _tcpSocket.GetStream()
                                                  .WriteByte(inputVerb == (int)Verbs.DO
                                                                 ? (byte)Verbs.WILL
                                                                 : (byte)Verbs.DO);
                                    else
                                        _tcpSocket.GetStream()
                                                  .WriteByte(inputVerb == (int)Verbs.DO
                                                                 ? (byte)Verbs.WONT
                                                                 : (byte)Verbs.DONT);
                                    _tcpSocket.GetStream().WriteByte((byte)inputOption);
                                    break;
                                default:
                                    break;
                            }

                            break;
                        default:
                            sb.Append((char)input);
                            break;
                    }
                }
            }
            catch 
            {
                DisConnect();
                //
            }
        }

    }
}

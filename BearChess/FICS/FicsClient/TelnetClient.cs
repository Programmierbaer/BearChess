using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public class TelnetClient : ITelnetClient
    {
        private readonly string _username;
        private readonly string _password;
        private readonly ILogging _logger;
        private readonly Telnet _telnet;
        private bool _stopReading;
        private readonly Thread _thread;
        private bool _sendUserName;
        private bool _sendPassword;
        

        public event EventHandler<string> ReadEvent;

        public bool IsLoggedIn => _sendUserName && _sendPassword;

        public TelnetClient(string hostname, int port, string username, string password, ILogging logger)
        {
            _username = username;
            _password = password;
            _logger = logger;
            _sendUserName = false;
            _sendPassword = false;
            _telnet = new Telnet(hostname, port);
            _stopReading = false;
            _thread = new Thread(HandleConnection) { IsBackground = true };
            _thread.Start();
        }

        public void Connect()
        {
            _telnet.Connect();
        }


        public void DisConnect()
        {
            _sendUserName = false;
            _sendPassword = false;
            _telnet.DisConnect();
        }

        public void Close()
        {
            _stopReading = true;
            _sendUserName = false;
            _sendPassword = false;
            _telnet.DisConnect();
        }

        public void Login(string username, string password)
        {
    
            try
            {
                _logger.LogDebug($"Login: {username}");
                var logMessage = _telnet.Login(username, password, 1000);
               _logger.LogInfo(logMessage);
               ReadEvent?.Invoke(this, logMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }

        public void Send(string command)
        {
            try
            {
                _logger.LogDebug($"Send: {command}");
                _telnet.SendCommand(command);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

        }

        private void SendNoLog(string command)
        {
            try
            {
                _telnet.SendCommand(command);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

        }

        private void HandleConnection()
        {
            while (!_stopReading)
            {
                if (_telnet != null && _telnet.IsConnected)
                {
                    string read =_telnet.Read();
                    if (!string.IsNullOrWhiteSpace(read))
                    {
                        //_logger.LogDebug($"Read: {read}");
                        if (read.Trim().ToLower().EndsWith(":"))
                        {
                            if (_sendUserName && !_sendPassword)
                            {
                                SendNoLog(_password);
                                _sendPassword = true;
                            }
                            if (!_sendUserName && !_sendPassword)
                            {
                                Send(_username);
                                _sendUserName = true;
                            }
                         
                        }
                        ReadEvent?.Invoke(this, read);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}

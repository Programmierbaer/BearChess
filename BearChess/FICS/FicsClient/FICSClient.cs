using System;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public class FICSClient : IFICSClient
    {
        private readonly ILogging _logger;
        private readonly ITelnetClient _telnetClient;

        public FICSClient(ITelnetClient telnetClient, ILogging logger)
        {
            _logger = logger;
            _telnetClient = telnetClient;
            _telnetClient.ReadEvent += telnetClient_ReadEvent;
        }

        public FICSClient(string hostname, int port, string username, string password, ILogging logger) : this(
            new TelnetClient(hostname, port, username, password, logger), logger)
        {
        }

        public event EventHandler<string> ReadEvent;

        public bool IsLoggedIn => _telnetClient.IsLoggedIn;

        public void Connect()
        {
            _telnetClient.Connect();
        }

        public void DisConnect()
        {
            _telnetClient.DisConnect();
        }

        public void Close()
        {
            _telnetClient.Close();
        }

        public void Login(string username, string password)
        {
            _telnetClient.Login(username, password);
        }

        public void Send(string command)
        {
            _telnetClient.Send(command);
        }

        private void telnetClient_ReadEvent(object sender, string e)
        {
            e = e.Replace("\n\r", Environment.NewLine);
            _logger?.LogDebug($"Read: {e}");
            ReadEvent?.Invoke(this, e);
        }
    }
}
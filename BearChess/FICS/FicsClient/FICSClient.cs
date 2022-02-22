using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public class FICSClient
    {
        private readonly string _hostname;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly ILogging _logger;
        private TelnetClient _telnetClient;

        public FICSClient(string hostname, int port, string username, string password, ILogging logger)
        {
            _hostname = hostname;
            _port = port;
            _username = username;
            _password = password;
            _logger = logger;
            _telnetClient = new TelnetClient(_hostname, _port, _username, _password, _logger);
            _telnetClient.ReadEvent += _telnetClient_ReadEvent;
        }

        private void _telnetClient_ReadEvent(object sender, string e)
        {
            //
        }
    }
}

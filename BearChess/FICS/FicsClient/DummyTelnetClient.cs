using System;

namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public class DummyTelnetClient : ITelnetClient
    {
        private bool _isLoggedIn = false;

        public event EventHandler<string> ReadEvent;

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => _isLoggedIn = value;
        }

        public void Connect()
        {
            _isLoggedIn = true;
        }

        public void DisConnect()
        {
            _isLoggedIn = false;
        }

        public void Close()
        {
            _isLoggedIn = false;
        }

        public void Login(string username, string password)
        {
            _isLoggedIn = true;
        }

        public void Send(string command)
        {
            if (command.Equals("getgame", StringComparison.OrdinalIgnoreCase))
            {
                ReadEvent?.Invoke(this, "Your getgame qualifies for camthejam's seek.");
                ReadEvent?.Invoke(this, "Creating: camthejam (1567) LarsBearchess (1285) rated blitz 3 0");
                ReadEvent?.Invoke(this, "games 31");
                ReadEvent?.Invoke(this, "Game 31: A disconnection will be considered a forfeit.");
                ReadEvent?.Invoke(this, "fics%");
                ReadEvent?.Invoke(this, "31 1567 camthejam   1285 LarsBearch [ br  3   0]   3:00 -  3:00 (39-39) W:  1");
                ReadEvent?.Invoke(this, "1 game displayed (of 81 in progress).");
                ReadEvent?.Invoke(this, "fics%");
            }
        }
    }
}
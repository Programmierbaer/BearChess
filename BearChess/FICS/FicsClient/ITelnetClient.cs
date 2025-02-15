using System;

namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public interface ITelnetClient
    {
        event EventHandler<string> ReadEvent;
        bool IsLoggedIn { get; }
        void Connect();
        void DisConnect();
        void Close();
        void Login(string username, string password);
        void Send(string command);
    }
}
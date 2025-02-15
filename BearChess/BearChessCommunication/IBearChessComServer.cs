using System;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public interface IBearChessComServer
    {

        event EventHandler<string> ClientConnected;
        event EventHandler<string> ClientDisconnected;
        event EventHandler<BearChessServerMessage> ClientMessage;
        event EventHandler ServerStarted;
        event EventHandler ServerStopped;
        bool IsRunning { get; }
        void RunServer();
        void StopServer();

        int CurrentPortNumber { get; set; }
    }
}
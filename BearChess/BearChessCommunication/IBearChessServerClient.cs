using System;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public interface IBearChessServerClient
    {
        void SendToServer(BearChessServerMessage message);
        void SendToServer(string action, string message);
        void StopSend();
        void StartSend(string clientName);
        void PauseSend();
        bool IsSending { get; }

        event EventHandler<BearChessServerMessage> ServerMessage;
        event EventHandler Connected;
        event EventHandler DisConnected;
    }
}
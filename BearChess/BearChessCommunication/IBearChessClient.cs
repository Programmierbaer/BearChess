namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public interface IBearChessClient
    {
        void SendToServer(string message);
        void StopSend();
        void StartSend();
        void PauseSend();
        bool IsSending { get; }
    }
}
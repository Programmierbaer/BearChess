using System;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface ITournamentInfoWindow
    {
        event EventHandler StopTournament;
        void AddResult(string result, int[] pairing);
        void Show();
        void Close();
        void CloseInfoWindow();
        void SetReadOnly();

        void SetForRunning();

        event EventHandler Closed;
    }
}
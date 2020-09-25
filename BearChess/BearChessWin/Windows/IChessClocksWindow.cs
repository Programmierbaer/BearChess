using System;
using System.ComponentModel;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public interface IChessClocksWindow
    {
        event EventHandler TimeOutEvent;
        ClockTime GetClockTime();
        void SetTime(ClockTime clockTime, int extraSeconds = 0);
        void SetTime(int hh, int mm, int ss, int extraSeconds= 0);
        void Reset();
        void Stop();
        void Go();
        void Show();
        void Hide();
        void Close();
        bool CountDown { get; set; }
        event CancelEventHandler Closing;
        event EventHandler Closed;
        double Top { get; set; }
        double Left { get; set; }
        double Height { get; set; }
        double Width { get; set; }

    }
}
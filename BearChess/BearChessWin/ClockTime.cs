using System;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class ClockTime
    {
        public int Hour { get; }
        public int Minute { get; }
        public int Second { get; }
        public int TotalSeconds => Hour * 60 * 60 + Minute * 60 + Second;

        public ClockTime(int hour, int minute, int second)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        public ClockTime(DateTime dateTime)
        {
            Hour = dateTime.Hour;
            Minute = dateTime.Minute;
            Second = dateTime.Second;
        }
    }
}

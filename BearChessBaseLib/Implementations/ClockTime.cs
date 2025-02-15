using System;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    [Serializable]
    public class ClockTime
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
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

        public ClockTime(TimeSpan timeSpan)
        {
            Hour = timeSpan.Hours;
            Minute = timeSpan.Minutes;
            Second = timeSpan.Seconds;
        }


        public ClockTime()
        {

        }
    }
}

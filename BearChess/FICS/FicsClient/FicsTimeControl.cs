using System;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    [Serializable]
    public class FicsTimeControl
    {
        public FicsTimeControl()
        {
            TimePerGame = 5;
            IncSecond = 0;
            Color = Fields.COLOR_EMPTY;
            RatedGame = false;
        }

        public FicsTimeControl(int presetNumber) : this()
        {
            switch (presetNumber)
            {
                case 0:
                    TimePerGame = 5;
                    IncSecond = 0;
                    Color = Fields.COLOR_EMPTY;
                    RatedGame = false;
                    break;
                case 1:
                    TimePerGame = 5;
                    IncSecond = 5;
                    Color = Fields.COLOR_EMPTY;
                    RatedGame = false;
                    break;
                case 2:
                    TimePerGame = 10;
                    IncSecond = 5;
                    Color = Fields.COLOR_EMPTY;
                    RatedGame = false;
                    break;
            }
        }

        public int TimePerGame { get; set; }
        public int IncSecond { get; set; }
        public int Color { get; set; }
        public bool RatedGame { get; set; }

        public string GetTimeSetting()
        {
            var result = $"{TimePerGame}";
            if (IncSecond > 0)
            {
                result = $"{result}/{IncSecond}";
            }
            result += " min";
            if (RatedGame)
            {
                result += " rated";
            }
            else
            {
                result += " unrated";
            }

            if (Color == Fields.COLOR_WHITE)
            {
                result += " white";
            }

            if (Color == Fields.COLOR_BLACK)
            {
                result += " black";
            }

            return result;
        }

        public string GetMatchCommand(string user)
        {
            var result = $"match {user.Trim()}";

            if (RatedGame)
            {
                result += " rated";
            }
            else
            {
                result += " unrated";
            }

            result += $" {TimePerGame}";
            if (IncSecond > 0)
            {
                result += $" {IncSecond}";
            }
            
            if (Color == Fields.COLOR_WHITE)
            {
                result += " white";
            }

            if (Color == Fields.COLOR_BLACK)
            {
                result += " black";
            }

            return result;
        }

        public string GetSeekCommand()
        {
            var result = $"seek {TimePerGame}";
            if (IncSecond > 0)
            {
                result = $"{result} {IncSecond}";
            }

            if (RatedGame)
            {
                result += " rated";
            }
            else
            {
                result += " unrated";
            }

            if (Color == Fields.COLOR_WHITE)
            {
                result += " white";
            }

            if (Color == Fields.COLOR_BLACK)
            {
                result += " black";
            }

            return result;
        }
    }
}
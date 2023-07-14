using System;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{


    [Serializable]
    public class TimeControl
    {
        public TimeControlEnum TimeControlType { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int HumanValue { get; set; }
        public bool WaitForMoveOnBoard { get; set; }
        public bool AverageTimInSec { get; set; }
        public bool AllowTakeBack { get; set; }
        public bool TournamentMode { get; set; }
        public bool SeparateControl { get; set; }
    }

    public static class TimeControlHelper
    {
        public static string GetDescription(TimeControl timeControl)
        {
            if (timeControl == null)
            {
                return string.Empty;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                return $"{timeControl.Value1} minutes per game";
            }
            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                return $"{timeControl.Value1} minutes per game with {timeControl.Value2} seconds increment";
            }
            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                return $"{timeControl.Value1} moves in {timeControl.Value2} minutes";
            }
            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                return timeControl.Value2==0 ? $"Average {timeControl.Value1} seconds per move" : $"Average {timeControl.Value1} minutes per move";
            }
            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                return "Adapted time";
            }
            return string.Empty;
        }
    }
}

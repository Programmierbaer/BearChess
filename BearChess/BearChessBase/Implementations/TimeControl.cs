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
}

using System;

namespace www.SoLaNoSoft.com.BearChessWin
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
        public bool PonderWhite { get; set; }
        public bool PonderBlack { get; set; }
    }
}

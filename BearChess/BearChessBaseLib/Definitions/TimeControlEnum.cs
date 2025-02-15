using System;

namespace www.SoLaNoSoft.com.BearChessBase.Definitions
{
    [Serializable]
    public enum TimeControlEnum
    {
        TimePerGame,
        TimePerGameIncrement,
        TimePerMoves,
        AverageTimePerMove,
        Adapted,
        Depth,
        Nodes,
        Movetime,
        NoControl
    }
}
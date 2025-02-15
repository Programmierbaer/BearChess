using System;
using System.Resources;
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
        public static TimeControl GetDefaultTimeControl()
        {
            TimeControl defauktTC = new TimeControl();
            defauktTC.AllowTakeBack = false;
            defauktTC.AverageTimInSec = false;
            defauktTC.HumanValue = 0;
            defauktTC.SeparateControl = false;
            defauktTC.TimeControlType = TimeControlEnum.TimePerGame;
            defauktTC.TournamentMode = false;
            defauktTC.Value1 = 5;
            defauktTC.Value2 = 0;
            defauktTC.WaitForMoveOnBoard = true;
            return defauktTC;
        }

        public static string GetDescription(TimeControl timeControl, ResourceManager rm)
        {
        
            if (timeControl == null)
            {
                return string.Empty;
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                return timeControl.Value1 == 1 ?  $"{timeControl.Value1} {rm.GetString("TCMinutePerGame")}" : $"{timeControl.Value1} {rm.GetString("TCMinutesPerGame")}"; 
            }
            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                var helpText = timeControl.Value1 == 1
                    ? $"{timeControl.Value1} {rm.GetString("TCMinutePerGame")} {rm.GetString("With")}"
                    : $"{timeControl.Value1} {rm.GetString("TCMinutesPerGame")} {rm.GetString("With")}";
                helpText += timeControl.Value2 == 1
                    ? $"{timeControl.Value2} {rm.GetString("TCSecondIncrement")} "
                    : $"{timeControl.Value2} {rm.GetString("TCSecondsIncrement")}";
                return  helpText;
            }
            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                var helpText = timeControl.Value1 == 1
                    ? $"{timeControl.Value1} {rm.GetString("MoveIn")} "
                    : $"{timeControl.Value1} {rm.GetString("MovesIn")} ";
                helpText += timeControl.Value2 == 1
                    ? $"{timeControl.Value2} {rm.GetString("TCMinute")}"
                    : $"{timeControl.Value2} {rm.GetString("TCMinutes")}";
                return helpText;
            }
            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                var helpText = $"{rm.GetString("Average")} {timeControl.Value1} ";
                if (timeControl.AverageTimInSec)
                {
                    if (timeControl.Value1 == 1)
                    {
                        helpText += rm.GetString("TCSecondPerMove");
                    }
                    else
                    {
                        helpText += rm.GetString("TCSecondsPerMove");
                    }
                }
                else
                {
                    if (timeControl.Value1 == 1)
                    {
                        helpText += rm.GetString("TCMinutePerMove");
                    }
                    else
                    {
                        helpText += rm.GetString("TCMinutesPerMove");
                    }
                }
                return helpText;
            }
            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                return rm.GetString("AdaptedTime");
            }
            if (timeControl.TimeControlType == TimeControlEnum.Depth)
            {
                return timeControl.Value1==1 ?  $"{rm.GetString("Depth")} {timeControl.Value1} {rm.GetString("Ply")}" : $"{rm.GetString("Depth")} {timeControl.Value1} {rm.GetString("Plies")}"; 
            }
            if (timeControl.TimeControlType == TimeControlEnum.Nodes)
            {
                return $"{rm.GetString("Nodes")} {timeControl.Value1} {rm.GetString("Nodes")}";
            }
            return string.Empty;
        }
    }
}

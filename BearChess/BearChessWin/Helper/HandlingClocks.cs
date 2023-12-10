using System.Windows.Input;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class HandlingClocks
    {
        public static void SetClocks(TimeControl timeControl, TimeControl timeControlBlack, IChessClocksWindow chessClocksWindowWhite,
                                     IChessClocksWindow chessClocksWindowBlack, bool whiteIsPlayer, bool blackIsPlayer,
                                     IElectronicChessBoard eChessBoard)

        {
            if (chessClocksWindowBlack == null || chessClocksWindowWhite == null)
            {
                return;
            }
            int hourWhite = 0;
            int minuteWhite = 0;
            int secWhite = 0;
            int hourBlack = 0;
            int minuteBlack = 0;
            int secBlack = 0;
            chessClocksWindowWhite.CountDown = true;
            chessClocksWindowBlack.CountDown = true;

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                var hour = timeControl.Value1 / 60;
                var hourH = (timeControl.Value1 + timeControl.HumanValue) / 60;
              

                if (timeControl.HumanValue > 0 && whiteIsPlayer)
                {
                    chessClocksWindowWhite.SetTime(hourH, timeControl.Value1 + timeControl.HumanValue - hourH * 60,
                                                   0);
                    chessClocksWindowWhite.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.HumanValue} extra minutes");
                }
                else
                {
                    chessClocksWindowWhite.SetTime(hour, timeControl.Value1 - hour * 60, 0);
                    chessClocksWindowWhite.SetTooltip($"{timeControl.Value1} minutes per game");
                }

                hourWhite = hour;
                minuteWhite = timeControl.Value1 - hour * 60;
                secWhite = 0;
              //  eChessBoard?.SetClock(hour, timeControl.Value1 - hour * 60,0,hourH, timeControl.Value1 - hour * 60,0);
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                var hour = timeControl.Value1 / 60;
                var hourH = (timeControl.Value1 + timeControl.HumanValue) / 60;
              
                if (timeControl.HumanValue > 0 && whiteIsPlayer)
                {
                    chessClocksWindowWhite.SetTime(hourH, timeControl.Value1 + timeControl.HumanValue - hourH * 60,
                                                   0, timeControl.Value2);
                    chessClocksWindowWhite.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.Value2} sec. increment and {timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    chessClocksWindowWhite.SetTime(hour, timeControl.Value1 - hour * 60, 0, timeControl.Value2);
                    chessClocksWindowWhite.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.Value2} sec. increment ");
                }
                hourWhite = hour;
                minuteWhite = timeControl.Value1 - hour * 60;
                secWhite = 0;
               // eChessBoard?.SetClock(hour, timeControl.Value1 - hour * 60, 0, hourH, timeControl.Value1 - hour * 60, 0);
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                var hour = timeControl.Value2 / 60;
                var hourH = (timeControl.Value2 + timeControl.HumanValue) / 60;
             

                if (timeControl.HumanValue > 0 && whiteIsPlayer)
                {
                    chessClocksWindowWhite.SetTime(hourH, timeControl.Value2 + timeControl.HumanValue - hourH * 60, 0);
                    chessClocksWindowWhite.SetTooltip(
                        $"{timeControl.Value1} moves in {timeControl.Value2} minutes with {timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    chessClocksWindowWhite.SetTime(hour, timeControl.Value2 - hour * 60, 0);
                    chessClocksWindowWhite.SetTooltip($"{timeControl.Value1} moves in {timeControl.Value2} minutes ");
                    chessClocksWindowWhite.SetInfo($"{timeControl.Value1} moves in {timeControl.Value2} minutes ");
                }
                hourWhite = hour;
                minuteWhite = timeControl.Value1 - hour * 60;
                secWhite = 0;
                //eChessBoard?.SetClock(hour, timeControl.Value2 - hour * 60, 0, hourH, timeControl.Value2 - hour * 60, 0);
            }

            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                chessClocksWindowWhite.SetTime(0, 0, 0);
                chessClocksWindowWhite.CountDown = false;
                var secOrMin = timeControl.AverageTimInSec ? "sec." : "min.";
                chessClocksWindowWhite.SetTooltip($"Average {timeControl.Value1} {secOrMin} per move ");
            }

            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                chessClocksWindowWhite.SetTime(0, 0, 0);
                chessClocksWindowWhite.CountDown = false;
                chessClocksWindowWhite.SetTooltip("Adapted time ");
            }
            if (timeControl.TimeControlType == TimeControlEnum.Depth)
            {
                chessClocksWindowWhite.SetTime(0, 0, 0);
                chessClocksWindowWhite.CountDown = false;
                chessClocksWindowWhite.SetTooltip($"Search depth {timeControl.Value1}");
            }
            if (timeControl.TimeControlType == TimeControlEnum.Nodes)
            {
                chessClocksWindowWhite.SetTime(0, 0, 0);
                chessClocksWindowWhite.CountDown = false;
                chessClocksWindowWhite.SetTooltip($"Search nodes {timeControl.Value1}");
            }
            if (timeControl.TimeControlType == TimeControlEnum.Movetime)
            {
                chessClocksWindowWhite.SetTime(0, 0, 0);
                chessClocksWindowWhite.CountDown = false;
                chessClocksWindowWhite.SetTooltip($"{timeControl.Value1} sec. per move");
            }


            if (timeControlBlack.TimeControlType == TimeControlEnum.TimePerGame)
            {
                var hour = timeControlBlack.Value1 / 60;
                var hourH = (timeControlBlack.Value1 + timeControlBlack.HumanValue) / 60;
                if (timeControl.HumanValue > 0 && blackIsPlayer)
                {
                    chessClocksWindowBlack.SetTime(hourH, timeControlBlack.Value1 + timeControl.HumanValue - hourH * 60,
                                                   0);
                    chessClocksWindowBlack.SetTooltip(
                        $"{timeControlBlack.Value1} minutes per game with {timeControl.HumanValue} extra minutes");
                }
                else
                {
                    chessClocksWindowBlack.SetTime(hour, timeControlBlack.Value1 - hour * 60, 0);
                    chessClocksWindowBlack.SetTooltip($"{timeControlBlack.Value1} minutes per game");
                }

                hourBlack = hourH;
                minuteBlack = timeControlBlack.Value1 - hour * 60;
                secBlack = 0;
                //eChessBoard?.SetClock(hour, timeControl.Value1 - hour * 60, 0, hourH, timeControlBlack.Value1 - hour * 60, 0);
            }

            if (timeControlBlack.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                var hour = timeControlBlack.Value1 / 60;
                var hourH = (timeControlBlack.Value1 + timeControl.HumanValue) / 60;
                if (timeControl.HumanValue > 0 && blackIsPlayer)
                {
                    chessClocksWindowBlack.SetTime(hourH, timeControlBlack.Value1 + timeControlBlack.HumanValue - hourH * 60,
                                                   0, timeControlBlack.Value2);
                    chessClocksWindowBlack.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControlBlack.Value2} sec. increment and {timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    chessClocksWindowBlack.SetTime(hour, timeControlBlack.Value1 - hour * 60, 0, timeControlBlack.Value2);
                    chessClocksWindowBlack.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControlBlack.Value2} sec. increment ");
                }
                hourBlack = hourH;
                minuteBlack = timeControlBlack.Value1 - hour * 60;
                secBlack = 0;

               // eChessBoard?.SetClock(hour, timeControl.Value1 - hour * 60, 0, hourH, timeControl.Value1 - hour * 60, 0);
            }

            if (timeControlBlack.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                var hour = timeControlBlack.Value2 / 60;
                var hourH = (timeControlBlack.Value2 + timeControlBlack.HumanValue) / 60;
                if (timeControl.HumanValue > 0 && blackIsPlayer)
                {
                    chessClocksWindowBlack.SetTime(hourH, timeControlBlack.Value2 + timeControlBlack.HumanValue - hourH * 60,
                                                   0);
                    chessClocksWindowBlack.SetTooltip(
                        $"{timeControlBlack.Value1} moves in {timeControlBlack.Value2} minutes with {timeControlBlack.HumanValue} extra minutes ");
                }
                else
                {
                    chessClocksWindowBlack.SetTime(hour, timeControlBlack.Value2 - hour * 60, 0);
                    chessClocksWindowBlack.SetTooltip($"{timeControlBlack.Value1} moves in {timeControlBlack.Value2} minutes ");
                    chessClocksWindowBlack.SetInfo($"{timeControlBlack.Value1} moves in {timeControlBlack.Value2} minutes ");
                }

                hourBlack = hourH;
                minuteBlack = timeControlBlack.Value2 - hour * 60;
                secBlack = 0;
              //  eChessBoard?.SetClock(hour, timeControl.Value2 - hour * 60, 0, hourH, timeControl.Value2 - hour * 60, 0);
            }

            if (timeControlBlack.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                chessClocksWindowBlack.SetTime(0, 0, 0);
                chessClocksWindowBlack.CountDown = false;
                var secOrMin = timeControlBlack.AverageTimInSec ? "sec." : "min.";
                chessClocksWindowBlack.SetTooltip($"Average {timeControlBlack.Value1} {secOrMin} per move");
            } 
            if (timeControlBlack.TimeControlType == TimeControlEnum.Adapted)
            {
                chessClocksWindowBlack.SetTime(0, 0, 0);
                chessClocksWindowBlack.CountDown = false;
                chessClocksWindowBlack.SetTooltip("Adapted time ");
            }
            if (timeControlBlack.TimeControlType == TimeControlEnum.Depth)
            {
                chessClocksWindowBlack.SetTime(0, 0, 0);
                chessClocksWindowBlack.CountDown = false;
                chessClocksWindowBlack.SetTooltip($"Search depth {timeControlBlack.Value1}");
            }
            if (timeControlBlack.TimeControlType == TimeControlEnum.Nodes)
            {
                chessClocksWindowBlack.SetTime(0, 0, 0);
                chessClocksWindowBlack.CountDown = false;
                chessClocksWindowBlack.SetTooltip($"Search nodes {timeControlBlack.Value1}");
            }
            if (timeControlBlack.TimeControlType == TimeControlEnum.Movetime)
            {
                chessClocksWindowBlack.SetTime(0, 0, 0);
                chessClocksWindowBlack.CountDown = false;
                chessClocksWindowBlack.SetTooltip($"{timeControlBlack.Value1} sec. per move");
            }

        }
    }
}
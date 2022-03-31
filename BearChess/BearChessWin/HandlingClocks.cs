using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class HandlingClocks
    {
        public static void SetClocks(TimeControl timeControl, IChessClocksWindow chessClocksWindowWhite,
                                     IChessClocksWindow chessClocksWindowBlack, bool whiteIsPlayer, bool blackIsPlayer)
        {

            chessClocksWindowWhite.CountDown = true;
            chessClocksWindowBlack.CountDown = true;

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGame)
            {
                var hour = timeControl.Value1 / 60;
                var hourH = (timeControl.Value1 + timeControl.HumanValue) / 60;
                if (timeControl.HumanValue > 0 && blackIsPlayer)
                {
                    chessClocksWindowBlack?.SetTime(hourH, timeControl.Value1 + timeControl.HumanValue - hourH * 60,
                                                    0);
                    chessClocksWindowBlack?.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.HumanValue} extra minutes");
                }
                else
                {
                    chessClocksWindowBlack?.SetTime(hour, timeControl.Value1 - hour * 60, 0);
                    chessClocksWindowBlack?.SetTooltip($"{timeControl.Value1} minutes per game");
                }

                if (timeControl.HumanValue > 0 && whiteIsPlayer)
                {
                    chessClocksWindowWhite?.SetTime(hourH, timeControl.Value1 + timeControl.HumanValue - hourH * 60,
                                                    0);
                    chessClocksWindowWhite?.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.HumanValue} extra minutes");
                }
                else
                {
                    chessClocksWindowWhite?.SetTime(hour, timeControl.Value1 - hour * 60, 0);
                    chessClocksWindowWhite?.SetTooltip($"{timeControl.Value1} minutes per game");
                }
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerGameIncrement)
            {
                var hour = timeControl.Value1 / 60;
                var hourH = (timeControl.Value1 + timeControl.HumanValue) / 60;
                if (timeControl.HumanValue > 0 && blackIsPlayer)
                {
                    chessClocksWindowBlack?.SetTime(hourH, timeControl.Value1 + timeControl.HumanValue - hourH * 60,
                                                    0, timeControl.Value2);
                    chessClocksWindowBlack?.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.Value2} sec. increment and {timeControl.HumanValue} extra minutes ");

                }
                else
                {
                    chessClocksWindowBlack?.SetTime(hour, timeControl.Value1 - hour * 60, 0, timeControl.Value2);
                    chessClocksWindowBlack?.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.Value2} sec. increment ");
                }

                if (timeControl.HumanValue > 0 && whiteIsPlayer)
                {
                    chessClocksWindowWhite?.SetTime(hourH, timeControl.Value1 + timeControl.HumanValue - hourH * 60,
                                                    0, timeControl.Value2);
                    chessClocksWindowWhite?.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.Value2} sec. increment and {timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    chessClocksWindowWhite?.SetTime(hour, timeControl.Value1 - hour * 60, 0, timeControl.Value2);
                    chessClocksWindowWhite?.SetTooltip(
                        $"{timeControl.Value1} minutes per game with {timeControl.Value2} sec. increment ");
                }
            }

            if (timeControl.TimeControlType == TimeControlEnum.TimePerMoves)
            {
                var hour = timeControl.Value2 / 60;
                var hourH = (timeControl.Value2 + timeControl.HumanValue) / 60;
                if (timeControl.HumanValue > 0 && blackIsPlayer)
                {
                    chessClocksWindowBlack?.SetTime(hourH, timeControl.Value2 + timeControl.HumanValue - hourH * 60,
                                                    0);
                    chessClocksWindowBlack?.SetTooltip(
                        $"{timeControl.Value1} moves in {timeControl.Value2} minutes with {timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    chessClocksWindowBlack?.SetTime(hour, timeControl.Value2 - hour * 60, 0);
                    chessClocksWindowBlack?.SetTooltip($"{timeControl.Value1} moves in {timeControl.Value2} minutes ");
                }

                if (timeControl.HumanValue > 0 && whiteIsPlayer)
                {
                    chessClocksWindowWhite?.SetTime(hourH, timeControl.Value2 + timeControl.HumanValue - hourH * 60,
                                                    0);
                    chessClocksWindowWhite?.SetTooltip(
                        $"{timeControl.Value1} moves in {timeControl.Value2} minutes with {timeControl.HumanValue} extra minutes ");
                }
                else
                {
                    chessClocksWindowWhite?.SetTime(hour, timeControl.Value2 - hour * 60, 0);
                    chessClocksWindowWhite?.SetTooltip($"{timeControl.Value1} moves in {timeControl.Value2} minutes ");
                }
            }

            if (timeControl.TimeControlType == TimeControlEnum.AverageTimePerMove)
            {
                chessClocksWindowBlack?.SetTime(0, 0, 0);
                chessClocksWindowWhite?.SetTime(0, 0, 0);
                chessClocksWindowWhite.CountDown = false;
                chessClocksWindowBlack.CountDown = false;
                string secOrMin = timeControl.AverageTimInSec ? "sec." : "min.";
                chessClocksWindowWhite?.SetTooltip($"Average {timeControl.Value1} {secOrMin} per move ");
                chessClocksWindowBlack?.SetTooltip($"Average {timeControl.Value1} {secOrMin} per move");
            }

            if (timeControl.TimeControlType == TimeControlEnum.Adapted)
            {
                chessClocksWindowBlack?.SetTime(0, 0, 0);
                chessClocksWindowWhite?.SetTime(0, 0, 0);
                chessClocksWindowWhite.CountDown = false;
                chessClocksWindowBlack.CountDown = false;
                chessClocksWindowWhite?.SetTooltip("Adapted time ");
                chessClocksWindowBlack?.SetTooltip("Adapted time ");
            }

        }
    }
}

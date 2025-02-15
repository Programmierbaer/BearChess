using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase
{
    public class MaterialHelper
    {
        public class WhiteBlackLine
        {
            public string BlackLine
            {
                get;
                set;
            } = string.Empty;

            public string WhiteLine
            {
                get;
                set;
            } = string.Empty;
        }

        public static WhiteBlackLine GeTopBottomLineForMoves(Move[] playedMoveList)
        {
            var topBottomLine = new WhiteBlackLine();
            var capturedFigures = new Dictionary<int, Dictionary<string, int>>
            {
                [Fields.COLOR_WHITE] = new Dictionary<string, int>
                {
                    ["q"] = 0,
                    ["r"] = 0,
                    ["b"] = 0,
                    ["n"] = 0,
                    ["p"] = 0
                },
                [Fields.COLOR_BLACK] = new Dictionary<string, int>
                {
                    ["Q"] = 0,
                    ["R"] = 0,
                    ["B"] = 0,
                    ["N"] = 0,
                    ["P"] = 0
                }
            };
            if (playedMoveList.Length > 0)
            {
                foreach (var move in playedMoveList)
                {
                    if (move == null)
                    {
                        continue;
                    }
                    if (move.CapturedFigure != FigureId.NO_PIECE)
                    {
                        var capturedFigure = FigureId.FigureIdToFenCharacter[move.CapturedFigure];
                        capturedFigures[move.FigureColor][capturedFigure]++;
                    }
                }

                foreach (var c in "qrbnp".ToCharArray())
                {
                    var blackFigure = c.ToString();
                    var whiteFigure = blackFigure.ToUpper();
                    for (var i = 0; i < capturedFigures[Fields.COLOR_WHITE][blackFigure]; i++)
                    {
                        topBottomLine.BlackLine += blackFigure;
                        var x = capturedFigures[Fields.COLOR_BLACK][whiteFigure];
                        if (capturedFigures[Fields.COLOR_BLACK][whiteFigure] < i+1)
                        {
                            topBottomLine.WhiteLine += "-";
                        }
                        else
                        {
                            topBottomLine.WhiteLine += whiteFigure;
                        }
                    }
                    for (var i = capturedFigures[Fields.COLOR_WHITE][blackFigure]; i < capturedFigures[Fields.COLOR_BLACK][whiteFigure]; i++)
                    {
                        topBottomLine.BlackLine += "-";
                        topBottomLine.WhiteLine += whiteFigure;

                    }

                }

            }
            return topBottomLine;
        }
    }
}

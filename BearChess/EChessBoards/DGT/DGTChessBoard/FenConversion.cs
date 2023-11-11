using System;
using System.Linq;

namespace www.SoLaNoSoft.com.BearChess.DGTChessBoard
{
    public static class FenConversions
    {

        static readonly string[] ranks = { "A", "B", "C", "D", "E", "F", "G", "H" };

        [Flags]
        public enum MyEnum
        {
            None = 0x0,
            WhiteShortCastling = 0x1,
            WhiteLongCastling = 0x2,
            BlackToMove = 0x8,
            BlackShortCastling = 0x10,
            BlackLongCastling = 0x20
        }

        

        public static string GetPiecesFen(string dgtFen, bool reversedOrder, bool playWithWhite)
        {
            string result = string.Empty;
            if (reversedOrder)
            {
                if (playWithWhite)
                {
                    for (int i = 57; i > 0; i -= 8)
                    {
                        var substring = dgtFen.Substring(i, 8);
                        result += GetFenLine(substring);
                    }
                }
                else
                {
                    for (int i = 1; i < 58; i += 8)
                    {
                        var substring = dgtFen.Substring(i, 8);
                        result += GetFenLine(string.Concat(substring.Reverse()));
                    }
                }

                return result.Substring(0, result.Length - 1);
            }

            if (playWithWhite)
            {
                for (int i = 1; i < 58; i += 8)
                {

                    var substring = dgtFen.Substring(i, 8);
                    result += GetFenLine(string.Concat(substring.Reverse()));
                }

                return result.Substring(0, result.Length - 1);
            }
            for (int i = 57; i > 0; i -= 8)
            {
                var substring = dgtFen.Substring(i, 8);
                result += GetFenLine(substring);
            }
            return result.Substring(0, result.Length - 1);

        }

        private static string GetFenLine(string substring)
        {
            var result = string.Empty;
            var noFigureCounter = 0;
            for (var i = substring.Length - 1; i >= 0; i--)
            {
                var piecesFromCode = substring[i];
                if (piecesFromCode == '.')
                {
                    noFigureCounter++;
                    continue;
                }

                if (noFigureCounter > 0)
                {
                    result += noFigureCounter;
                }

                noFigureCounter = 0;
                result += piecesFromCode;
            }

            if (noFigureCounter > 0)
            {
                result += noFigureCounter;
            }

            return result + "/";
        }



      
    }
}

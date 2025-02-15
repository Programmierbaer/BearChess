using System;
using System.Linq;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard
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

        public static string GetChessLinkFen(string fen)
        {
            
            string result = string.Empty;
            var strings = fen.Split(" ".ToCharArray());
            var lines = strings[0].Split("/".ToCharArray());
            for (int i = 0; i < lines.Length; i++)
            {
                var substring = lines[i];
                result += GetChessLinkFenLine(substring);
            }

            var attr = MyEnum.None;
            if (strings[1].Equals("b"))
            {
                attr |= MyEnum.BlackToMove;
            }

            if (strings[2].Contains("K"))
            {
                attr |= MyEnum.WhiteShortCastling;
            }

            if (strings[2].Contains("Q"))
            {
                attr |= MyEnum.WhiteLongCastling;
            }

            if (strings[2].Contains("k"))
            {
                attr |= MyEnum.BlackShortCastling;
            }

            if (strings[2].Contains("q"))
            {
                attr |= MyEnum.BlackLongCastling;
            }

            string format = Enum.Format(typeof(MyEnum), attr, "X");
            format = format.Substring(format.Length - 2);
            if (strings[3].Equals("-"))
            {
                format += "00";
            }
            else
            {
                string row = (int.Parse(strings[3][1].ToString()) - 1)+ Array.IndexOf(ranks, strings[3][0].ToString().ToUpper()).ToString();
                format += row;
            }


            if (int.TryParse(strings[4], out int h))
            {
                format += h.ToString("X4");
            }

            if (int.TryParse(strings[5], out int m))
            {
                format += m.ToString("X4");
            }

            return $"{result}{format}";
        }

        public static string GetPiecesFen(string chessLinkFen, bool reversedOrder, bool playWithWhite)
        {
            string result = string.Empty;
            if (reversedOrder)
            {
                if (playWithWhite)
                {
                    for (int i = 57; i > 0; i -= 8)
                    {
                        var substring = chessLinkFen.Substring(i, 8);
                        result += GetFenLine(substring);
                    }
                }
                else
                {
                    for (int i = 1; i < 58; i += 8)
                    {
                        var substring = chessLinkFen.Substring(i, 8);
                        result += GetFenLine(string.Concat(substring.Reverse()));
                    }
                }

                return result.Substring(0, result.Length - 1);
            }

            for (int i = 1; i < 58; i += 8)
            {
                var substring = chessLinkFen.Substring(i, 8);
                result += GetFenLine(string.Concat(substring.Reverse()));
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



        private static string GetChessLinkFenLine(string substring)
        {
            var result = string.Empty;
            var noFigureCounter = 0;
            for (var i = 0; i<substring.Length; i++)
            {
                var piecesFromCode = substring[i];
                if (int.TryParse(piecesFromCode.ToString(), out noFigureCounter))
                {
                    result += new string('.', noFigureCounter);

                }
                else
                {
                    result += piecesFromCode;
                }

            }

            return result;
        }
    }
}

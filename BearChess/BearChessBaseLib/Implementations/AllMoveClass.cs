using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class AllMoveClass
    {
        public int MoveNumber { get; }
        private readonly Dictionary<int, Move> _moves = new Dictionary<int, Move>();
        private readonly Dictionary<int, string> _fens = new Dictionary<int, string>();

        public AllMoveClass(int moveNumber)
        {
            MoveNumber = moveNumber;
        }

        public void SetMove(int color, Move move, string fenPosition)
        {
            _moves[color] = (Move)move.Clone();
            _fens[color] = fenPosition;
        }

        public Move GetMove(int color)
        {
            return _moves.ContainsKey(color) ? _moves[color] : null;
        }

        public string GetFen(int color)
        {
            return _fens.TryGetValue(color, out var fen) ? fen : null;
        }

      
        public string GetMoveString(bool longFormat = true, DisplayCountryType countryType=DisplayCountryType.GB)
        {
            Move move;
            if (_moves.ContainsKey(Fields.COLOR_BLACK))
            {
                move = _moves[Fields.COLOR_BLACK];
            }
            else
            {
                move = _moves[Fields.COLOR_WHITE];
            }

            var result = FigureId.FigureIdToFenCharacter[move.Figure].ToUpper();
            if (result.Equals("P"))
            {
                result = " ";
            }
            else
            {
                result = DisplayCountryHelper.CountryLetter(result, countryType);
            }

            if (longFormat)
            {
                result += move.FromFieldName.ToLower();
            }
            else
            {
                result += move.ShortMoveIdentifier;
            }

            if (move.CapturedFigure == FigureId.NO_PIECE)
            {
                if (longFormat)
                {
                    result += "-";
                }
            }
            else
            {
                result += "x";
            }

            if (move.Figure.Equals(FigureId.WHITE_KING) || move.Figure.Equals(FigureId.BLACK_KING))
            {
                if (move.FromField == Fields.FE1)
                {
                    if (move.ToField == Fields.FG1)
                    {
                        return "   0-0 " + move.CheckOrMateSign;
                    }
                    if (move.ToField == Fields.FC1)
                    {
                        return " 0-0-0 " + move.CheckOrMateSign;
                    }
                }
                if (move.FromField == Fields.FE8)
                {
                    if (move.ToField == Fields.FG8)
                    {
                        return "   0-0 " + move.CheckOrMateSign;
                    }
                    if (move.ToField == Fields.FC8)
                    {
                        return " 0-0-0 " + move.CheckOrMateSign;
                    }
                }
            }
            string p = string.Empty;
            if (move.PromotedFigure != FigureId.NO_PIECE)
            {
                p = DisplayCountryHelper.CountryLetter(FigureId.FigureIdToFenCharacter[move.PromotedFigure].ToUpper(), countryType);
            }

            result += move.ToFieldName.ToLower() + p + move.CheckOrMateSign;
            return result;
        }
    }
}
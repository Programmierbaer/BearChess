using System;
using System.Collections.Generic;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class FastChessBoard
    {
        private readonly Dictionary<string, string> _allFields = new Dictionary<string, string>();
        private DisplayFigureType _displayFigureType;
        private DisplayMoveType _displayMoveType;
        private DisplayCountryType _displayCountryType;

        public FastChessBoard()
        {
            Init(Array.Empty<string>());
            _displayFigureType = DisplayFigureType.Symbol;
            _displayMoveType = DisplayMoveType.FromToField;
            _displayCountryType = DisplayCountryType.GB;
        }

      

        public void SetDisplayTypes(DisplayFigureType figureType, DisplayMoveType moveType, DisplayCountryType countryType)
        {
            _displayFigureType = figureType;
            _displayMoveType = moveType;
            _displayCountryType = countryType;
        }

      

        public void Init(string[] allMoves)
        {
           
            _allFields.Clear();
            _allFields["a1"] = FenCodes.WhiteRook;
            _allFields["b1"] = FenCodes.WhiteKnight;
            _allFields["c1"] = FenCodes.WhiteBishop;
            _allFields["d1"] = FenCodes.WhiteQueen;
            _allFields["e1"] = FenCodes.WhiteKing;
            _allFields["h1"] = FenCodes.WhiteRook;
            _allFields["g1"] = FenCodes.WhiteKnight;
            _allFields["f1"] = FenCodes.WhiteBishop;
            _allFields["a8"] = FenCodes.BlackRook;
            _allFields["b8"] = FenCodes.BlackKnight;
            _allFields["c8"] = FenCodes.BlackBishop;
            _allFields["d8"] = FenCodes.BlackQueen;
            _allFields["e8"] = FenCodes.BlackKing;
            _allFields["h8"] = FenCodes.BlackRook;
            _allFields["g8"] = FenCodes.BlackKnight;
            _allFields["f8"] = FenCodes.BlackBishop;
            _allFields["a2"] = FenCodes.WhitePawn;
            _allFields["b2"] = FenCodes.WhitePawn;
            _allFields["c2"] = FenCodes.WhitePawn;
            _allFields["d2"] = FenCodes.WhitePawn;
            _allFields["e2"] = FenCodes.WhitePawn;
            _allFields["f2"] = FenCodes.WhitePawn;
            _allFields["g2"] = FenCodes.WhitePawn;
            _allFields["h2"] = FenCodes.WhitePawn;
            _allFields["a7"] = FenCodes.BlackPawn;
            _allFields["b7"] = FenCodes.BlackPawn;
            _allFields["c7"] = FenCodes.BlackPawn;
            _allFields["d7"] = FenCodes.BlackPawn;
            _allFields["e7"] = FenCodes.BlackPawn;
            _allFields["f7"] = FenCodes.BlackPawn;
            _allFields["g7"] = FenCodes.BlackPawn;
            _allFields["h7"] = FenCodes.BlackPawn;
            foreach (var move in allMoves)
            {
                GetMove(move.ToLower());
            }
        }

        public void Init(string fenPosition, string[] allMoves)
        {
            SetPosition(fenPosition);
            foreach (var move in allMoves)
            {
                GetMove(move.ToLower());
            }
        }

        public string GetPositionHashCode()
        {
          
            string result = string.Empty;
            foreach (string k in  _allFields.Keys.OrderBy(k => k) )
            {
                if (!string.IsNullOrEmpty(_allFields[k]))
                    result += k + _allFields[k];
            }
            return result;
        }

        private void SetPosition(string fenPosition)
        {
            _allFields.Clear();
            if (string.IsNullOrWhiteSpace(fenPosition))
            {
                return;
            }

            var epdArray = fenPosition.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var positionArray = epdArray[0].Split("/".ToCharArray());
            var rIndex = 9;
            const string tRows = "abcdefgh";
            for (var i = 0; i < positionArray.Length; i++)
            {
                rIndex--;
                positionArray[i] = positionArray[i].Replace("1", "_");
                positionArray[i] = positionArray[i].Replace("2", "__");
                positionArray[i] = positionArray[i].Replace("3", "___");
                positionArray[i] = positionArray[i].Replace("4", "____");
                positionArray[i] = positionArray[i].Replace("5", "_____");
                positionArray[i] = positionArray[i].Replace("6", "______");
                positionArray[i] = positionArray[i].Replace("7", "_______");
                positionArray[i] = positionArray[i].Replace("8", "________");
                for (var c = 0; c < positionArray[i].Length; c++)
                {
                    var tmpFigure = positionArray[i].Substring(c, 1);
                    var tmpPos = tRows.Substring(c, 1) + rIndex;
                    if (tmpFigure == "_")
                    {
                        continue;
                    }

                    _allFields[tmpPos] = tmpFigure;
                }
            }
        }

        public void SetMove(string move)
        {
            var fromField = move.Substring(0, 2).ToLower();
            var toField = move.Substring(2, 2).ToLower();
            var figureOnField = _allFields[fromField];
            _allFields[toField] = figureOnField;
            _allFields[fromField] = string.Empty;
            if (figureOnField.Equals(FenCodes.WhiteKing))
            {
                if (fromField.Equals("e1") && toField.Equals("g1"))
                {
                    _allFields["f1"] = FenCodes.WhiteRook;
                    _allFields["h1"] = string.Empty;
                }

                if (fromField.Equals("e1") && toField.Equals("c1"))
                {
                    _allFields["d1"] = FenCodes.WhiteRook;
                    _allFields["a1"] = string.Empty;
                }
            }

            if (figureOnField.Equals(FenCodes.BlackKing))
            {
                if (fromField.Equals("e8") && toField.Equals("g8"))
                {
                    _allFields["f8"] = FenCodes.BlackRook;
                    _allFields["h8"] = string.Empty;
                }

                if (fromField.Equals("e8") && toField.Equals("c8"))
                {
                    _allFields["d8"] = FenCodes.BlackRook;
                    _allFields["a8"] = string.Empty;
                }
            }
        }

        public string GetMove(string move)
        {
            if (string.IsNullOrWhiteSpace(move))
            {
                return string.Empty;
            }

            var captureSign = _displayMoveType == DisplayMoveType.FromToField ? "-" : string.Empty;
            var fromField = move.Substring(0, 2);
            var toField = move.Substring(2, 2);
            if (!_allFields.ContainsKey(fromField))
            {
                return string.Empty;
            }

            var figureOnField = _allFields[fromField];
            if (string.IsNullOrWhiteSpace(figureOnField))
            {
                return string.Empty;
            }

            if (_allFields.ContainsKey(toField))
            {
                if (!_allFields[toField].Equals(""))
                {
                    captureSign = "x";
                }
            }

            _allFields[toField] = figureOnField;
            _allFields[fromField] = string.Empty;
            if (figureOnField.Equals(FenCodes.WhiteKing))
            {
                if (fromField.Equals("e1") && toField.Equals("g1"))
                {
                    _allFields["f1"] = FenCodes.WhiteRook;
                    _allFields["h1"] = string.Empty;
                    return "0-0";
                }

                if (fromField.Equals("e1") && toField.Equals("c1"))
                {
                    _allFields["d1"] = FenCodes.WhiteRook;
                    _allFields["a1"] = string.Empty;
                    return "0-0-0";
                }
            }

            if (figureOnField.Equals(FenCodes.BlackKing))
            {
                if (fromField.Equals("e8") && toField.Equals("g8"))
                {
                    _allFields["f8"] = FenCodes.BlackRook;
                    _allFields["h8"] = string.Empty;
                    return "0-0";
                }

                if (fromField.Equals("e8") && toField.Equals("c8"))
                {
                    _allFields["d8"] = FenCodes.BlackRook;
                    _allFields["a8"] = string.Empty;
                    return "0-0-0";
                }
            }

            if (figureOnField.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                figureOnField = _displayMoveType == DisplayMoveType.FromToField ? string.Empty : captureSign=="x" ? fromField.Substring(0, 1) : string.Empty;
            }
            else
            {
                if (_displayFigureType == DisplayFigureType.Symbol)
                {
                    figureOnField = FontConverter.ConvertFont(figureOnField, string.Empty);
                }
                else
                {
                    figureOnField = DisplayCountryHelper.CountryLetter(figureOnField, _displayCountryType).ToUpper();
                }
            }
            if (_displayMoveType==DisplayMoveType.FromToField)
              return $"{figureOnField}{fromField}{captureSign}{toField}";
            return $"{figureOnField}{captureSign}{toField}";
        }
    }
}
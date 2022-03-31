using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    public class PgnCreator
    {
        private readonly List<Move> _allMoves;
        private readonly List<string> _allPgnMoves;
        private IChessBoard _chessBoard;

      

        public PgnCreator()
        {
            _allPgnMoves = new List<string>();
            _allMoves = new List<Move>();
        }

        public string[] GetAllMoves()
        {
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _allPgnMoves.Clear();
            foreach (var move in _allMoves)
            {
                _allPgnMoves.Add(ConvertToPgnMove(move));
            }

            return _allPgnMoves.ToArray();
        }

        public void AddMove(Move move)
        {
            _allMoves.Add(move);
        }

        public string GetMoveList()
        {
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            var sb = new StringBuilder();
            var moveCnt = 0;
            var newMove = true;
            foreach (var move in _allMoves)
            {
                var m = ConvertToPgnMove(move);
                if (newMove)
                {
                    moveCnt++;
                    sb.Append($"{moveCnt}.{m}");
                    newMove = false;
                }
                else
                {
                    sb.Append($" {m} ");
                    newMove = true;
                }
            }

            return sb.ToString();
        }

        private bool GetCastle(Move move, out string castle)
        {
            castle = string.Empty;
            switch (move.FromField)
            {
                case Fields.FE1 when move.ToField.Equals(Fields.FG1):
                    castle = "O-O";
                    return true;
                case Fields.FE1 when move.ToField.Equals(Fields.FC1):
                    castle = "O-O-O";
                    return true;
                case Fields.FE8 when move.ToField.Equals(Fields.FG8):
                    castle = "O-O";
                    return true;
                case Fields.FE8 when move.ToField.Equals(Fields.FC8):
                    castle = "O-O-O";
                    return true;
                default:
                    return false;
            }
        }

        private string ConvertToPgnMove(Move move)
        {
            var pgnMove = string.Empty;
            var figureFromField = _chessBoard.GetFigureOn(move.FromField);

            if (figureFromField.GeneralFigureId == FigureId.PAWN)
            {
                if (move.CapturedFigure != FigureId.NO_PIECE)
                {
                    pgnMove = $"{move.FromFieldName.Substring(0, 1).ToLower()}x{move.ToFieldName.ToLower()}";
                }
                else
                {

                    pgnMove = move.ToFieldName.ToLower();
                }

                if (move.PromotedFigure != FigureId.NO_PIECE)
                {
                    pgnMove += $"={FigureId.FigureIdToFenCharacter[move.PromotedFigure].ToUpper()}";
                }
            }
            else
            {
                if (figureFromField.GeneralFigureId == FigureId.KING)
                {
                    if (GetCastle(move, out var castle))
                    {
                        pgnMove = castle;
                    }
                }

                if (string.IsNullOrWhiteSpace(pgnMove))
                {
                    var moveList = _chessBoard.GenerateMoveList();
                    var tmpMove = moveList.FirstOrDefault(m => m.ToField == move.ToField &&
                                                               m.FromField != move.FromField &&
                                                               _chessBoard.GetFigureOn(m.FromField).GeneralFigureId ==
                                                               figureFromField.GeneralFigureId);
                    if (tmpMove != null)
                    {
                        var tmpFigure = _chessBoard.GetFigureOn(tmpMove.FromField);
                        if (tmpFigure.GeneralFigureId == figureFromField.GeneralFigureId &&
                            tmpFigure.Color == figureFromField.Color)
                        {
                            var fieldToLineFrom = Fields.FieldToLines[figureFromField.Field];
                            var fieldToLineTmp = Fields.FieldToLines[tmpFigure.Field];
                            if (fieldToLineTmp != fieldToLineFrom)
                            {
                                pgnMove =
                                    $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}{move.FromFieldName.Substring(0, 1).ToLower()}{move.ToFieldName.ToLower()}".Trim();
                            }
                            else
                            {
                                pgnMove =
                                    $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}{move.FromFieldName.Substring(1, 1).ToLower()}{move.ToFieldName.ToLower()}".Trim();
                            }
                        }
                    }
                    else
                    {
                        if (move.CapturedFigure != FigureId.NO_PIECE)
                        {
                            pgnMove =
                                $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}x{move.ToFieldName.ToLower()}".Trim();
                        }
                        else
                        {
                            pgnMove =
                                $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}{move.ToFieldName.ToLower()}".Trim();
                        }
                    }
                }
            }

            _chessBoard.MakeMove(move);
            var generateMoveList = _chessBoard.GenerateMoveList();
            if (_chessBoard.IsInCheck(Fields.COLOR_WHITE) || _chessBoard.IsInCheck(Fields.COLOR_BLACK))
            {
                var isMate = true;
                foreach (var move1 in _chessBoard.CurrentMoveList)
                {
                    var chessBoard = new ChessBoard();
                    chessBoard.Init(_chessBoard);
                    chessBoard.MakeMove(move1);
                    chessBoard.GenerateMoveList();
                    if (chessBoard.IsInCheck(chessBoard.EnemyColor))
                    {
                        continue;
                    }

                    isMate = false;
                    break;
                }

                pgnMove += isMate ? "#" : "+";
            }
            string mComment = string.IsNullOrWhiteSpace(move.Comment) ? string.Empty : "{"+ReplaceNagInComment(move.Comment)+"}"; 
            string mBestline = string.IsNullOrWhiteSpace(move.BestLine) ? string.Empty : "{"+move.Score.ToString(CultureInfo.InvariantCulture)+"} (" + move.BestLine+")";
            //string mScore = move.IsEngineMove ? move.Score.ToString(CultureInfo.InvariantCulture) : string.Empty;
            return $"{pgnMove} {GetNAG(move.MoveSymbol)} {GetNAG(move.EvaluationSymbol)} {mComment} {mBestline}".Trim();
        }

        private string ReplaceNagInComment(string comment)
        {
            var commentStrings = comment.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < commentStrings.Length; i++)
            {
                string commentString = commentStrings[i];
                if (PgnDefinitions.SymbolToNag.ContainsKey(commentString))
                {
                    commentStrings[i] = PgnDefinitions.SymbolToNag[commentString];
                }
            }

            return string.Join(" ", commentStrings);
        }

        private string GetNAG(string symbol)
        {
            if (!string.IsNullOrWhiteSpace(symbol) && PgnDefinitions.SymbolToNag.ContainsKey(symbol))
            {
                return PgnDefinitions.SymbolToNag[symbol];
            }

            return string.Empty;
        }
    }
}
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    public abstract class AbstractPgnCreator
    {
        protected  string _fenStartPosition;
        protected  List<Move> _allMoves;
        protected  List<string> _allPgnMoves;
        protected IChessBoard _chessBoard;
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

        protected string ConvertToPgnMove(Move move, int moveCnt)
        {
            var pgnMove = string.Empty;
            if (move == null)
            {
                return pgnMove;
            }
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
            string mComment = string.IsNullOrWhiteSpace(move.Comment) ? string.Empty : "{" + ReplaceNagInComment(move.Comment) + "}";
            string mBestLine = string.IsNullOrWhiteSpace(move.BestLine) ? string.Empty : "{" + move.Score.ToString(CultureInfo.InvariantCulture) + "} (" + AddMoveNumberToBestLine(move.BestLine, moveCnt, move.FigureColor) + ")";
            string emt = string.IsNullOrEmpty(move.ElapsedMoveTime)
                             ? string.Empty
                             : "{[%emt " + move.ElapsedMoveTime + "]}";
            return $"{pgnMove} {GetNAG(move.MoveSymbol)} {GetNAG(move.EvaluationSymbol)} {mComment} {mBestLine} {emt}".Trim();
        }

        private string AddMoveNumberToBestLine(string bestLine, int moveCnt, int currentColor)
        {
            string result = string.Empty;
            foreach (var move in bestLine.Split(" ".ToCharArray()).ToArray())
            {
                if (currentColor == Fields.COLOR_WHITE)
                {
                    result += $"{moveCnt}. {move}";
                    moveCnt++;
                    currentColor = Fields.COLOR_BLACK;
                }
                else
                {
                    result += $" {move} ";
                    currentColor = Fields.COLOR_WHITE;
                }

            }

            return result;
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
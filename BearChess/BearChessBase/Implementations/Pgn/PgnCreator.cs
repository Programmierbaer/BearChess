using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.Pgn
{
    public class PgnCreator
    {
        private readonly IChessBoard _chessBoard;
        private List<string> _allMoves;

        public PgnCreator()
        {
            _allMoves = new List<string>();
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
        }

        public string[] GetAllMoves => _allMoves.ToArray();

        public void AddMove(IMove move)
        {
            string pgnMove = string.Empty;
            IChessFigure figureFromField = _chessBoard.GetFigureOn(move.FromField);
         
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
                    pgnMove += FigureId.FigureIdToFenCharacter[move.PromotedFigure].ToUpper();
                }

            }
            else
            {
                if (figureFromField.GeneralFigureId == FigureId.KING)
                {
                    if (GetCastle(move, out string rochade))
                    {
                        pgnMove = rochade;
                    }
                }

                if (string.IsNullOrWhiteSpace(pgnMove))
                {
                    List<IMove> moveList = _chessBoard.GenerateMoveList();
                    IMove tmpMove = moveList.FirstOrDefault(m => m.ToField == move.ToField &&
                                                                 m.FromField != move.FromField &&
                                                                 _chessBoard.GetFigureOn(m.FromField).GeneralFigureId ==
                                                                 figureFromField.GeneralFigureId);
                    if (tmpMove != null)
                    {
                        IChessFigure tmpFigure = _chessBoard.GetFigureOn(tmpMove.FromField);
                        if (tmpFigure.GeneralFigureId == figureFromField.GeneralFigureId &&
                            tmpFigure.Color == figureFromField.Color)
                        {
                            Fields.Lines fieldToLineFrom = Fields.FieldToLines[figureFromField.Field];
                            Fields.Lines fieldToLineTmp = Fields.FieldToLines[tmpFigure.Field];
                            if (fieldToLineTmp != fieldToLineFrom)
                            {
                                pgnMove =
                                    $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}{move.FromFieldName.Substring(0, 1).ToLower()}{move.ToFieldName.ToLower()}";
                            }
                            else
                            {
                                pgnMove =
                                    $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}{move.FromFieldName.Substring(1, 1).ToLower()}{move.ToFieldName.ToLower()}";
                            }

                        }
                    }
                    else
                    {
                        if (move.CapturedFigure != FigureId.NO_PIECE)
                        {
                            pgnMove =
                                $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}x{move.ToFieldName.ToLower()}";
                        }
                        else
                        {
                            pgnMove =
                                $"{FigureId.FigureIdToFenCharacter[figureFromField.FigureId].ToUpper()}{move.ToFieldName.ToLower()}";
                        }
                    }
                }
            }

            _chessBoard.MakeMove(move);
            var generateMoveList = _chessBoard.GenerateMoveList();
            if (_chessBoard.IsInCheck(Fields.COLOR_WHITE) || _chessBoard.IsInCheck(Fields.COLOR_BLACK))
            {
                bool isMate = true;
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

            _allMoves.Add(pgnMove);
        }

        public string GetMoveList()
        {
            StringBuilder sb = new StringBuilder();
            int moveCnt = 0;
            bool newMove = true;
            foreach (var s in _allMoves)
            {
                if (newMove)
                {
                    moveCnt++;
                    sb.Append($"{moveCnt}.{s}");
                    newMove = false;
                }
                else
                {
                    sb.Append($" {s} ");
                    newMove = true;
                }
            }

            return sb.ToString();
        }

        private bool GetCastle(IMove move, out string rochade)
        {
            rochade = string.Empty;
            if (move.FromField.Equals(Fields.FE1) && move.ToField.Equals(Fields.FG1))
            {
                rochade = "0-0";
                return true;
            }
            if (move.FromField.Equals(Fields.FE1) && move.ToField.Equals(Fields.FC1))
            {
                rochade = "0-0-0";
                return true;
            }
            if (move.FromField.Equals(Fields.FE8) && move.ToField.Equals(Fields.FG8))
            {
                rochade = "0-0";
                return true;
            }
            if (move.FromField.Equals(Fields.FE8) && move.ToField.Equals(Fields.FC8))
            {
                rochade = "0-0-0";
                return true;
            }

            return false;
        }
    }
}

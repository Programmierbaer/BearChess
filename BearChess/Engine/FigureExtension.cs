using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Engine
{
    public static class FigureExtension
    {
        #region Pawn

        private static bool IsLost(this PawnFigure pawn, IChessBoard chessBoard)
        {
            if (pawn.IsAttackedByColor(pawn.EnemyColor) && chessBoard.CurrentColor == pawn.EnemyColor)
            {
                if (!pawn.IsAttackedByColor(pawn.Color))
                {
                    return true;
                }
            }

            return false;
        }

        private static int PositionValue(this PawnFigure pawn, IChessBoard chessBoard, GamePhase gamePhase)
        {
            int result = 0;
            int doubleCount = 0;
            int pawnsCount = 0;
            int attackingTheForeignColor = 0;

            int minMaterialWin = 0;
            int maxMaterialWin = 0;


            foreach (var chessFigure in chessBoard.GetFiguresOnLine(Fields.GetLine(pawn.Field)))
            {
                if (chessFigure.FigureId == pawn.FigureId)
                {
                    doubleCount++;
                }
            }

            foreach (var neighbourLine in Fields.NeighbourLines(Fields.GetLine(pawn.Field)))
            {
                foreach (var chessFigure in chessBoard.GetFiguresOnLine(neighbourLine))
                {
                    if (chessFigure.FigureId == pawn.FigureId)
                    {
                        pawnsCount++;
                    }
                }
            }

            attackingTheForeignColor = pawn.IsAttackingTheColor(pawn.EnemyColor);
            if (attackingTheForeignColor > 0)
            {
                minMaterialWin = pawn.GetLowestAttackedMaterialBy(pawn.EnemyColor);
                maxMaterialWin = pawn.GetHighestAttackedMaterialBy(pawn.EnemyColor);
            }


            if (chessBoard.CurrentColor == pawn.Color)
            {
                if (maxMaterialWin > 0)
                {
                    return maxMaterialWin;
                }

                if (doubleCount > 1)
                {
                    result -= 5;
                }

                if (pawnsCount == 0)
                {
                    result -= 5;
                }

                result += pawn.Color == Fields.COLOR_WHITE ? Definitions.WhitePawnPositionValueFields[pawn.Field] : Definitions.BlackPawnPositionValueFields[pawn.Field];
            }
            else
            {
                if (pawn.IsLost(chessBoard))
                {
                    return -pawn.Material;
                }
                if (minMaterialWin > 0)
                {
                    return minMaterialWin;
                }
                if (doubleCount > 1)
                {
                    result -= 5;
                }

                if (pawnsCount == 0)
                {
                    result -= 5;
                }

                result += pawn.Color == Fields.COLOR_WHITE ? Definitions.WhitePawnPositionValueFields[pawn.Field] : Definitions.BlackPawnPositionValueFields[pawn.Field];
            }
            return result;
        }

        #endregion

        #region Bishop

        private static bool IsLost(this BishopFigure bishop, IChessBoard chessBoard)
        {
            if (bishop.IsAttackedByColor(bishop.EnemyColor) && chessBoard.CurrentColor == bishop.EnemyColor)
            {
                if (!bishop.IsAttackedByColor(bishop.Color))
                {
                    return true;
                }
            }

            return false;
        }

        private static int PositionValue(this BishopFigure bishop, IChessBoard chessBoard, GamePhase gamePhase)
        {
            int result = 10;
            if (!chessBoard.Castled(bishop.Color))
            {
                if (bishop.Color == Fields.COLOR_WHITE && gamePhase == GamePhase.Opening &&
                    (bishop.Field == Fields.FF1 || bishop.Field == Fields.FC1))
                {
                    return -5;
                }

                if (bishop.Color == Fields.COLOR_BLACK && gamePhase == GamePhase.Opening &&
                    (bishop.Field == Fields.FF8 || bishop.Field == Fields.FC8))
                {
                    return -5;
                }
            }

            if (bishop.IsLost(chessBoard))
            {
                return -bishop.Material;
            }

            if (bishop.IsAttackingTheColor(bishop.EnemyColor) > 0)
            {

                return bishop.GetLowestAttackedMaterialBy(bishop.EnemyColor);
            }

            return result;
        }

        #endregion

        #region Knight

        private static bool IsLost(this KnightFigure knight, IChessBoard chessBoard)
        {
            if (knight.IsAttackedByColor(knight.EnemyColor) && chessBoard.CurrentColor == knight.EnemyColor)
            {
                if (!knight.IsAttackedByColor(knight.Color))
                {
                    return true;
                }
            }

            return false;
        }

        private static int PositionValue(this KnightFigure knight, IChessBoard chessBoard, GamePhase gamePhase)
        {
            int result = 10;
            if (Fields.InLine(Fields.Lines.A, knight.Field) || Fields.InLine(Fields.Lines.H, knight.Field))
            {
                result = -5;
            }

            if (!chessBoard.Castled(knight.Color))
            {
                if (knight.Color == Fields.COLOR_WHITE && gamePhase == GamePhase.Opening &&
                    (knight.Field == Fields.FG1 || knight.Field == Fields.FB1))
                {
                    result = -5;
                }

                if (knight.Color == Fields.COLOR_BLACK && gamePhase == GamePhase.Opening &&
                    (knight.Field == Fields.FG8 || knight.Field == Fields.FB8))
                {
                    result = -5;
                }
            }

            var attacking = knight.IsAttackingTheColor(knight.EnemyColor);
            if (attacking > 0)
            {
                if (!knight.IsAttackingTheFigure(FigureId.KNIGHT, knight.EnemyColor))
                {
                    if (attacking > 1)
                    {
                        result += 10;
                    }
                }
                else
                {
                    result += 5;
                }
            }

            if (knight.IsLost(chessBoard))
            {
                result -= knight.Material;
            }
            return result;
        }

        #endregion

        #region Rook

        private static bool IsLost(this RookFigure rook, IChessBoard chessBoard)
        {
            if (rook.IsAttackedByColor(rook.EnemyColor) && chessBoard.CurrentColor == rook.EnemyColor)
            {
                if (!rook.IsAttackedByColor(rook.Color))
                {
                    return true;
                }
            }

            return false;
        }

        private static int PositionValue(this RookFigure rook, IChessBoard chessBoard, GamePhase gamePhase)
        {
            var result = 10;
            var count = 0;
            if (rook.Color == Fields.COLOR_WHITE && gamePhase == GamePhase.Opening && rook.Field != Fields.FA1 &&
                rook.Field != Fields.FH1)
            {
                count = 0;
                count += chessBoard.GetFigureOn(Fields.FF1).FigureId == FigureId.WHITE_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FC1).FigureId == FigureId.WHITE_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FG1).FigureId == FigureId.WHITE_KNIGHT ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FB1).FigureId == FigureId.WHITE_KNIGHT ? 1 : 0;
                switch (count)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        result = -10;
                        break;
                    case 3:
                    case 4:
                        result = -20;
                        break;
                    default:
                        result = 0;
                        break;
                }
            }

            if (rook.Color == Fields.COLOR_BLACK && gamePhase == GamePhase.Opening && rook.Field != Fields.FA8 &&
                rook.Field != Fields.FH8)
            {
                count = 0;
                count += chessBoard.GetFigureOn(Fields.FF8).FigureId == FigureId.BLACK_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FC8).FigureId == FigureId.BLACK_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FG8).FigureId == FigureId.BLACK_KNIGHT ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FB8).FigureId == FigureId.BLACK_KNIGHT ? 1 : 0;
                switch (count)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        result = -10;
                        break;
                    case 3:
                    case 4:
                        result = -20;
                        break;
                    default:
                        result = 0;
                        break;
                }
            }

            if (rook.IsLost(chessBoard))
            {
                result = -rook.Material;
            }
            else
            {

                if (rook.IsAttackedByFigure(FigureId.PAWN, rook.EnemyColor))
                {
                    result -= rook.Material - Material.PAWN;
                }
                else if (rook.IsAttackedByFigure(FigureId.KNIGHT, rook.EnemyColor))
                {
                    result -= rook.Material - Material.KNIGHT;
                }
                else if (rook.IsAttackedByFigure(FigureId.BISHOP, rook.EnemyColor))
                {
                    result -= rook.Material - Material.BISHOP;
                }

            }

            return result;
        }

        #endregion

        #region Queen

        private static bool IsLost(this QueenFigure queen, IChessBoard chessBoard)
        {
            if (queen.IsAttackedByColor(queen.EnemyColor) && chessBoard.CurrentColor == queen.EnemyColor)
            {
                if (!queen.IsAttackedByColor(queen.Color))
                {
                    return true;
                }
            }

            return false;
        }

        private static int PositionValue(this QueenFigure queen, IChessBoard chessBoard, GamePhase gamePhase)
        {
            var result = 10;
            var count = 0;
            if (queen.Color == Fields.COLOR_WHITE && queen.Field != Fields.FD1)
            {
                count = 0;
                count += chessBoard.GetFigureOn(Fields.FF1).FigureId == FigureId.WHITE_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FC1).FigureId == FigureId.WHITE_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FG1).FigureId == FigureId.WHITE_KNIGHT ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FB1).FigureId == FigureId.WHITE_KNIGHT ? 1 : 0;
                switch (count)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        result = -10;
                        break;
                    case 3:
                    case 4:
                        result = -20;
                        break;
                    default:
                        result = 0;
                        break;
                }
            }

            if (queen.Color == Fields.COLOR_BLACK && queen.Field != Fields.FD8)
            {
                count = 0;
                count += chessBoard.GetFigureOn(Fields.FF8).FigureId == FigureId.BLACK_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FC8).FigureId == FigureId.BLACK_BISHOP ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FG8).FigureId == FigureId.BLACK_KNIGHT ? 1 : 0;
                count += chessBoard.GetFigureOn(Fields.FB8).FigureId == FigureId.BLACK_KNIGHT ? 1 : 0;
                switch (count)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        result = -10;
                        break;
                    case 3:
                    case 4:
                        result = -20;
                        break;
                    default:
                        result = 0;
                        break;
                }
            }

            if (queen.IsLost(chessBoard))
            {
                result = -queen.Material;
            }
            else
            {
                if (queen.IsAttackedByFigure(FigureId.PAWN, queen.EnemyColor))
                {
                    result -= queen.Material - Material.PAWN;
                }
                else if (queen.IsAttackedByFigure(FigureId.KNIGHT, queen.EnemyColor))
                {
                    result -= queen.Material - Material.KNIGHT;
                }
                else if (queen.IsAttackedByFigure(FigureId.BISHOP, queen.EnemyColor))
                {
                    result -= queen.Material - Material.BISHOP;
                }

                if (queen.IsAttackedByFigure(FigureId.ROOK, queen.EnemyColor))
                {
                    result -= queen.Material - Material.ROOK;
                }

            }

            return result;
        }

        #endregion

        #region King


        private static bool IsLost(this KingFigure king, IChessBoard chessBoard)
        {
            if (king.IsAttackedByColor(king.EnemyColor) && chessBoard.CurrentColor == king.EnemyColor)
            {

                return true;

            }

            return false;
        }

        private static int PositionValue(this KingFigure king, IChessBoard chessBoard, GamePhase gamePhase)
        {
            var result = 0;
            if (king.IsLost(chessBoard))
            {
                result = -king.Material;
            }
            else
            {
                if (gamePhase == GamePhase.Opening)
                {
                    if (chessBoard.Castled(king.Color))
                    {
                        result = 10;
                    }
                    else
                    {
                        if (chessBoard.CanCastling(king.Color, CastlingEnum.Short) ||
                            chessBoard.CanCastling(king.Color, CastlingEnum.Long))
                        {
                            result = 0;

                        }
                        else
                        {
                            result = -10;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        public static int PositionValue(this IChessFigure abstractChessFigure, IChessBoard chessBoard, GamePhase gamePhase)
        {
            if (((AbstractChessFigure)abstractChessFigure).CurrentMoveList == null)
            {
                return -20;
            }

            if (abstractChessFigure is PawnFigure pawnFigure)
            {
                return (int)pawnFigure.PositionValue(chessBoard, gamePhase);
            }

            if (abstractChessFigure is BishopFigure bishopFigure)
            {
                return (int)bishopFigure.PositionValue(chessBoard, gamePhase);
            }

            if (abstractChessFigure is KnightFigure knightFigure)
            {
                return (int)knightFigure.PositionValue(chessBoard, gamePhase);
            }

            if (abstractChessFigure is RookFigure rookFigure)
            {
                return (int)rookFigure.PositionValue(chessBoard, gamePhase);
            }

            if (abstractChessFigure is QueenFigure queenFigure)
            {
                return queenFigure.PositionValue(chessBoard, gamePhase);
            }
            var kingFigure = abstractChessFigure as KingFigure;
            return kingFigure?.PositionValue(chessBoard, gamePhase) ?? 0;
        }

    }
}

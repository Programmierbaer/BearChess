using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class KingFigure : AbstractChessFigure
    {
        protected KingFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.KING)
        {
            GeneralFigureId = Definitions.FigureId.KING;
            FromOffset = 0;
            ToOffset = 7;
        }

        public override List<IMove> GetMoveList()
        {
            var moves = new List<IMove>();
            int tmpField;
            IChessFigure tmpFigure;
            for (var offSet = FromOffset; offSet <= ToOffset; offSet++)
            {
                tmpField = Field + Move.MoveOffsets[offSet]; // Nächste Feldnummer
                tmpFigure = ChessBoard.GetFigureOn(tmpField); // Figur auf diesem Feld
                // Ist das Feld nicht von eigener Farbe und nicht ausserhalb des Brettes...
                if (tmpFigure.Color != Color && tmpFigure.FigureId != Definitions.FigureId.OUTSIDE_PIECE)
                {
                    tmpFigure.IncAttack(this);
                    IncIsAttacking(tmpFigure);
                    // und wird nicht angegriffen...
                    if (!tmpFigure.IsAttackedByColor(EnemyColor))
                    {
                        // Gültiger Zug
                        IMove tmpMove = new Move(Field, tmpField, Color,FigureId, tmpFigure);
                        moves.Add(tmpMove);
                    }
                }
                else // Eigene Figur oder ausserhalb...
                {
                    if (tmpFigure.Color == Color)
                    {
                        tmpFigure.IncAttack(this);
                        IncIsAttacking(tmpFigure);
                    }
                }
            }
            // Farbe darf noch kurz rochieren?
            if (!IsAttackedByColor(EnemyColor) && ChessBoard.CanCastling(Color, CastlingEnum.Short))
            {
                tmpField = Field + 1; // Nächstes F-Feld
                tmpFigure = ChessBoard.GetFigureOn(tmpField); // Figur auf diesem Feld
                if (tmpFigure.Color == Fields.COLOR_EMPTY && !tmpFigure.IsAttackedByColor(EnemyColor))
                {
                    tmpField = Field + 2; // Nächstes G-Feld
                    tmpFigure = ChessBoard.GetFigureOn(tmpField); // Figur auf diesem Feld
                    if (tmpFigure.Color == Fields.COLOR_EMPTY && !tmpFigure.IsAttackedByColor(EnemyColor))
                    {
                        // Gültiger Zug
                        IMove tmpMove = new Move(Field, tmpField,Color, FigureId);
                        moves.Add(tmpMove);
                    }
                }
            }
            // Farbe darf noch lang rochieren?
            if (!IsAttackedByColor(EnemyColor) && ChessBoard.CanCastling(Color, CastlingEnum.Long))
            {
                tmpField = Field - 1; // Nächstes D-Feld
                tmpFigure = ChessBoard.GetFigureOn(tmpField); // Figur auf diesem Feld
                if (tmpFigure.Color == Fields.COLOR_EMPTY && !tmpFigure.IsAttackedByColor(EnemyColor))
                {
                    tmpField = Field - 2; // Nächstes C-Feld
                    tmpFigure = ChessBoard.GetFigureOn(tmpField); // Figur auf diesem Feld
                    if (tmpFigure.Color == Fields.COLOR_EMPTY && !tmpFigure.IsAttackedByColor(EnemyColor))
                    {
                        tmpField = Field - 3; // Nächstes B-Feld
                        tmpFigure = ChessBoard.GetFigureOn(tmpField); // Figur auf diesem Feld
                        if (tmpFigure.Color == Fields.COLOR_EMPTY)
                        {
                            // Gültiger Zug
                            IMove tmpMove = new Move(Field, Field - 2, Color, FigureId);
                            moves.Add(tmpMove);
                        }
                    }
                }
            }

            CurrentMoveList = moves;
            return moves;
        }
    }

    public class WhiteKingFigure : KingFigure
    {
        public WhiteKingFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.WHITE_KING;
            Color = Fields.COLOR_WHITE;
            EnemyColor = Fields.COLOR_BLACK;
        }
    }

    public class BlackKingFigure : KingFigure
    {
        public BlackKingFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.BLACK_KING;
            Color = Fields.COLOR_BLACK;
            EnemyColor = Fields.COLOR_WHITE;
        }
    }
}

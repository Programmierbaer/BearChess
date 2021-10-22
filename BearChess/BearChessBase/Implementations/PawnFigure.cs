using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class PawnFigure : AbstractChessFigure
    {
        protected int Step1;
        protected int Step2;
        protected int Step3;

        protected PawnFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.PAWN)
        {
            GeneralFigureId = Definitions.FigureId.PAWN;
        }

        public override List<Move> GetMoveList()
        {
            var moves = new List<Move>(10);
            // Nächstes Feld in Zugrichtung gerade aus
            var tmpFigure = ChessBoard.GetFigureOn(Field + Step1);
            // Nur wenn das Feld leer ist...
            if (tmpFigure.Color == Fields.COLOR_EMPTY)
            {
                var tmpMove = new Move(Field, Field + Step1, Color, FigureId);
                moves.Add(tmpMove);
                if (Color == Fields.COLOR_WHITE && Field < Fields.FA3 ||
                    Color == Fields.COLOR_BLACK && Field >= Fields.FA7
                    )
                // Doppelschritt erlaubt?
                {
                    tmpFigure = ChessBoard.GetFigureOn(Field + Step1 + Step1);
                    // Ist das übernächste Feld auch leer?
                    if (tmpFigure.Color == Fields.COLOR_EMPTY)
                    {
                        // Dann ist es auch ein neuer Zug
                        tmpMove = new Move(Field, Field + Step1 + Step1, Color, FigureId);
                        moves.Add(tmpMove);
                    }
                }
            }
            // Feld in 1. diagonaler Richtung
            tmpFigure = ChessBoard.GetFigureOn(Field + Step2);
            // Steht dort eine generische Figur?
            if (tmpFigure.Color == EnemyColor || tmpFigure.Field == ChessBoard.EnPassantTargetFieldNumber)
            {
                tmpFigure.IncAttack(this);
                IncIsAttacking(tmpFigure);
                var tmpMove = new Move(Field, Field + Step2, Color, FigureId, tmpFigure.Color == EnemyColor
                                                                 ? tmpFigure
                                                                 : ChessBoard.GetFigureOn(ChessBoard.EnPassantTargetFieldNumber - Step1));

                moves.Add(tmpMove);
            }
            else if (tmpFigure.Color != Fields.COLOR_OUTSIDE)
            {
                tmpFigure.IncAttack(this);
                IncIsAttacking(tmpFigure);
            }
            // Feld in 2. diagonaler Richtung
            tmpFigure = ChessBoard.GetFigureOn(Field + Step3);
            // Steht dort eine generische Figur?
            if (tmpFigure.Color == EnemyColor || tmpFigure.Field == ChessBoard.EnPassantTargetFieldNumber)
            {
                tmpFigure.IncAttack(this);
                IncIsAttacking(tmpFigure);
                // Erlaubter Schlagzug
                var tmpMove = new Move(Field, Field + Step3, Color,FigureId, tmpFigure.Color == EnemyColor
                                                                 ? tmpFigure
                                                                 : ChessBoard.GetFigureOn(ChessBoard.EnPassantTargetFieldNumber - Step1));
                moves.Add(tmpMove);
            }
            else if (tmpFigure.Color != Fields.COLOR_OUTSIDE)
            {
                tmpFigure.IncAttack(this);
                IncIsAttacking(tmpFigure);
            }
            CurrentMoveList = moves;
            return moves;
        }
    }

    public class WhitePawnFigure : PawnFigure
    {
        public WhitePawnFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.WHITE_PAWN;
            Color = Fields.COLOR_WHITE;
            EnemyColor = Fields.COLOR_BLACK;
            Step1 = 10;
            Step2 = 9;
            Step3 = 11;
        }
    }

    public class BlackPawnFigure : PawnFigure
    {
        public BlackPawnFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.BLACK_PAWN;
            Color = Fields.COLOR_BLACK;
            EnemyColor = Fields.COLOR_WHITE;
            Step1 = -10;
            Step2 = -9;
            Step3 = -11;
        }
    }
}

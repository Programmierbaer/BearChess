using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public abstract class AbstractChessFigure : IChessFigure
    {

        private readonly int[] _attackedColor = { 0, 0 };
        private readonly int[] _highestAttackedFigureByColor = { 0, 0 };
        private readonly int[] _lowestAttackedFigureByColor = { 0, 0 };
        private readonly int[] _attackedByFigure = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int[] _attackingByFigure = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int[] _attackingColor = { 0, 0 };
        private readonly List<int> _attackingFigureIds = new List<int>();

        protected int FromOffset;
        protected int ToOffset;


        /// <summary>
        /// Chessboard
        /// </summary>
        protected IChessBoard ChessBoard { get; }

        /// <inheritdoc />
        public int FigureId { get; protected set; }

        /// <inheritdoc />
        public int GeneralFigureId { get; protected set; }

        /// <inheritdoc />
        public int Field { get; set; }

        /// <inheritdoc />
        public int Color { get; set; }

        /// <inheritdoc />
        public int Material { get; }

        /// <inheritdoc />
        public int EnemyColor { get; set; }

        /// <inheritdoc />
        public string FenFigureCharacter => Definitions.FigureId.FigureIdToFenCharacter[FigureId];

        /// <inheritdoc />
        public List<IMove> CurrentMoveList { get; protected set; }



        protected AbstractChessFigure(IChessBoard board, int field, int material)
        {
            ChessBoard = board;
            Field = field;
            Material = material;
        }

        /// <inheritdoc />
        public virtual List<IMove> GetMoveList()
        {
            var moves = new List<IMove>();
            for (var offSet = FromOffset; offSet <= ToOffset; offSet++)
            {
                var tmpField = Field;
                var tmpOffSet = Move.MoveOffsets[offSet];
                for (var i = 0; i < 8; i++) // Bewege die Figur maximal 7 Felder in eine Zugrichtung...
                {
                    tmpField += tmpOffSet; // Nächstes Zielfeld (Feldnummer)
                    var tmpFigure = ChessBoard.GetFigureOn(tmpField); // Figur auf dem Zielfeld
                    // Ist das Feld nicht von eigener Farbe und nicht ausserhalb des Brettes...
                    if ((tmpFigure.Color != Color) && (tmpFigure.FigureId != Definitions.FigureId.OUTSIDE_PIECE))
                    {
                        tmpFigure.IncAttack(this);
                        IncIsAttacking(tmpFigure);
                        IMove newMove;
                        // Gültiger Zug. Entweder freies Feld oder gegnerische Figur
                        if (tmpFigure.Color == EnemyColor)
                        {
                            // Gegnerische Figur geschlagen? Dann Zugrichtung beenden
                            newMove = new Move(Field, tmpField, Color, FigureId, tmpFigure);
                            moves.Add(newMove);
                            break;
                        }

                        newMove = new Move(Field, tmpField, Color, FigureId);

                        moves.Add(newMove);
                    }
                    else // Eigene Figur oder ausserhalb...
                    {
                        if (tmpFigure.Color == Color)
                        {
                            tmpFigure.IncAttack(this);
                            IncIsAttacking(tmpFigure);
                        }
                        break; // Abbrechen
                    }
                    // Springer können pro Zugrichtung nur einen Zug machen
                    if (GeneralFigureId == Definitions.FigureId.KNIGHT)
                        break; // Abbrechen
                }
            }
            CurrentMoveList = moves;
            return moves;
        }


        /// <inheritdoc />
        public bool IsAttackedByColor(int figureColor)
        {
            return _attackedColor[figureColor] > 0;
        }


        /// <inheritdoc />
        public bool IsAttacked()
        {
            return _attackedColor[Fields.COLOR_WHITE] > 0 || _attackedColor[Fields.COLOR_BLACK] > 0;
        }

        /// <inheritdoc />
        public int[] GetAttackedFigureIds()
        {
            return _attackingFigureIds.ToArray();
        }

        /// <inheritdoc />
        public bool IsAttackedByFigure(int figureId)
        {
            return _attackedByFigure[figureId] > 0;
        }

        /// <inheritdoc />
        public bool IsAttackedByFigure(int generalFigureId, int figureColor)
        {
            int diff = figureColor == Fields.COLOR_WHITE ? 0 : 6;
            return _attackedByFigure[generalFigureId + diff] > 0;
        }

        /// <inheritdoc />
        public int GetHighestAttackedMaterialBy(int figureColor)
        {
            return _highestAttackedFigureByColor[figureColor];
        }

        /// <inheritdoc />
        public int GetLowestAttackedMaterialBy(int figureColor)
        {
            return _lowestAttackedFigureByColor[figureColor];
        }

        /// <inheritdoc />
        public void IncAttack(IChessFigure figure)
        {
            _attackedByFigure[figure.FigureId] = _attackedByFigure[figure.FigureId] + 1;
            if (_attackingByFigure[figure.FigureId] == 1)
            {
                _attackingFigureIds.Add(figure.FigureId);
            }

            _attackedColor[figure.Color] = _attackedColor[figure.Color] + 1;

            if (_highestAttackedFigureByColor[figure.Color] < figure.Material)
            {
                _highestAttackedFigureByColor[figure.Color] = figure.Material;
            }

            if (_lowestAttackedFigureByColor[figure.Color] > figure.Material || _lowestAttackedFigureByColor[figure.Color] == 0)
            {
                _lowestAttackedFigureByColor[figure.Color] = figure.Material;
            }

        }

        /// <inheritdoc />
        public void IncIsAttacking(IChessFigure figure)
        {
            if (figure.FigureId == Definitions.FigureId.NO_PIECE)
            {
                return;
            }
            _attackingByFigure[figure.FigureId] = _attackingByFigure[figure.FigureId] + 1;
            _attackingColor[figure.Color] = _attackingColor[figure.Color] + 1;
        }

        /// <inheritdoc />
        public int IsAttackingTheColor( int figureColor)
        {
            return _attackingColor[figureColor];
        }


        /// <inheritdoc />
        public bool IsAttackingTheFigure(int baseFigureId, int figureColor)
        {
            int diff = figureColor == Fields.COLOR_WHITE ? 0 : 6;
            return _attackingByFigure[baseFigureId + diff] > 0;
        }


        /// <inheritdoc />
        public void ClearAttacks()
        {
            Array.Clear(_attackedByFigure, 0, _attackedByFigure.Length);
            _attackedColor[Fields.COLOR_BLACK] = 0;
            _attackedColor[Fields.COLOR_WHITE] = 0;
            _attackingColor[Fields.COLOR_WHITE] = 0;
            _attackingColor[Fields.COLOR_BLACK] = 0;
            _highestAttackedFigureByColor[Fields.COLOR_BLACK] = 0;
            _highestAttackedFigureByColor[Fields.COLOR_WHITE] = 0;
        }

        /// <inheritdoc />
        public string FigureCharacter
        {
            get
            {
                if (string.IsNullOrEmpty(FenFigureCharacter)) return @" ";
                return FenFigureCharacter;
            }
        }

        public int ResolveAttackingList()
        {
            int[] attackedByFigure = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Array.Copy(_attackedByFigure, attackedByFigure, attackedByFigure.Length);
            int myValue = Material;
            int result = 0;
            int diff = Color == Fields.COLOR_WHITE ? 6 : 0;
            bool cont = true;
            bool add = false;
            while (cont)
            {
                cont = false;
                for (int i = 1; i < 7; i++)
                {
                    if (attackedByFigure[i + diff] > 0)
                    {
                        if (add)
                        {
                            result += myValue;
                            myValue = GetMaterialFor(i + diff);
                        }
                        else
                        {
                            result -= myValue;
                            myValue = GetMaterialFor(i + diff);
                        }
                        add = !add;
                        attackedByFigure[i + diff] = attackedByFigure[i + diff] - 1;
                        cont = true;
                        break;
                    }
                }
                diff = diff == 6 ? 0 : 6;
            }
            return result;
        }

        private static int GetMaterialFor(int figureId)
        {
            switch (figureId)
            {
                case 0:
                    return Definitions.Material.NO_FIGURE;
                case 1:
                    return Definitions.Material.PAWN;
                case 2:
                case 3:
                    return Definitions.Material.BISHOP;
                case 4:
                    return Definitions.Material.ROOK;
                case 5:
                    return Definitions.Material.QUEEN;
                case 6:
                    return Definitions.Material.KING;
                case 7:
                    return Definitions.Material.PAWN;
                case 8:
                case 9:
                    return Definitions.Material.BISHOP;
                case 10:
                    return Definitions.Material.ROOK;
                case 11:
                    return Definitions.Material.QUEEN;
                case 12:
                    return Definitions.Material.KING;
                default:
                    return Definitions.Material.NO_FIGURE;
            }
        }

    }
}

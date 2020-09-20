using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class NoFigure : AbstractChessFigure
    {
        /// <summary>
        /// Represents an empty field on a chessboard.
        /// </summary>
        public NoFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.NO_FIGURE)
        {
            FigureId = Definitions.FigureId.NO_PIECE;
            Color = Fields.COLOR_EMPTY;
            EnemyColor = Fields.COLOR_EMPTY;
            GeneralFigureId = Definitions.FigureId.NO_PIECE;
        }

        public override List<IMove> GetMoveList()
        {
            return new List<IMove>(0);
        }
    }
}

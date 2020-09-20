using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class OutsideFigure : AbstractChessFigure
    {
        /// <summary>
        /// Represents a field outside of the regular chessboard.
        /// </summary>
        public OutsideFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.NO_FIGURE)
        {
            GeneralFigureId = Definitions.FigureId.OUTSIDE_PIECE;
            FigureId = Definitions.FigureId.OUTSIDE_PIECE;
            Color = Fields.COLOR_OUTSIDE;
            EnemyColor = Fields.COLOR_OUTSIDE;
        }

        public override List<IMove> GetMoveList()
        {
            return new List<IMove>(0);
        }
    }
}

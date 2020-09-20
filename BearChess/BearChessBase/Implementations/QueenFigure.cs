using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class QueenFigure : AbstractChessFigure
    {
        protected QueenFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.QUEEN)
        {
            GeneralFigureId = Definitions.FigureId.QUEEN;
            FromOffset = 0;
            ToOffset = 7;
        }
    }

    public class WhiteQueenFigure : QueenFigure
    {
        public WhiteQueenFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.WHITE_QUEEN;
            Color = Fields.COLOR_WHITE;
            EnemyColor = Fields.COLOR_BLACK;
        }
    }

    public class BlackQueenFigure : QueenFigure
    {
        public BlackQueenFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.BLACK_QUEEN;
            Color = Fields.COLOR_BLACK;
            EnemyColor = Fields.COLOR_WHITE;
        }
    }
}

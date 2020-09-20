using System;
using System.Collections.Generic;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class KnightFigure : AbstractChessFigure
    {
        protected KnightFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.KNIGHT)
        {
            GeneralFigureId = Definitions.FigureId.KNIGHT;
            FromOffset = 8;
            ToOffset = 15;
        }
    }

    public class WhiteKnightFigure : KnightFigure
    {
        public WhiteKnightFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.WHITE_KNIGHT;
            Color = Fields.COLOR_WHITE;
            EnemyColor = Fields.COLOR_BLACK;
        }
    }

    public class BlackKnightFigure : KnightFigure
    {
        public BlackKnightFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.BLACK_KNIGHT;
            Color = Fields.COLOR_BLACK;
            EnemyColor = Fields.COLOR_WHITE;
        }
    }
}

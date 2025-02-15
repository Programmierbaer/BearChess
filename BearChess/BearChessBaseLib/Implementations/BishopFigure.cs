using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class BishopFigure : AbstractChessFigure
    {
        protected BishopFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.BISHOP)
        {
            GeneralFigureId = Definitions.FigureId.BISHOP;
            FromOffset = 0;
            ToOffset = 3;
        }
    }

    public class WhiteBishopFigure : BishopFigure
    {
        public WhiteBishopFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.WHITE_BISHOP;
            Color = Fields.COLOR_WHITE;
            EnemyColor = Fields.COLOR_BLACK;
        }
    }

    public class BlackBishopFigure : BishopFigure
    {
        public BlackBishopFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.BLACK_BISHOP;
            Color = Fields.COLOR_BLACK;
            EnemyColor = Fields.COLOR_WHITE;
        }
    }
}

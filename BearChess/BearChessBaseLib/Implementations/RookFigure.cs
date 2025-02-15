using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class RookFigure : AbstractChessFigure
    {
        protected RookFigure(IChessBoard board, int field) : base(board: board, field: field, material: Definitions.Material.ROOK)
        {
            GeneralFigureId = Definitions.FigureId.ROOK;
            FromOffset = 4;
            ToOffset = 7;
        }
    }

    public class WhiteRookFigure : RookFigure
    {
        public WhiteRookFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.WHITE_ROOK;
            Color = Fields.COLOR_WHITE;
            EnemyColor = Fields.COLOR_BLACK;
        }
    }

    public class BlackRookFigure : RookFigure
    {
        public BlackRookFigure(IChessBoard board, int field) : base(board: board, field: field)
        {
            FigureId = Definitions.FigureId.BLACK_ROOK;
            Color = Fields.COLOR_BLACK;
            EnemyColor = Fields.COLOR_WHITE;
        }
    }
}

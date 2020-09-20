using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class Move : IMove
    {
        public static readonly int[] MoveOffsets = { -9, -11, 9, 11, -10, 10, 1, -1, 19, 21, 12, -8, -19, -21, -12, 8 };

        public int Figure { get; }
        public int FigureColor { get; }

        /// <inheritdoc />
        public int FromField { get; }

        /// <inheritdoc />
        public string FromFieldName { get; }

        /// <inheritdoc />
        public int ToField { get; }

        /// <inheritdoc />
        public string ToFieldName { get; }

        /// <inheritdoc />
        public int CapturedFigure { get; }


        /// <inheritdoc />
        public int PromotedFigure { get; }

        /// <inheritdoc />
        public int Value { get; set; }

        /// <inheritdoc />
        public int CapturedFigureMaterial { get; }

        /// <inheritdoc />
        public int Identifier { get; }

        public Move(int fromField, int toField, int color, int figureId)
        {
            Figure = figureId;
            FigureColor = color;
            FromField = fromField;
            ToField = toField;
            Value = int.MinValue;
            CapturedFigure = FigureId.NO_PIECE;
            PromotedFigure = FigureId.NO_PIECE;
            Identifier = fromField * 100 + toField;
            FromFieldName = Fields.GetFieldName(FromField);
            ToFieldName = Fields.GetFieldName(ToField);
        }

        public Move(int fromField, int toField,  int color, int figureId, IChessFigure capturedFigure) : this(fromField,toField, color, figureId)
        {
            CapturedFigure = capturedFigure.FigureId;
            CapturedFigureMaterial = capturedFigure.Material;
        }

        public Move(int fromField, int toField,  int color, int figureId, IChessFigure capturedFigure, int promotedFigure) : this(fromField, toField, color, figureId)
        {
            CapturedFigure = capturedFigure.FigureId;
            CapturedFigureMaterial = capturedFigure.Material;
            PromotedFigure = promotedFigure;
        }

        public Move(int fromField, int toField,  int color, int figureId, int promotedFigure) : this(fromField, toField, color, figureId)
        {
            PromotedFigure = promotedFigure;
        }

        public Move(IMove move)
        {
            FigureColor = move.FigureColor;
            FromField = move.FromField;
            ToField = move.ToField;
            Value = move.Value;
            CapturedFigure = move.CapturedFigure;
            CapturedFigureMaterial = move.CapturedFigureMaterial;
            PromotedFigure = move.PromotedFigure;
            Identifier = move.Identifier;
            FromFieldName = move.FromFieldName;
            ToFieldName = move.ToFieldName;
        }

        public override string ToString()
        {
            return $"{FromFieldName}-{ToFieldName} ({Value})";
        }
    }

    public static class MoveExtentions
    {
        public static bool EqualMove(this IMove move, IMove move2)
        {
            return move.Identifier.Equals(move2.Identifier);
        }
    }
}

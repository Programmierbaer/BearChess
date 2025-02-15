using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface IMove
    {


        int Figure { get; }

        /// <summary>
        /// Color
        /// </summary>
        int FigureColor { get; }

        /// <summary>
        ///     Source field <see cref="Fields" />
        /// </summary>
        int FromField { get; }

        /// <summary>
        /// Source field name
        /// </summary>
        string FromFieldName { get; }

        /// <summary>
        ///     Target field <see cref="Fields" />
        /// </summary>
        int ToField { get; }

        /// <summary>
        /// Target field name
        /// </summary>
        string ToFieldName { get; }

        /// <summary>
        ///     Captured figure
        /// </summary>
        int CapturedFigure { get; }

        /// <summary>
        /// Promoted figure
        /// </summary>
        int PromotedFigure { get; }

        /// <summary>
        ///     Calculated value of this move
        /// </summary>
        int Value { get; set; }

        /// <summary>
        ///     Material of the captured figure
        /// </summary>
        int CapturedFigureMaterial { get; }

        /// <summary>
        ///     Identifier of the move. Two moves with the same <see cref="FromField"/> and <see cref="ToField"/> has the same value.
        /// </summary>
        int Identifier { get; }
    }
}
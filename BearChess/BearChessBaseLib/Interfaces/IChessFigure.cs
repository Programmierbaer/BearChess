using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface IChessFigure
    {
        /// <summary>
        /// <see cref="Definitions.FigureId"/> e.g. <see cref="Definitions.FigureId.BLACK_KING"/>.
        /// </summary>
        int FigureId { get; }

        /// <summary>
        /// <see cref="Definitions.FigureId"/> e.g. <see cref="Definitions.FigureId.KING"/>.
        /// </summary>
        int GeneralFigureId { get;  }

        /// <summary>
        /// <see cref="Fields"/> e.g. <see cref="Fields.FA1"/>.
        /// </summary>
        int Field { get; set; }

        /// <summary>
        /// <see cref="Fields"/> e.g. <see cref="Fields.COLOR_WHITE"/>.
        /// </summary>
        int Color { get; set; }

        /// <summary>
        /// <see cref="Fields"/> e.g. <see cref="Fields.COLOR_BLACK"/>.
        /// </summary>
        int EnemyColor { get; }

        /// <summary>
        /// Static material value for this figure
        /// </summary>
        int Material { get; }

        /// <summary>
        /// Figure character string for FEN representation
        /// </summary>
        string FenFigureCharacter { get; }

        /// <summary>
        /// Figure character string for board representation
        /// </summary>
        string FigureCharacter { get; }

        /// <summary>
        /// Returns the current move list
        /// </summary>        
        List<Move> GetMoveList();

        /// <summary>
        /// Returns the current move list
        /// </summary>        
        List<Move> CurrentMoveList { get; }

        /// <summary>
        /// Returns true if this figure is attacked by color <paramref name="figureColor"/> (<see cref="Fields.COLOR_WHITE"/> or <see cref="Fields.COLOR_BLACK"/>).
        /// </summary>
        bool IsAttackedByColor(int figureColor);

        /// <summary>
        /// Add an attack by <paramref name="figure"/>.
        /// </summary>
        void IncAttack(IChessFigure figure);

        void IncIsAttacking(IChessFigure figure);

        int IsAttackingTheColor(int figureColor);

        bool IsAttackingTheFigure(int baseFigureId, int figureColor);

        /// <summary>
        /// Returns true if this figure is attacked by figure <paramref name="figureId"/>
        /// </summary>
        bool IsAttackedByFigure(int figureId);

        bool IsMultipleAttackedByFigure(int figureId);

        /// <summary>
        /// Returns true if this figure is attacked
        /// </summary>
        bool IsAttacked();

        /// <summary>
        /// Returns the ids of the attacked figures
        /// </summary>
        int[] GetAttackedFigureIds();

        /// <summary>
        /// Returns true if this figure is attacked by figure <paramref name="generalFigureId"/> (<see cref="Definitions.FigureId.KING"/>) and color <paramref name="figureColor"/> (<see cref="Fields.COLOR_WHITE"/> or <see cref="Fields.COLOR_BLACK"/>).
        /// </summary>
        bool IsAttackedByFigure(int generalFigureId, int figureColor);

        /// <summary>
        /// Returns the highest material value of the attacking color <paramref name="figureColor"/>
        /// </summary>       
        int GetHighestAttackedMaterialBy(int figureColor);

        /// <summary>
        /// Returns the lowest material value of the attacking color <paramref name="figureColor"/>
        /// </summary>
        int GetLowestAttackedMaterialBy(int figureColor);

        /// <summary>
        /// Remove all attacks
        /// </summary>
        void ClearAttacks();
    }
}

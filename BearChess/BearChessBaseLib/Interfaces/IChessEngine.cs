using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface IChessEngine
    {
        /// <summary>
        /// Returns the identification of the engine.
        /// </summary>
        string Identification { get; }

        /// <summary>
        /// Returns the author of the engine.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Returns a logo of the engine.
        /// </summary>
        byte[] Logo { get; }

        /// <summary>
        /// Activate or deactivate logging.
        /// </summary>
        bool Logging { get; set; }

        /// <summary>
        /// Event if the calculation finished.
        /// </summary>
        event EventHandler<EngineFinishedCalculationEventArgs> EngineFinishedCalculationEvent;

        /// <summary>
        /// Event for common information.
        /// </summary>
        event EventHandler<EngineInformationEventArgs> EngineInformationEvent;

        /// <summary>
        /// Initialize the engine for a new game. Reset to base position.
        /// </summary>
        void NewGame();

      
        /// <summary>
        /// Make a move <paramref name="fromField"/> to <paramref name="toField"/>. In case of castling only
        /// the king move is given.
        /// </summary>        
        void MakeMove(string fromField, string toField);

        /// <summary>
        /// Make a pawn move <paramref name="fromField"/> to <paramref name="toField"/> and the promotion figure <see cref="promotionFigureId"/>.
        /// </summary>       
        void MakeMove(string fromField, string toField, int promotionFigureId);

        /// <summary>
        /// Set the new current position by <paramref name="fen"/>.
        /// </summary>
        void SetPosition(string fen);

        /// <summary>
        /// Starts the calculation.
        /// </summary>
        Task<Move> Evaluate();

        /// <summary>
        /// Starts the calculation to a maximal tree depth <paramref name="maxDeep"/>.
        /// </summary>
        Task<Move> Evaluate(int maxDeep);

        /// <summary>
        /// Stop the calculation.
        /// </summary>
        void Stop();

        /// <summary>
        /// Returns the current best move.
        /// </summary>
        Move GetBestMove();

        /// <summary>
        /// Returns the current best moves line.
        /// </summary>
        List<Move> GetBestMovesLine();

        /// <summary>
        /// Returns the current move list.
        /// </summary>
        List<Move> GetMoves();

        /// <summary>
        /// Set the logger implementation.
        /// </summary>        
        void SetLogger(ILogger logger);

        /// <summary>
        /// Returns the logger implementation.
        /// </summary>
        ILogger GetLogger();

        string GetBoardEvaluation(int color);
    }
}

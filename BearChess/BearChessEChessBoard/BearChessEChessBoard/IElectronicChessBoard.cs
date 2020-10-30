using System;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface IElectronicChessBoard
    {
        /// <summary>
        /// If demo mode is true, all position changes are possible.
        /// </summary>
        void SetDemoMode(bool inDemoMode);

        /// <summary>
        /// Indicates if in demo mode
        /// </summary>
        bool IsInDemoMode { get; }

        /// <summary>
        /// Indicates if there a connection to the chess board.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Shows the LED for the last move in <paramref name="allMoves"/>, limited by spaces.
        /// </summary>
        void ShowMove(string allMoves);

        /// <summary>
        /// Show fields <paramref name="fromField"/> and <paramref name="toField"/> on board.
        /// </summary>
        void ShowMove(string fromField, string toField);

        /// <summary>
        /// Show all fields <paramref name="fields"/> on board.
        /// </summary>
        void SetLedsFor(string[] fields);

        /// <summary>
        /// All led off
        /// </summary>
        void SetAllLedsOff();

        /// <summary>
        /// All led on
        /// </summary>
        void SetAllLedsOn();

        /// <summary>
        /// Returns the current position as fen (only pieces)
        /// </summary>
        DataFromBoard GetPiecesFen();

        /// <summary>
        /// Returns the current position as fen 
        /// </summary>
        string GetFen();

     
        /// <summary>
        /// Indicates to start a new game
        /// </summary>
        void NewGame();

        /// <summary>
        /// Set a new position from fen and all following moves
        /// </summary>
        void SetFen(string fen, string allMoves);

        /// <summary>
        /// Close the connection to the chess board
        /// </summary>
        void Close();

        /// <summary>
        /// Stop reading from board
        /// </summary>
        void Stop();

        /// <summary>
        /// Calibrate board
        /// </summary>
        void Calibrate();

        /// <summary>
        /// Play with white or black pieces from the base line.
        /// </summary>
        void PlayWithWhite(bool withWhite);

        /// <summary>
        /// Move made on board by user
        /// </summary>
        event EventHandler<string> MoveEvent;

        /// <summary>
        /// New FEN position on board by user
        /// </summary>
        event EventHandler<string> FenEvent;

        event EventHandler AwaitedPosition;

        /// <summary>
        /// Return a best move
        /// </summary>
        string GetBestMove();

        /// <summary>
        /// Set COM-Port to <paramref name="portName"/>.
        /// </summary>
        void SetComPort(string portName);

        /// <summary>
        /// Set COM-Port to <paramref name="portName"/>.
        /// </summary>
        bool CheckComPort(string portName);

        /// <summary>
        /// Dim all LEDs
        /// </summary>
        void DimLeds(bool dimLeds);

        /// <summary>
        /// Synchronized flash all fields
        /// </summary>
        void FlashInSync(bool flashSync);

        /// <summary>
        /// Returns the configuration.
        /// </summary>
        EChessBoardConfiguration GetEChessBoardConfiguration();

        /// <summary>
        /// Save the configuration <paramref name="configuration"/>.
        /// </summary>
        void SaveEChessBoardConfiguration(EChessBoardConfiguration configuration);

        void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);

    }
}

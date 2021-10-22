using System;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface IEBoardWrapper
    {
        /// <summary>
        /// If demo mode is true, all position changes are possible.
        /// </summary>
        void SetDemoMode(bool inDemoMode);

        /// <summary>
        /// If replay mode is true, only valid moves (force and back) are allowed
        /// </summary>
        void SetReplayMode(bool inReplayMode);

        /// <summary>
        /// Indicates if in demo mode
        /// </summary>
        bool IsInDemoMode { get; }

        /// <summary>
        /// Indicates if in replay mode
        /// </summary>
        bool IsInReplayMode { get; }

        /// <summary>
        /// Indicates if there a connection to the chess board
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Shows the LED for the last move in <paramref name="allMoves"/>.
        /// </summary>
        void ShowMove(string allMoves, bool waitFor);

        void ShowMove(string fromField, string toField);

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
        /// Returns the current (awaited) position as fen 
        /// </summary>
        string GetFen();

        /// <summary>
        /// Returns the current position on board as fen
        /// </summary>
        string GetBoardFen();

        /// <summary>
        /// Indicates to start a new game
        /// </summary>
        void NewGame();

        /// <summary>
        /// Set a new position from fen 
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
        /// Continue reading from board
        /// </summary>
        void Continue();

        /// <summary>
        /// Calibrate board
        /// </summary>
        void Calibrate();

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
        event EventHandler BasePositionEvent;

        /// <summary>
        /// Return a best move
        /// </summary>
        string GetBestMove();

        /// <summary>
        /// Set COM-Port
        /// </summary>
        /// <param name="portName">Name of the COM-Port</param>
        void SetCOMPort(string portName);

        /// <summary>
        /// Set COM-Port
        /// </summary>
        /// <param name="portName">Name of the COM-Port</param>
        bool CheckCOMPort(string portName);

        /// <summary>
        /// Returns the current COM-Port
        /// </summary>
        /// <returns></returns>
        string GetCurrentCOMPort();

        bool IsOnBasePosition();

        void DimLeds(bool dimLeds);

        void FlashInSync(bool flashSync);

        void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);

        void SendCommand(string anyCommand);
    }
}

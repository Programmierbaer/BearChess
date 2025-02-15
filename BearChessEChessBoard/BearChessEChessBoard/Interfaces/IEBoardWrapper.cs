using System;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface IEBoardWrapper
    {
        void Reset();
        void Release();

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

        void ShowMove(string allMoves, string fenStartPosition, SetLEDsParameter setLeDsParameter, bool waitFor);

        void ShowMove(SetLEDsParameter setLeDsParameter);
        
        void SetLedsFor(SetLEDsParameter setLeDsParameter);

        /// <summary>
        /// All led off
        /// </summary>
        void SetAllLedsOff(bool forceOff);

        /// <summary>
        /// All led on
        /// </summary>
        void SetAllLedsOn();

        /// <summary>
        /// Returns the current position as fen (only pieces)
        /// </summary>
        DataFromBoard GetPiecesFen();

        DataFromBoard GetDumpPiecesFen();

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
        /// Indicates white wins the game
        /// </summary>
        void WhiteWins();

        /// <summary>
        /// Indicates black wins the game
        /// </summary>
        void BlackWins();

        /// <summary>
        /// Indicates game ends by a draw
        /// </summary>
        void IsADraw();

        /// <summary>
        /// Set a new position from fen 
        /// </summary>
        void SetFen(string fen, string allMoves);

        void AwaitingMove(int fromField, int toField);

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
        bool Calibrate();

        void SendInformation(string message);
        string RequestInformation(string message);
        void AdditionalInformation(string information);

        void SetCurrentColor(int currentColor);

        void SetEngineColor(int color);

        void RequestDump();

        void PlayWithWhite(bool withWhite);

        bool PlayingWithWhite { get; }

        /// <summary>
        /// Move made on board by user
        /// </summary>
        event EventHandler<string> MoveEvent;

        /// <summary>
        /// New FEN position on board by user
        /// </summary>
        event EventHandler<string> FenEvent;
        event EventHandler<string> DataEvent;
        event EventHandler<string[]> ProbeMoveEvent;

        event EventHandler AwaitedPosition;
        event EventHandler BasePositionEvent;
        event EventHandler NewGamePositionEvent;
        event EventHandler BatteryChangedEvent;
        event EventHandler HelpRequestedEvent;
        event EventHandler ProbeMoveEndingEvent;

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
        bool CheckCOMPort(string portName, string baud);

        /// <summary>
        /// Returns the current COM-Port
        /// </summary>
        /// <returns></returns>
        string GetCurrentCOMPort();
        string GetCurrentBaud();

        bool IsOnBasePosition();

        void DimLEDs(bool dimLeds);

        void DimLEDs(int level);

        void SetScanTime(int scanTime);

        void SetDebounce(int debounce);

        void FlashMode(EnumFlashMode flashMode);

        bool UseChesstimation { get; set; }

        void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);

        void SendCommand(string anyCommand);
        
        string BatteryLevel { get; }
        string BatteryStatus { get; }
        string Information { get; }
        string Level { get; }
        void AllowTakeBack(bool allowTakeBack);
        bool PieceRecognition { get; }
        void Ignore(bool ignore);

        void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack);

        void StopClock();
        void StartClock(bool white);
        void DisplayOnClock(string display);

        bool MultiColorLEDs { get; }

        bool ValidForAnalyse { get; }

        void AcceptProbingMoves(bool acceptProbingMoves);

        void BuzzerOnConnected();
        void BuzzerOnMove();
        void BuzzerOnCheck();
        void BuzzerOnDraw();
        void BuzzerOnCheckMate();
        void BuzzerOnInvalid();

        void PlayBuzzer(string soundCode);
    }
}

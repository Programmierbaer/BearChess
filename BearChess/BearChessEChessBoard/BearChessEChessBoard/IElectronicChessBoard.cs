using System;


namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface IElectronicChessBoard: IDisposable
    {

        string Identification { get; set; }

        EChessBoardConfiguration Configuration { get; }

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
        /// Indicates if there a connection to the chess board.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Shows the LED for the last move in <paramref name="allMoves"/>, limited by spaces.
        /// </summary>
        void ShowMove(string allMoves, string fenStartPosition, SetLEDsParameter setLeDsParameter, bool waitFor);

        /// <summary>
        /// Show fields <paramref name="fromField"/> and <paramref name="toField"/> on board.
        /// </summary>
        void ShowMove( SetLEDsParameter setLeDsParameter);

        /// <summary>
        /// Show all fields <paramref name="fields"/> on board.
        /// </summary>
   
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
        /// Returns the current position (awaited) as fen 
        /// </summary>
        string GetFen();

        /// <summary>
        /// Returns the current position on board
        /// </summary>
        /// <returns></returns>
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
        /// Set a new position from fen and all following moves
        /// </summary>
        void SetFen(string fen, string allMoves);
        void SetFen(string fen);

        void AwaitingMove(int fromField, int toField, int promoteFigureId);

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

        /// <summary>
        /// Send a information to display
        /// </summary>
        /// <param name="message"></param>
        void SendInformation(string message);
        string RequestInformation(string message);
        void AdditionalInformation(string information);

        void SendCommand(string command);

        void SetCurrentColor(int currentColor);

        void RequestDump();


        /// <summary>
        /// Play with white or black pieces from the base line.
        /// </summary>
        void PlayWithWhite(bool withWhite);

        bool PlayingWithWhite { get; }

        void SetEngineColor(int color);

        /// <summary>
        /// Move made on board by user
        /// </summary>
        event EventHandler<string> MoveEvent;
        
        event EventHandler<string> DataEvent;

        /// <summary>
        /// New FEN position on board by user
        /// </summary>
        event EventHandler<string> FenEvent;

        event EventHandler<string[]> ProbeMoveEvent;

        /// <summary>
        /// The awaited position is on the board
        /// </summary>
        event EventHandler AwaitedPosition;
        
        /// <summary>
        /// The pieces are replaced on the base position
        /// </summary>
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
        /// Set COM-Port to <paramref name="portName"/>.
        /// </summary>
        void SetComPort(string portName);

        /// <summary>
        /// Set COM-Port to <paramref name="portName"/>.
        /// </summary>
        bool CheckComPort(string portName);

        /// <summary>
        /// Returns the current COM-Port
        /// </summary>
        /// <returns></returns>
        string GetCurrentComPort();
        string GetCurrentBaud();

        /// <summary>
        /// Dim all LEDs
        /// </summary>
        void DimLeds(bool dimLeds);

        /// <summary>
        /// Synchronized flash all fields
        /// </summary>
        void FlashInSync(bool flashSync);
        void FlashMode(EnumFlashMode flashMode);

        void UseChesstimation(bool useChesstimation);

        /// <summary>
        /// Returns the configuration.
        /// </summary>
        EChessBoardConfiguration GetEChessBoardConfiguration();

        /// <summary>
        /// Save the configuration <paramref name="configuration"/>.
        /// </summary>
        void SaveEChessBoardConfiguration(EChessBoardConfiguration configuration);

        /// <summary>
        /// Set which leds around the field should mark 
        /// </summary>
        void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);

        /// <summary>
        /// Returns true if the board is current on the base position
        /// </summary>
        /// <returns></returns>
        bool IsOnBasePosition();

        string BatteryLevel { get; }
        string BatteryStatus { get; }
        string Information { get; }

        string Level { get; }
        void AllowTakeBack(bool allowTakeBack);
        bool PieceRecognition { get; }
        bool MultiColorLEDs { get; }
        bool ValidForAnalyse { get; }
        void Ignore(bool ignore);

        void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack);

        void StopClock();
        void StartClock(bool white);

        void DisplayOnClock(string display);

        void AcceptProbingMoves(bool acceptProbingMoves);

        void BuzzerOnConnected();
        void BuzzerOnMove();
        void BuzzerOnCheck();
        void BuzzerOnDraw();
        void BuzzerOnCheckMate();
        void BuzzerOnInvalid();
        void BuzzerSound(string soundCode);

    }
}

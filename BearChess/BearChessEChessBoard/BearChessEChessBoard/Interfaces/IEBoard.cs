using System;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface IEBoard : IDisposable
    {

        void Reset();
        
        void Release();

        void SetLedForFields(SetLEDsParameter ledsParameter);

        /// <summary>
        /// Switch all leds off
        /// </summary>
        void SetAllLedsOff(bool forceOff);

        /// <summary>
        /// Switch all leds on
        /// </summary>
        void SetAllLedsOn();

        /// <summary>
        /// Player is faces to the white pieces
        /// </summary>
        void PlayWithWhite();

        /// <summary>
        /// Player is faces to the black pieces
        /// </summary>
        void PlayWithBlack();

        bool PlayingWithWhite { get; }

        /// <summary>
        /// Performs a new calibration of the board. All pieces must be in their place.
        /// </summary>
        void Calibrate();

        void SendInformation(string message);
        void AdditionalInformation(string information);

        string RequestInformation(string message);

        void RequestDump();

        /// <summary>
        /// Indicates if a calibration needed
        /// </summary>
        /// <returns>true if calibration data present</returns>
        bool IsCalibrated { get; }

        /// <summary>
        /// Returns the pieces fen line from board 
        /// </summary>
        DataFromBoard GetPiecesFen();

        DataFromBoard GetDumpPiecesFen();

        /// <summary>
        /// Reset to a new game. Pieces are on base position.
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
        /// Indicates if all pieces are on base position
        /// </summary>
        bool PiecesAreOnBasePosition { get; }

        /// <summary>
        /// Indicates if the chess board is found and connected 
        /// </summary>
        /// <returns>False is the board is not found or connected</returns>
        bool IsConnected { get; }

        /// <summary>
        /// Set COM-Port
        /// </summary>
        /// <param name="portName">Name of the COM-Port</param>
        void SetComPort(string portName);

        /// <summary>
        /// Checks if the board is connected on the given COM port<paramref name="portName"/>.
        /// </summary>
        /// <param name="portName">COM port to check</param>
        /// <returns>Returns true if the board is connected</returns>
        bool CheckComPort(string portName);
        bool CheckComPort(string portName, string baud);

        string GetCurrentComPort();
        string GetCurrentBaud();
        string UnknownPieceCode { get; }
        void DimLeds(bool dimLeds);
        void DimLeds(int  level);
        void SetScanTime(int scanTime);
        void SetDebounce(int debounce);
        void FlashMode(EnumFlashMode flashMode);

        void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);
        void SendCommand(string anyCommand);
        string BatteryLevel { get; }
        string BatteryStatus { get; }
        string Level { get; }
        void SetDemoMode(bool inDemoMode);
        void AllowTakeBack(bool allowTakeBack);
        void SetFen(string fen);

        void AwaitingMove(int fromField, int toField);
        bool PieceRecognition { get; }
        bool SelfControlled { get; }

        bool MultiColorLEDs { get; }
        bool ValidForAnalyse { get; }

        bool UseFieldDumpForFEN { get; }

        void Stop(bool stop);
        void Ignore(bool ignore);
        string Information { get; }
        void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack);
        void StopClock();
        void StartClock(bool white);
        void DisplayOnClock(string display);

        void SetCurrentColor(int currentColor);
        void SetEngineColor(int color);

        event EventHandler BasePositionEvent;
        event EventHandler NewGamePositionEvent;
        event EventHandler HelpRequestedEvent;
        event EventHandler<string> DataEvent;
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

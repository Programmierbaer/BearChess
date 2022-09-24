using System;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface IEBoard : IDisposable
    {

        void Reset();

        /// <summary>
        /// Set the leds on for the given fields <paramref name="fromFieldName"/> and <paramref name="toFieldName"/>-
        /// </summary>
        /// <param name="fromFieldName">Field name e.g. E2</param>
        /// <param name="toFieldName">Field name e.g. E4</param>
        /// <param name="thinking"></param>
        /// <param name="isMove"></param>
        void SetLedForFields(string fromFieldName, string toFieldName, bool thinking, bool isMove, string displayString);

        /// <summary>
        /// Set the leds on for the given fields <paramref name="fieldNames"/>-
        /// </summary>
        /// <param name="fieldNames">Field names e.g. E2</param>
        /// <param name="thinking"></param>
        /// <param name="isMove"></param>
        void SetLedForFields(string[] fieldNames, bool thinking, bool isMove, string displayString);

        void SetLastLeds();

        /// <summary>
        /// Switch all leds off
        /// </summary>
        void SetAllLedsOff();

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

        /// <summary>
        /// Reset to a new game. Pieces are on base position.
        /// </summary>
        void NewGame();

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

        string GetCurrentCOMPort();

        string UnknownPieceCode { get; }

        void DimLeds(bool dimLeds);
        void DimLeds(int  level);

        void SetScanTime(int scanTime);

        void SetDebounce(int debounce);

        void FlashSync(bool flashSync);
        void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);
        void SendCommand(string anyCommand);

        string BatteryLevel { get; }
        string BatteryStatus { get; }

        void SetDemoMode(bool inDemoMode);
        void AllowTakeBack(bool allowTakeBack);

        void SetFen(string fen);
        bool PieceRecognition { get; }

        void Stop(bool stop);
        void Ignore(bool ignore);

        string Information { get; }

        void SetClock(int hourWhite, int minuteWhite, int minuteSec, int hourBlack, int minuteBlack, int secondBlack);

        void StopClock();

        void StartClock(bool white);

        void DisplayOnClock(string display);
    }
}

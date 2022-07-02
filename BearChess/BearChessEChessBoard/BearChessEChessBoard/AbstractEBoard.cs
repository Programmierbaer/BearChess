using System;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard

{
    public abstract class AbstractEBoard : IEBoard
    {

        protected ISerialCommunication _serialCommunication;
        protected object _locker = new object();
        protected ILogging _logger;
        protected bool _playWithWhite = true;
        protected bool _isFirstInstance;
        protected bool _inDemoMode = false;
        protected bool _allowTakeBack;
        private bool _pieceRecognition = true;
        protected bool _stopReading = false;
        protected bool _stopAll = false;
        protected bool _ignoreReading = false;


        public bool IsCalibrated { get; protected set; }
        public bool PiecesAreOnBasePosition { get; protected set; }
        public bool IsConnected { get; protected set; }

        public string UnknownPieceCode => "unknown";

        public abstract void Reset();

        public void SetLedForFields(string fromFieldName, string toFieldName, bool thinking, bool isMove, string displayString)
        {
            SetLedForFields(new[] { fromFieldName, toFieldName }, thinking, isMove, displayString);
        }

        public abstract bool CheckComPort(string portName);

        public abstract void SetLedForFields(string[] fieldNames, bool thinking, bool isMove, string displayString);

        public abstract void SetLastLeds();

        public abstract void SetAllLedsOff();

        public abstract void SetAllLedsOn();

        public abstract void DimLeds(bool dimLeds);

        public abstract void DimLeds(int level);
        public abstract void SpeedLeds(int level);

        public abstract void FlashSync(bool flashSync);

        public abstract void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);

        public abstract void Calibrate();

        public abstract void SendInformation(string message);

        public abstract void RequestDump();

        public abstract DataFromBoard GetPiecesFen();

        protected abstract void SetToNewGame();

        protected abstract void Release();

        public abstract void SetFen(string fen);

        public bool PieceRecognition
        {
            get => _pieceRecognition;
            set => _pieceRecognition = value;
        }

        public void Stop(bool stop)
        {
            _stopReading = stop;
        }

        public void Ignore(bool ignore)
        {
            _ignoreReading = ignore;
        }

        public  string BatteryLevel { get; protected set; }
        public  string BatteryStatus { get; protected set; }
        public  string Information { get; protected set; }

        public abstract void SetClock(int hourWhite, int minuteWhite, int minuteSec, int hourBlack, int minuteBlack,
                                      int secondBlack);

        public abstract void StopClock();
        public abstract void StartClock(bool white);
        public abstract void DisplayOnClock(string display);

        public void SetDemoMode(bool inDemoMode)
        {
            _inDemoMode = inDemoMode;
        }

        public void AllowTakeBack(bool allowTakeBack)
        {
            _allowTakeBack = allowTakeBack;
        }


        public void PlayWithWhite()
        {
            _playWithWhite = true;
        }

        public void PlayWithBlack()
        {
            _playWithWhite = false;
        }

        public bool PlayingWithWhite => _playWithWhite;

        public void NewGame()
        {
            _logger?.LogDebug("New game");
            EnsureConnection();
            SetToNewGame();
            _allowTakeBack = false;
        }

        public void SetComPort(string portName)
        {
            if (_serialCommunication.SetComPort(portName))
            {
                // Initiate Reconnect
                IsConnected = false;
            }
        }

        public string GetCurrentCOMPort()
        {
            return _serialCommunication.CurrentComPort;
        }

        public void SendCommand(string anyCommand)
        {
            lock (_locker)
            {
                _serialCommunication?.Send(anyCommand);
            }
        }

        protected bool EnsureConnection()
        {
            if (IsConnected)
            {
                return true;
            }

            if (!IsConnected && _serialCommunication.Connect())
            {
                _serialCommunication.StartCommunication();
                IsConnected = true;
                return true;
            }
            _logger?.LogError("B: chess board not found");
            return false;
        }

        public void Dispose()
        {
            _stopAll = true;
            _serialCommunication.DisConnect();
            Release();
        }
    }
}

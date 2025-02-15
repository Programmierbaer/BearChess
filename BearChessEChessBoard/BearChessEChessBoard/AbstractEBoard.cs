using System;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard

{
    public abstract class AbstractEBoard : IEBoard
    {

        protected ISerialCommunication _serialCommunication;
        protected object _locker = new object();
        protected ILogging _logger;
        protected bool _playWithWhite = true;
        protected bool _inDemoMode = false;
        protected bool _allowTakeBack;
        protected bool _stopReading = false;
        protected volatile bool _stopAll = false;
        protected bool _ignoreReading = false;
        protected bool _acceptProbingMoves = false;
        protected int _awaitingMoveFromField = Fields.COLOR_OUTSIDE;
        protected int _awaitingMoveToField = Fields.COLOR_OUTSIDE;


        public bool IsCalibrated { get; protected set; }

        public virtual void IsADraw() { }

        public bool PiecesAreOnBasePosition { get; protected set; }
        public bool IsConnected { get; protected set; }

        public string UnknownPieceCode => "unknown";

        public abstract void Reset();

        public abstract bool CheckComPort(string portName);
        public abstract bool CheckComPort(string portName, string baud);

        public abstract void SetLedForFields(SetLEDsParameter ledsParameter);

        public abstract void SetAllLedsOff(bool forceOff);

        public abstract void SetAllLedsOn();

        public abstract void DimLeds(bool dimLeds);

        public abstract void DimLeds(int level);
        public abstract void SpeedLeds(int level);

        public abstract void SetScanTime(int scanTime);

        public abstract void SetDebounce(int debounce);

        public abstract void FlashMode(EnumFlashMode flashMode);

        public abstract void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);

        public abstract void Calibrate();

        public abstract void SendInformation(string message);

        public virtual string RequestInformation(string message)
        {
            return string.Empty;
        }

        public abstract void AdditionalInformation(string information);

        public abstract void RequestDump();

        public abstract DataFromBoard GetPiecesFen();

        public abstract DataFromBoard GetDumpPiecesFen();

        protected abstract void SetToNewGame();

        public abstract void Release();

        public abstract void SetFen(string fen);


        public void AwaitingMove(int fromField, int toField)
        {
            _awaitingMoveFromField = fromField;
            _awaitingMoveToField = toField;
        }

        public bool PieceRecognition { get; set; } = true;

        public bool SelfControlled { get; set; }
        public bool MultiColorLEDs { get; set; } = false;
        public bool ValidForAnalyse { get; set; }

        public bool UseFieldDumpForFEN { get; set; } = false;

        public void Stop(bool stop)
        {
            _stopReading = stop;
            AcceptProbingMoves(false);
        }

        public void Ignore(bool ignore)
        {
            _ignoreReading = ignore;
        }

        public string BatteryLevel { get; protected set; }
        public string BatteryStatus { get; protected set; }
        public string Information { get; protected set; }
        public string Level { get; protected set; }


        public abstract void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack,
                                      int secondBlack);

        public abstract void StopClock();
        public abstract void StartClock(bool white);
        public abstract void DisplayOnClock(string display);
        public abstract void SetCurrentColor(int currentColor);
        public abstract void SetEngineColor(int color);

        public abstract event EventHandler BasePositionEvent;
        public abstract event EventHandler NewGamePositionEvent;
        public abstract event EventHandler HelpRequestedEvent;

        public abstract event EventHandler<string> DataEvent;
        public void AcceptProbingMoves(bool acceptProbingMoves)
        {
            _acceptProbingMoves = acceptProbingMoves;
        }

        public virtual void BuzzerOnConnected() { }
        public virtual void BuzzerOnMove() { }

        public virtual void BuzzerOnCheck(){ }

        public virtual void BuzzerOnDraw(){ }
        public virtual void PlayBuzzer(string soundCode){ }

        public virtual void BuzzerOnCheckMate() { }

        public virtual void BuzzerOnInvalid() { }

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

        public virtual void WhiteWins() {}

        public virtual void BlackWins()  { }

        public void SetComPort(string portName)
        {
            if (_serialCommunication.SetComPort(portName))
            {
                // Initiate Reconnect
                IsConnected = false;
            }
        }

        public string GetCurrentComPort()
        {
            return _serialCommunication.CurrentComPort;
        }

        public string GetCurrentBaud()
        {
            return _serialCommunication.CurrentBaud;
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
        }
    }
}

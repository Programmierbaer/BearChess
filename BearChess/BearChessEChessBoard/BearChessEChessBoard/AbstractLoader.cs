using System;
using System.IO;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public abstract class AbstractLoader : IElectronicChessBoard
    {
        private IEBoardWrapper _eChessBoard;
        private  string _configFile;
        public EChessBoardConfiguration Configuration { get; private set; }
        protected string Name { get; }
        protected bool Check { get; }



        public event EventHandler<string> MoveEvent;
        public event EventHandler<string> DataEvent;
        public event EventHandler<string> FenEvent;
        public event EventHandler<string[]> ProbeMoveEvent;
        public event EventHandler AwaitedPosition;
        public event EventHandler BasePositionEvent;
        public event EventHandler NewGamePositionEvent;
        public event EventHandler BatteryChangedEvent;
        public event EventHandler HelpRequestedEvent;
        public event EventHandler ProbeMoveEndingEvent;

        public void SetReplayMode(bool inReplayMode)
        {
            _eChessBoard.SetReplayMode(inReplayMode);
        }

        public bool IsInDemoMode => _eChessBoard.IsInDemoMode;
        public bool IsInReplayMode => _eChessBoard.IsInReplayMode;
        public bool IsConnected => _eChessBoard.IsConnected;


        protected AbstractLoader(bool check, string name)
        {
            Name = name;
            Check = check;
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                        Constants.BearChess, Name);
            _configFile = Path.Combine(basePath, $"{Name}Cfg.xml");
            Configuration = ReadConfiguration();
            // ReSharper disable once VirtualMemberCallInConstructor
            _eChessBoard = GetEBoardImpl(basePath, Configuration);
            _eChessBoard.HelpRequestedEvent += EChessBoard_HelpRequestedEvent;
        }


        protected AbstractLoader(string folderPath, string name)
        {
            Name = name;
            var basePath = Path.Combine(folderPath, name);
            _configFile = Path.Combine(basePath, $"{Name}Cfg.xml");
            Configuration = ReadConfiguration();
            Init(basePath);

        }

        protected AbstractLoader(string name)
        {
            Name = name;
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                        Constants.BearChess, name);
            _configFile = Path.Combine(basePath, $"{Name}Cfg.xml");
            Configuration = ReadConfiguration();
            Init(basePath);
        }

        public void SetEngineColor(int color)
        {
            _eChessBoard?.SetEngineColor(color);
        }

        private void Init(string basePath)
        {
            try
            {
                Directory.CreateDirectory(basePath);
                Directory.CreateDirectory(Path.Combine(basePath, "log"));
            }
            catch
            {
                return;
            }
            _configFile = Path.Combine(basePath, $"{Name}Cfg.xml");
            EChessBoardConfiguration configuration = ReadConfiguration();
            // ReSharper disable once VirtualMemberCallInConstructor
            _eChessBoard = GetEBoardImpl(basePath, configuration);
            _eChessBoard.FenEvent += EChessBoard_FenEvent;
            _eChessBoard.MoveEvent += EChessBoard_MoveEvent;
            _eChessBoard.AwaitedPosition += EChessBoard_AwaitedPosition;
            _eChessBoard.BasePositionEvent += EChessBoard_BasePositionEvent;
            _eChessBoard.NewGamePositionEvent += EChessBoard_NewGamePositionEvent;
            _eChessBoard.BatteryChangedEvent += EChessBoard_BatteryChangedEvent;
            _eChessBoard.DataEvent += EChessBoard_DataEvent;
            _eChessBoard.HelpRequestedEvent += EChessBoard_HelpRequestedEvent;
            _eChessBoard.ProbeMoveEvent += EChessBoard_ProbeMoveEvent;
            _eChessBoard.ProbeMoveEndingEvent += EChessBoard_ProbeMoveEndingEvent; ;
        }

        private void EChessBoard_ProbeMoveEndingEvent(object sender, EventArgs e)
        {
            ProbeMoveEndingEvent?.Invoke(this, e);
        }

        private void EChessBoard_ProbeMoveEvent(object sender, string[] e)
        {
            ProbeMoveEvent?.Invoke(this,e);
        }

        private void EChessBoard_HelpRequestedEvent(object sender, EventArgs e)
        {
           HelpRequestedEvent?.Invoke(this, e);
        }

        private void EChessBoard_DataEvent(object sender, string e)
        {
           DataEvent?.Invoke(this,e);
        }

        protected abstract IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration);

        public void Reset()
        {
            _eChessBoard.Reset();
        }

        public void Release()
        {
            _eChessBoard.Release();
        }

        /// <inheritdoc />
        public void SetDemoMode(bool inDemoMode)
        {
            _eChessBoard.SetDemoMode(inDemoMode);
        }

        /// <inheritdoc />
       

        public void ShowMove(string allMoves, string fenStartPosition, SetLEDsParameter setLeDsParameter, bool waitFor)
        {
            _eChessBoard.ShowMove(allMoves,fenStartPosition, setLeDsParameter, waitFor);
        }

        public void ShowMove(SetLEDsParameter setLeDsParameter)
        {
            _eChessBoard.ShowMove(setLeDsParameter);
        }

        /// <inheritdoc />
        public void SetLedsFor(string[] fields, bool thinking)
        {
            _eChessBoard.SetLedsFor(new SetLEDsParameter()
                                    {
                                        FieldNames = fields,
                                        IsThinking = thinking
                                    } );
        }

        public void SetLedsFor(SetLEDsParameter setLeDsParameter)
        {
            _eChessBoard.SetLedsFor(setLeDsParameter);
        }

        public void SetAllLedsOff(bool forceOff)
        {
            _eChessBoard.SetAllLedsOff(forceOff);
        }



        /// <inheritdoc />
        public void SetAllLedsOn()
        {
            _eChessBoard.SetAllLedsOn();
        }

        /// <inheritdoc />
        public DataFromBoard GetPiecesFen()
        {
            return _eChessBoard.GetPiecesFen();
        }

        public DataFromBoard GetDumpPiecesFen()
        {
            return _eChessBoard.GetDumpPiecesFen();
        }

        /// <inheritdoc />
        public string GetFen()
        {
            return _eChessBoard.GetFen();
        }

        public string GetBoardFen()
        {
            return _eChessBoard.GetBoardFen();
        }

        /// <inheritdoc />
        public void NewGame()
        {
            _eChessBoard.NewGame();
        }

        /// <inheritdoc />
        public void SetFen(string fen, string allMoves)
        {
            _eChessBoard.SetFen(fen, allMoves);
        }

        public void AwaitingMove(int fromField, int toField)
        {
            _eChessBoard.AwaitingMove(fromField, toField);
        }

        /// <inheritdoc />
        public void Close()
        {
            _eChessBoard.Close();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _eChessBoard.Stop();
        }

        public void Continue()
        {
            _eChessBoard.Continue();
        }

        /// <inheritdoc />
        public void Calibrate()
        {
            _eChessBoard.Calibrate();

        }

        public void SendInformation(string message)
        {
            _eChessBoard.SendInformation(message);
        }

        public string RequestInformation(string message)
        {
            return _eChessBoard.RequestInformation(message);
        }

        public void AdditionalInformation(string information)
        {
            _eChessBoard.AdditionalInformation(information);
        }

        public void SendCommand(string command)
        {
            _eChessBoard.SendCommand(command);
        }

        public void SetCurrentColor(int currentColor)
        {
            _eChessBoard.SetCurrentColor(currentColor);
        }

        public void RequestDump()
        {
            _eChessBoard.RequestDump();
        }

        /// <inheritdoc />
        public void PlayWithWhite(bool withWhite)
        {
            _eChessBoard.PlayWithWhite(withWhite);
        }

        public bool PlayingWithWhite => _eChessBoard.PlayingWithWhite;

        /// <inheritdoc />
        public string GetBestMove()
        {
            return _eChessBoard.GetBestMove();
        }

        /// <inheritdoc />
        public void SetComPort(string portName)
        {
            
            _eChessBoard.SetCOMPort(portName);
        }

        /// <inheritdoc />
        public bool CheckComPort(string portName)
        {
            
            return _eChessBoard.CheckCOMPort(portName);
        }
        public bool CheckComPort(string portName, string baud)
        {

            return _eChessBoard.CheckCOMPort(portName, baud);
        }

        public string GetCurrentComPort()
        {
            return _eChessBoard.GetCurrentCOMPort();
        }

        public string GetCurrentBaud()
        {
            return _eChessBoard.GetCurrentBaud();
        }

        /// <inheritdoc />
        public void DimLeds(bool dimLeds)
        {
            _eChessBoard.DimLEDs(dimLeds);
        }


        public void DimLeds(int level)
        {
            _eChessBoard.DimLEDs(level);
        }

        public void SetDebounce(int debounce)
        {
            _eChessBoard.SetDebounce(debounce);
        }

        /// <inheritdoc />
        public void FlashInSync(bool flashSync)
        {
            _eChessBoard.FlashMode(flashSync ? EnumFlashMode.FlashSync : EnumFlashMode.FlashAsync);
        }

        public void FlashMode(EnumFlashMode flashMode)
        {
            _eChessBoard.FlashMode(flashMode);
        }

        public void UseChesstimation(bool useChesstimation)
        {
            _eChessBoard.UseChesstimation = useChesstimation;
        }


        /// <inheritdoc />
        public EChessBoardConfiguration GetEChessBoardConfiguration()
        {
            return ReadConfiguration();
        }

        /// <inheritdoc />
        public void SaveEChessBoardConfiguration(EChessBoardConfiguration configuration)
        {
            SaveConfiguration(configuration);
        }

        /// <inheritdoc />
        public void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            _eChessBoard.SetLedCorner(upperLeft,upperRight,lowerLeft,lowerRight);
        }

        public bool IsOnBasePosition()
        {
            return _eChessBoard.IsOnBasePosition();
        }

        public string BatteryLevel => _eChessBoard.BatteryLevel;
        public string BatteryStatus => _eChessBoard.BatteryStatus;
        public string Information => _eChessBoard.Information;
        public string Level => _eChessBoard.Level;

        public void AllowTakeBack(bool allowTakeBack)
        {
            _eChessBoard.AllowTakeBack(allowTakeBack);
        }

        public bool PieceRecognition => _eChessBoard.PieceRecognition;
        public bool MultiColorLEDs => _eChessBoard.MultiColorLEDs;

        public bool ValidForAnalyse => _eChessBoard.ValidForAnalyse;

        //public bool Valif => _eChessBoard.Va;

        public void Ignore(bool ignore)
        {
            _eChessBoard.Ignore(ignore);
        }

        public void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
        {
            _eChessBoard.SetClock(hourWhite, minuteWhite, secWhite, hourBlack, minuteBlack, secondBlack);
        }

        public void StopClock()
        {
            _eChessBoard.StopClock();
        }

        public void StartClock(bool white)
        {
            _eChessBoard.StartClock(white);
        }

        public void DisplayOnClock(string display)
        {
            _eChessBoard.DisplayOnClock(display);
        }

        public void AcceptProbingMoves(bool acceptProbingMoves)
        {
            _eChessBoard.AcceptProbingMoves(acceptProbingMoves);
        }

        #region private

        private void EChessBoard_MoveEvent(object sender, string move)
        {
            MoveEvent?.Invoke(this, move);
        }

        private EChessBoardConfiguration ReadConfiguration()
        {
            return EChessBoardConfiguration.Load(_configFile);
        }

        private void SaveConfiguration(EChessBoardConfiguration configuration)
        {
            EChessBoardConfiguration.Save(configuration,_configFile);
        }

        private void EChessBoard_FenEvent(object sender, string fenPosition)
        {
            FenEvent?.Invoke(this, fenPosition);
        }

        private void EChessBoard_AwaitedPosition(object sender, EventArgs e)
        {
            AwaitedPosition?.Invoke(sender,e);
        }

        private void EChessBoard_BasePositionEvent(object sender, EventArgs e)
        {
            BasePositionEvent?.Invoke(sender, e);
        }

        private void EChessBoard_NewGamePositionEvent(object sender, EventArgs e)
        {
            NewGamePositionEvent?.Invoke(sender, e);
        }
        private void EChessBoard_BatteryChangedEvent(object sender, EventArgs e)
        {
            BatteryChangedEvent?.Invoke(sender, e);
        }


        #endregion

        public void Dispose()
        {
            _eChessBoard = null;
        }
    }
}

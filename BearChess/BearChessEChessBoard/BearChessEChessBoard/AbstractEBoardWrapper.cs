using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public abstract class AbstractEBoardWrapper : IEBoardWrapper
    {


        private bool _stopCommunication = false;
        private bool _allLedOff = true;
        private bool _forcedBasePosition = false;

        public string Name { get; }
        protected IInternalChessBoard _internalChessBoard;
        protected readonly ILogging _fileLogger;
        protected IEBoard _board;
        protected bool _stop = false;
        protected readonly ConcurrentQueue<string> _waitForFen = new ConcurrentQueue<string>();
        protected Thread _handleBoardThread;
        private bool _inDemoMode = false;
        private bool _inReplayMode = false;
        protected string _basePath;
        protected string _comPortName;
        protected string _baud;
        protected bool _useBluetooth;
        protected bool _useClock;
        protected bool _clockUpperCase;
        protected bool _showMovesOnly;
        protected bool _switchClockSide;

        protected EChessBoardConfiguration _configuration;
        private bool _piecesFenBasePosition;
        private string _lastChangedFigure = string.Empty;
        public bool UseChesstimation { get; set; }


        public event EventHandler<string> MoveEvent;
        public event EventHandler<string> FenEvent;
        public event EventHandler<string> DataEvent;
        public event EventHandler BasePositionEvent;
        public event EventHandler ProbeMoveEndingEvent;
        public event EventHandler AwaitedPosition;
        public event EventHandler BatteryChangedEvent;
        public event EventHandler HelpRequestedEvent;
        public event EventHandler<string[]> ProbeMoveEvent;

        protected AbstractEBoardWrapper(string name, string basePath)
        {
            Name = name;
            _basePath = basePath;

            try
            {
                _fileLogger = new FileLogger(Path.Combine(basePath, "log", $"{Name}_1.log"), 10, 10);
                _fileLogger.Active = bool.Parse(Configuration.Instance.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }

            // ReSharper disable once VirtualMemberCallInConstructor
            _board = GetEBoard(true);
            _board.BasePositionEvent += _board_BasePositionEvent;
            _board.DataEvent += _board_DataEvent;
            _board.HelpRequestedEvent += _board_HelpRequestedEvent;
        }

        private void _board_HelpRequestedEvent(object sender, EventArgs e)
        {
            HelpRequestedEvent?.Invoke(sender, EventArgs.Empty);
        }

        private void _board_DataEvent(object sender, string e)
        {
            DataEvent?.Invoke(sender, e);
        }

        protected AbstractEBoardWrapper(string name, string basePath, string comPortName, bool useChesstimation) : this(
            name, basePath, comPortName, string.Empty, false, false, false, false, useChesstimation, null)
        {

        }

        protected AbstractEBoardWrapper(string name, string basePath, string comPortName) : this(
            name, basePath, comPortName, string.Empty, false, false, false, false, false, null)
        {

        }

        protected AbstractEBoardWrapper(string name, string basePath, string comPortName,
            bool useBluetooth, bool useClock, bool showMovesOnly, bool switchClockSide) :
            this(name, basePath, comPortName, string.Empty, useBluetooth, useClock, showMovesOnly, switchClockSide,
                false, null)
        {


        }

        protected AbstractEBoardWrapper(string name, string basePath, EChessBoardConfiguration configuration) :
            this(name, basePath, configuration.PortName, string.Empty, configuration.UseBluetooth,
                configuration.UseClock,
                configuration.ClockShowOnlyMoves, configuration.ClockSwitchSide, false, configuration)
        {


        }

        protected AbstractEBoardWrapper(string name, string basePath, string comPortName, string baud,
            bool useBluetooth, bool useClock, bool showMovesOnly, bool switchClockSide, bool useChesstimation,
            EChessBoardConfiguration configuration)
        {
            _configuration = configuration;
            Name = name;
            _basePath = basePath;
            _comPortName = comPortName;
            _baud = baud;
            _useBluetooth = useBluetooth;
            _useClock = useClock;
            _clockUpperCase = configuration?.ClockUpperCase ?? false;
            _showMovesOnly = showMovesOnly;
            _switchClockSide = switchClockSide;
            UseChesstimation = useChesstimation;
            try
            {
                _fileLogger = new FileLogger(Path.Combine(basePath, "log", $"{Name}_1.log"), 10, 10);
                _fileLogger.Active = bool.Parse(Configuration.Instance.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }

            Init();
        }

        public abstract bool Calibrate();
        public abstract void SendInformation(string message);

        public void SetEngineColor(int color)
        {
            _board?.SetEngineColor(color);
        }

        public void RequestDump()
        {
            _board?.RequestDump();
        }

        public bool CheckCOMPort(string portName, string baud)
        {
            _fileLogger?.LogDebug($"AB: Set COM-Port to: {portName} with {baud}");
            return _board.CheckComPort(portName, baud);
        }

        public string GetCurrentCOMPort()
        {
            return _board?.GetCurrentCOMPort();
        }

        public string GetCurrentBaud()
        {
            return _board?.GetCurrentBaud();
        }

        public bool IsOnBasePosition()
        {
            return _piecesFenBasePosition;
        }

        public abstract void DimLEDs(bool dimLeds);

        public abstract void DimLEDs(int level);

        public abstract void SetScanTime(int scanTime);
        public abstract void SetDebounce(int debounce);

        public abstract void FlashMode(EnumFlashMode flashMode);

        public void SetCurrentColor(int currentColor)
        {
            _board?.SetCurrentColor(currentColor);
        }


        public void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            _board?.SetLedCorner(upperLeft, upperRight, lowerLeft, lowerRight);
        }

        public void SendCommand(string anyCommand)
        {
            _board?.SendCommand(anyCommand);
        }

        public void AdditionalInformation(string information)
        {
            _board?.AdditionalInformation(information);
        }

        public string BatteryLevel => _board?.BatteryLevel;
        public string BatteryStatus => _board?.BatteryStatus;
        public string Information => _board?.Information;
        public string Level => _board?.Level;

        public void AllowTakeBack(bool allowTakeBack)
        {
            _board?.AllowTakeBack(allowTakeBack);
        }

        public bool PieceRecognition => _board?.PieceRecognition ?? true;

        public void Ignore(bool ignore)
        {
            _board?.Ignore(ignore);
        }

        public void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack,
            int secondBlack)
        {
            _board?.SetClock(hourWhite, minuteWhite, secWhite, hourBlack, minuteBlack, secondBlack);
        }

        public void StartClock(bool white)
        {
            _board?.StartClock(white);
        }

        public void DisplayOnClock(string display)
        {
            _board?.DisplayOnClock(display);
        }

        public bool MultiColorLEDs => _board?.MultiColorLEDs ?? false;

        public void StopClock()
        {
            _board?.StopClock();
        }

        protected abstract IEBoard GetEBoard();
        protected abstract IEBoard GetEBoard(bool check);

        public void Reset()
        {
            _board?.Reset();
        }

        public void Release()
        {
            _board?.Release();
        }

        /// <inheritdoc />
        public void SetDemoMode(bool inDemoMode)
        {
            _fileLogger?.LogDebug($"AB: Switch into demo mode: {inDemoMode}");
            _inDemoMode = inDemoMode;
            if (!_inDemoMode)
            {
                _inReplayMode = false;
            }

            _board?.SetDemoMode(inDemoMode);
        }

        /// <inheritdoc />
        public void SetReplayMode(bool inReplayMode)
        {
            _fileLogger?.LogDebug($"AB: Switch into replay mode: {inReplayMode}");
            _inReplayMode = inReplayMode;
            _inDemoMode = inReplayMode;
        }

        /// <inheritdoc />
        public bool IsInDemoMode => _inDemoMode;

        /// <inheritdoc />
        public bool IsInReplayMode => _inReplayMode;

        /// <inheritdoc />
        public bool IsConnected => _board?.IsConnected ?? false;

        public void ShowMove(string allMoves, string startFenPosition, SetLEDsParameter setLeDsParameter, bool waitFor)
        {
            if (string.IsNullOrWhiteSpace(allMoves))
            {
                _fileLogger?.LogError("AB: No moves given for ShowMove");
                return;
            }


            _fileLogger?.LogDebug($"AB: Show Move for: {allMoves}");
            var moveList = allMoves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            _internalChessBoard = new InternalChessBoard();
            _internalChessBoard.NewGame();
            if (!string.IsNullOrWhiteSpace(startFenPosition))
            {
                _internalChessBoard.SetPosition(startFenPosition);
            }

            foreach (string move in moveList)
            {
                if (move.Length < 4)
                {
                    _fileLogger?.LogError($"AB: Invalid move {move}");
                    return;
                }

                setLeDsParameter.Promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                _internalChessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), setLeDsParameter.Promote);
            }

            var lastMove = moveList[moveList.Length - 1];
            setLeDsParameter.Promote = lastMove.Length == 4 ? string.Empty : lastMove.Substring(4, 1);
            _fileLogger?.LogDebug($"AB: Show Move {lastMove}");
            var position = _internalChessBoard.GetPosition();

            if (waitFor && !_board.SelfControlled)
            {
                _fileLogger?.LogDebug($"AB: Wait for: {position}");
                // Set LEDs on for received move and wait until board is in same position
                _waitForFen.Enqueue(position);
            }

            if (_board != null && !_board.SelfControlled)
            {
                _board?.SetLedForFields(new SetLEDsParameter()
                {
                    FieldNames = new[] { lastMove.Substring(0, 2), lastMove.Substring(2, 2) },
                    Promote = setLeDsParameter.Promote,
                    IsThinking = false,
                    IsTakeBack = true,
                    IsMove = false,
                    DisplayString = string.Empty
                });
            }

            _stop = false;
        }



        public void ShowMove(SetLEDsParameter setLeDsParameter)
        {
            _internalChessBoard.MakeMove(setLeDsParameter.FieldNames[0], setLeDsParameter.FieldNames[1],
                setLeDsParameter.Promote);
            var position = _internalChessBoard.GetPosition();
            _waitForFen.Enqueue(position);
            _board?.SetLedForFields(setLeDsParameter);
            _stop = false;
        }

        public void SetLedsFor(SetLEDsParameter setLeDsParameter)
        {
            _board?.SetLedForFields(setLeDsParameter);
        }

        public void SetAllLedsOff(bool forceOff)
        {
            if (!_allLedOff)
            {
                _fileLogger?.LogDebug("AB: Set all LEDs off");
                while (_waitForFen.Count > 0)
                {
                    _waitForFen.TryDequeue(out _);
                }

                _board?.SetAllLedsOff(forceOff);
                while (_waitForFen.Count > 0)
                {
                    _waitForFen.TryDequeue(out _);
                }
            }

            _allLedOff = true;
        }

        public void SetAllLedsOn()
        {
            _allLedOff = false;
            _board?.SetAllLedsOn();
        }

        public DataFromBoard GetPiecesFen()
        {
            return _board?.GetPiecesFen();
        }

        public DataFromBoard GetDumpPiecesFen()
        {
            return _board?.GetDumpPiecesFen();
        }

        public string GetFen()
        {
            return _internalChessBoard.GetPosition();
        }

        public string GetBoardFen()
        {
            return _board?.GetPiecesFen().FromBoard;
        }

        public void NewGame()
        {
            _fileLogger?.LogDebug("AB: New game");
            _board?.NewGame();
            _internalChessBoard = new InternalChessBoard();
            _internalChessBoard.NewGame();
            SetAllLedsOff(false);
            while (_waitForFen.Count > 0)
            {
                _waitForFen.TryDequeue(out _);
            }

            _stop = false;
            _waitForFen.Enqueue(_internalChessBoard.GetPosition());
        }

        public void SetFen(string fen, string allMoves)
        {
            if (string.IsNullOrWhiteSpace(fen))
            {
                _fileLogger?.LogError("AB: No fen position");
                return;
            }

            SetAllLedsOff(false);
            _fileLogger?.LogDebug($"AB: set fen position: {fen}");
            _internalChessBoard = new InternalChessBoard();

            _internalChessBoard.NewGame();
            _internalChessBoard.SetPosition(fen);
            var dataFromBoard = _board?.GetPiecesFen();
            _fileLogger?.LogDebug($"AB: current Fen: {dataFromBoard?.FromBoard}");
            _board?.SetFen(fen);
            var position = _internalChessBoard.GetPosition();

            if (string.IsNullOrWhiteSpace(allMoves))
            {
                _fileLogger?.LogDebug($"AB: Wait for: {position}");
                // Set LEDs on for received fen and wait until board is in same position
                _waitForFen.Enqueue(position);
                _stop = false;
                return;
            }

            var moveList = allMoves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string promote = string.Empty;
            foreach (string move in moveList)
            {
                _fileLogger?.LogError($"AB: move after fen: {move}");
                if (move.Length < 4)
                {
                    _fileLogger?.LogError($"AB: Invalid move {move}");
                    return;
                }

                promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                _internalChessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
            }

            var lastMove = moveList[moveList.Length - 1];
            promote = lastMove.Length == 4 ? string.Empty : lastMove.Substring(4, 1);
            _fileLogger?.LogDebug($"AB: Show Move {lastMove}");
            position = _internalChessBoard.GetPosition();
            _fileLogger?.LogDebug($"AB: Wait for: {position}");
            // Set LEDs on for received move and wait until board is in same position
            _waitForFen.Enqueue(position);
            _board?.SetLedForFields(new SetLEDsParameter()
            {
                FieldNames = new[] { lastMove.Substring(0, 2), lastMove.Substring(2, 2) },
                Promote = promote,
                IsThinking = false,
                IsMove = true,
                DisplayString = string.Empty
            });
            _stop = false;

        }

        public void Close()
        {
            _stop = true;
            _allLedOff = false;
            SetAllLedsOff(true);
            _stopCommunication = true;
            try
            {
                _board?.Stop(_stop);

                _board?.Dispose();
            }
            catch
            {
                //
            }

            _board = null;
        }


        public void Stop()
        {
            _stop = true;
            _board.Stop(_stop);
        }

        public void Continue()
        {
            _stop = false;
            _board.Stop(_stop);
        }

        public void PlayWithWhite(bool withWhite)
        {
            _fileLogger?.LogDebug($"AB: Play with white: {withWhite}");
            if (withWhite)
            {
                _board.PlayWithWhite();
                return;
            }

            _board.PlayWithBlack();
        }

        public bool PlayingWithWhite => _board?.PlayingWithWhite ?? true;


        public string GetBestMove()
        {
            return _internalChessBoard.GetBestMove();
        }

        public void SetCOMPort(string portName)
        {
            _fileLogger?.LogDebug($"AB: Set COM-Port to: {portName}");
            _board.SetComPort(portName);
        }

        public bool CheckCOMPort(string portName)
        {
            _fileLogger?.LogDebug($"AB: Set COM-Port to: {portName}");
            return _board.CheckComPort(portName);

        }


        #region private

        private void Init()
        {
            _board = GetEBoard();
            _board.BasePositionEvent += _board_BasePositionEvent;
            _board.DataEvent += _board_DataEvent;
            _board.HelpRequestedEvent += _board_HelpRequestedEvent;
            if (!_board.IsCalibrated)
            {
                _board.Calibrate();
            }

            SetAllLedsOn();
            Thread.Sleep(100);
            SetAllLedsOff(true);
            _handleBoardThread = new Thread(HandleBoard) { IsBackground = true };
            _handleBoardThread.Start();
            _internalChessBoard = new InternalChessBoard();
            _internalChessBoard.NewGame();
            _inDemoMode = false;
            _inReplayMode = false;

        }

        private void _board_BasePositionEvent(object sender, EventArgs e)
        {

            _forcedBasePosition = true;
        }

        private void HandleBoard()
        {
            string waitForFen = string.Empty;
            string currentFen = string.Empty;
            string prevFen = string.Empty;
            string potentialMove = string.Empty;
            string changedFenHelper = string.Empty;
            string batteryLevel = string.Empty;
            string batteryStatus = string.Empty;
            _fileLogger?.LogDebug("AB: Handle board");
            while (!_stopCommunication)
            {
                Thread.Sleep(5);
                if (_stop)
                {
                    continue;
                }

                if (_waitForFen.TryDequeue(out string fen))
                {
                    waitForFen = fen;
                    _fileLogger?.LogDebug($"AB: Wait for fen: {waitForFen}");
                }
                if (!batteryLevel.Equals(_board?.BatteryLevel) || (!batteryStatus.Equals(_board?.BatteryStatus)))
                {
                    batteryLevel = _board?.BatteryLevel;
                    batteryStatus = _board?.BatteryStatus;
                    BatteryChangedEvent?.Invoke(this, null);
                }
                var piecesFen = GetPiecesFenAndSetLedsForInvalidField(!string.IsNullOrWhiteSpace(waitForFen)) ??
                                _board?.GetPiecesFen();
                if (piecesFen == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(piecesFen.FromBoard) ||
                    piecesFen.FromBoard.Equals(_board?.UnknownPieceCode))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentFen))
                {
                    currentFen = piecesFen.FromBoard;
                }

                _piecesFenBasePosition = piecesFen.BasePosition;
                if (piecesFen.Repeated > 2 && !string.IsNullOrWhiteSpace(potentialMove))
                {
                    _fileLogger?.LogDebug(
                        $"AB: Fen {piecesFen.FromBoard} repeated {piecesFen.Repeated} for same position. OnMoveEvent for: {potentialMove}  {currentFen}");
                    potentialMove = string.Empty;
                    OnMoveEvent(currentFen);
                }

                // Something changed on board?
                if (!piecesFen.FromBoard.Equals(currentFen))
                {
                    if (changedFenHelper != $"{piecesFen.FromBoard}{currentFen}")
                    {
                        _fileLogger?.LogDebug($"AB: Fen on board changed from {currentFen} to {piecesFen.FromBoard}");
                    }

                    changedFenHelper = $"{piecesFen.FromBoard}{currentFen}";
                }

                // Awaited position on board?
                if (!string.IsNullOrWhiteSpace(waitForFen) &&
                    !string.IsNullOrWhiteSpace(piecesFen.FromBoard) &&
                    waitForFen.StartsWith(piecesFen.FromBoard))
                {
                    _fileLogger?.LogDebug($"AB: Awaited fen position on board received: {waitForFen}");
                    _board?.SetAllLedsOff(false);

                    currentFen = piecesFen.FromBoard;
                    waitForFen = string.Empty;
                    AwaitedPosition?.Invoke(this, null);
                }

                if (_forcedBasePosition || (piecesFen.Invalid && _piecesFenBasePosition))
                {
                    _forcedBasePosition = false;
                    BasePositionEvent?.Invoke(this, null);
                }

               

                if (!string.IsNullOrWhiteSpace(waitForFen))
                {

                    continue;
                }

                if (!prevFen.Equals(piecesFen.FromBoard))
                {
                    var changedFigure = _internalChessBoard.GetChangedFigure(prevFen, piecesFen.FromBoard);
                    prevFen = piecesFen.FromBoard;
                    OnFenEvent(piecesFen.FromBoard, changedFigure);
                }

                if (currentFen.Equals(piecesFen.FromBoard))
                {
                    continue;
                }

                var move = _internalChessBoard.GetMove(piecesFen.FromBoard, false);
                if (move.Length >= 4)
                {
                    _fileLogger?.LogDebug($"AB: Move detected: {move}");

                    currentFen = piecesFen.FromBoard;
                    if (string.IsNullOrWhiteSpace(potentialMove))
                    {
                        _fileLogger?.LogDebug($"AB: Set as potential move: {move}");
                        potentialMove = move;
                        continue;
                    }

                    if (potentialMove.Equals(move))
                    {
                        _fileLogger?.LogDebug($"AB: OnMoveEvent: move equal to potential move: {move}");
                        potentialMove = string.Empty;
                        OnMoveEvent(piecesFen.FromBoard);

                        continue;
                    }

                    if (potentialMove.StartsWith(move.Substring(0, 2)))
                    {
                        _fileLogger?.LogDebug(
                            $"AB: Slow figure: new move {move} starts with potential move: {potentialMove}");
                        potentialMove = move;
                        continue;
                    }
                }
            }

        }



        /// <summary>
        /// New fen position received from chess board. Translate to a move
        /// </summary>
        private void OnMoveEvent(string fenPosition)
        {
            _fileLogger?.LogDebug($"AB: MoveEvent from board: {fenPosition}");
            // Calculate move from current position to new fen position
            var move = _internalChessBoard.GetMove(fenPosition, _inDemoMode);
            if (string.IsNullOrWhiteSpace(move))
            {
                _fileLogger?.LogDebug("AB: Move is invalid");
                return;
            }

            _fileLogger?.LogDebug($"AB: Move detected: {move}");
            string promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
            _internalChessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
            // Inform the world about a new move 
            MoveEvent?.Invoke(this, move);
        }

        /// <summary>
        /// New fen position received from chess board
        /// </summary>
        private void OnFenEvent(string fenPosition, string changedFigure)
        {
            string c = changedFigure.ToLower().Equals(changedFigure) ? "w" : "b";
            if (_inReplayMode)
            {
                if (_lastChangedFigure == changedFigure)
                {
                    c = c == "w" ? "b" : "w";
                }
            }

            _lastChangedFigure = changedFigure;
            //      _lastFenColor = c;
            _fileLogger?.LogDebug($"AB: FenEvent from board: {fenPosition} {changedFigure}");
            FenEvent?.Invoke(this, fenPosition + " " + c);
        }

        private DataFromBoard GetPiecesFenAndSetLedsForInvalidField(bool waitMove)
        {
            try
            {
                // Every position is possible
                if (_inDemoMode || _stop)
                {
                    if (!_allLedOff)
                    {
                        _board?.SetAllLedsOff(false);
                        _allLedOff = true;
                    }

                    return null;
                }

                if (_board.SelfControlled)
                {
                    return null;
                }

                //_board?.SetAllLedsOff();
                var fenPosition = _board?.GetPiecesFen();
                if (fenPosition == null)
                {
                    return null;
                }

                var invalidFields = new List<string>();
                var validFields = new List<string>();
                Move prevMove = null;
                if (fenPosition.IsFieldDump)
                {
                    var hashSet = new HashSet<string>(fenPosition.FromBoard.Split(",".ToCharArray()));
                    for (int i = 0; i < Fields.BoardFields.Length; i++)
                    {
                        int f = Fields.BoardFields[i];
                        var figureOnField = _internalChessBoard.GetFigureOnField(f);
                        var fieldName = InternalChessBoard.GetFieldName(f);
                        if (string.IsNullOrWhiteSpace(figureOnField))
                        {
                            if (hashSet.Contains(fieldName))
                            {
                                invalidFields.Add(fieldName);
                                hashSet.Remove(fieldName);
                            }
                        }
                        else
                        {
                            if (hashSet.Contains(fieldName))
                            {
                                hashSet.Remove(fieldName);
                            }
                        }

                    }

                    invalidFields.AddRange(hashSet);
                    if (invalidFields.Count > 0)
                    {
                        fenPosition.Invalid = true;
                    }

                    invalidFields.Clear();
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(fenPosition.FromBoard) &&
                        fenPosition.FromBoard.Equals(_board?.UnknownPieceCode))
                    {
                        return fenPosition;
                    }

                    var internalChessBoard = new InternalChessBoard();
                    internalChessBoard.NewGame();
                    internalChessBoard.SetPosition(fenPosition.FromBoard);
                    HashSet<string> prevMoveFields = new HashSet<string>();
                    var allMoveClass = _internalChessBoard.GetPrevMove();

                    if (allMoveClass != null)
                    {
                        prevMove = allMoveClass.GetMove(Fields.COLOR_BLACK);
                        if (prevMove == null)
                        {
                            prevMove = allMoveClass.GetMove(Fields.COLOR_WHITE);
                        }

                        if (prevMove != null)
                        {
                            // _fileLogger.LogDebug($"AB: Prev move: {prevMove.FromFieldName}-{prevMove.ToFieldName} ");
                            prevMoveFields.Add(prevMove.FromFieldName);
                            prevMoveFields.Add(prevMove.ToFieldName);
                            if (prevMove.IsCastleMove())
                            {
                                if (prevMove.FromField == Fields.FE1)
                                {
                                    prevMoveFields.Add("F1");
                                    prevMoveFields.Add("H1");
                                }

                                if (prevMove.FromField == Fields.FE8)
                                {
                                    prevMoveFields.Add("F8");
                                    prevMoveFields.Add("H8");

                                }
                            }
                        }
                        else
                        {
                            _fileLogger.LogDebug($"AB: Prev move is null");
                        }
                    }

                    for (var f = InternalChessBoard.FA1; f <= InternalChessBoard.FH8; f++)
                    {
                        if (!_internalChessBoard.GetFigureOnField(f).Equals(internalChessBoard.GetFigureOnField(f)))
                        {
                            //_fileLogger?.LogDebug($"AB: Not equal on {f}: {_internalChessBoard.GetFigureOnField(f)} vs. {internalChessBoard.GetFigureOnField(f)}");
                            var fieldName = InternalChessBoard.GetFieldName(f);
                            if (_board != null && _board.MultiColorLEDs)
                            {
                                if (prevMoveFields.Contains(fieldName))
                                {
                                    validFields.Add(fieldName);
                                }
                                else
                                {
                                    invalidFields.Add(fieldName);
                                }
                            }
                            else
                            {
                                invalidFields.Add(fieldName);
                            }
                        }
                    }
                }

                if (invalidFields.Count > 0 || validFields.Count > 0)
                {
                    fenPosition.Invalid = true;
                    //     if (_board!=null && _board.MultiColorLEDs)
                    if (_board != null)
                    {
                        Move[] checkMoves;
                        if (prevMove != null && waitMove)
                        {
                            if (prevMove.FigureColor == Fields.COLOR_WHITE)
                            {
                                checkMoves = new List<Move>(_internalChessBoard.CurrentMoveList).ToArray();
                            }
                            else
                            {
                                checkMoves = new List<Move>(_internalChessBoard.EnemyMoveList).ToArray();
                            }
                        }
                        else
                        {
                            checkMoves = new List<Move>(_internalChessBoard.CurrentMoveList).ToArray();
                        }

                        foreach (var move in checkMoves)
                        {
                            if (invalidFields.Contains(move.FromFieldName) && invalidFields.Contains(move.ToFieldName))
                            {
                                validFields.Add(move.ToFieldName);
                                invalidFields.Remove(move.ToFieldName);
                                validFields.Add(move.FromFieldName);
                                invalidFields.Remove(move.FromFieldName);
                                break;
                            }

                            if (validFields.Count == 0)
                            {
                                if (invalidFields.Count == 1 && invalidFields.Contains(move.ToFieldName))
                                {
                                    validFields.Add(move.ToFieldName);
                                    invalidFields.Remove(move.ToFieldName);
                                    break;
                                }

                                if (invalidFields.Count == 1 && invalidFields.Contains(move.FromFieldName))
                                {
                                    validFields.Add(move.FromFieldName);
                                    invalidFields.Remove(move.FromFieldName);
                                    break;
                                }
                            }

                        }

                        ;
                        var setLeDsParameter = new SetLEDsParameter()
                        {
                            FieldNames = validFields.ToArray(),
                            InvalidFieldNames = invalidFields.ToArray(),
                            Promote = string.Empty,
                            IsThinking = false,
                            IsMove = invalidFields.Count == 0,
                            IsError = invalidFields.Count > 0,
                            IsProbing = false,
                            DisplayString = string.Empty

                        };
                        if (_configuration.ShowPossibleMoves && setLeDsParameter.IsMove &&
                            setLeDsParameter.FieldNames.Length == 1 && !waitMove)
                        {
                            List<string> hintFields = new List<string> { setLeDsParameter.FieldNames[0] };
                            foreach (var move in checkMoves)
                            {
                                if (move.FromFieldName.Equals(setLeDsParameter.FieldNames[0]))
                                {
                                    hintFields.Add(move.ToFieldName);
                                }
                            }

                            if (hintFields.Count() > 1)
                            {
                                _fileLogger?.LogDebug($"AB: ProbeMoveEvent for  {string.Join(" ", hintFields)} ");
                                ProbeMoveEvent?.Invoke(this, hintFields.ToArray());
                            }
                        }

                        if (!_configuration.ShowOwnMoves && setLeDsParameter.IsMove)
                        {
                            setLeDsParameter.FieldNames = Array.Empty<string>();
                        }

                        _board?.SetLedForFields(setLeDsParameter);

                        //if (_board.MultiColorLEDs || _configuration.ShowOwnMoves)
                        //{
                        //    _board?.SetLedForFields(setLeDsParameter);
                        //}
                        //else
                        //{
                        //    if (!setLeDsParameter.IsMove && setLeDsParameter.FieldNames.Length==0)
                        //    {
                        //        setLeDsParameter.FieldNames = setLeDsParameter.InvalidFieldNames.ToArray();
                        //        _board?.SetLedForFields(setLeDsParameter);
                        //    }
                        //}
                    }
                    else
                    {
                        var setLeDsParameter = new SetLEDsParameter()
                        {
                            FieldNames = invalidFields.ToArray(),
                            Promote = string.Empty,
                            IsThinking = false,
                            IsMove = invalidFields.Count == 0,
                            IsError = true,
                            IsProbing = false,
                            DisplayString = string.Empty

                        };
                        //if (!_configuration.ShowOwnMoves && setLeDsParameter.IsMove)
                        if (!_configuration.ShowOwnMoves)
                        {

                            {
                                Move[] checkMoves;
                                if (prevMove != null && waitMove)
                                {
                                    if (prevMove.FigureColor == Fields.COLOR_WHITE)
                                    {
                                        checkMoves = new List<Move>(_internalChessBoard.CurrentMoveList).ToArray();
                                    }
                                    else
                                    {
                                        checkMoves = new List<Move>(_internalChessBoard.EnemyMoveList).ToArray();
                                    }
                                }
                                else
                                {
                                    checkMoves = new List<Move>(_internalChessBoard.CurrentMoveList).ToArray();
                                }

                                setLeDsParameter.IsMove = true;
                                foreach (var invalidField in invalidFields)
                                {
                                    bool found = false;
                                    foreach (var checkMove in checkMoves)
                                    {
                                        if (checkMove.FromFieldName.Equals(invalidField) ||
                                            checkMove.ToFieldName.Equals(invalidField))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        setLeDsParameter.IsMove = false;
                                        break;
                                    }
                                }
                            }
                            if (setLeDsParameter.IsMove)
                                setLeDsParameter.FieldNames = Array.Empty<string>();
                        }

                        _board?.SetLedForFields(setLeDsParameter);
                    }

                    _allLedOff = false;
                }
                else
                {
                    if (_waitForFen.IsEmpty && !_stop)
                    {
                        ProbeMoveEndingEvent?.Invoke(this, EventArgs.Empty);
                        if (!_allLedOff && !fenPosition.IsFieldDump)
                        {
                            _fileLogger?.LogDebug("AB: Waiting for fen is empty: Set all LEDs off");
                            _board?.SetAllLedsOff(false);
                            _board?.SetCurrentColor(_internalChessBoard.CurrentColor);
                            _allLedOff = true;
                        }
                    }
                }

                return fenPosition;
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
                return null;
            }
        }


    }


    #endregion

}
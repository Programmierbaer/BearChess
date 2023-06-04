using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

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
        protected  bool _useClock;
        protected  bool _showMovesOnly;
        protected  bool _switchClockSide;
        private bool _piecesFenBasePosition;
        private string _lastChangedFigure = string.Empty;
        private string _lastFenColor = string.Empty;
        public bool UseChesstimation { get; set; }


        public event EventHandler<string> MoveEvent;
        public event EventHandler<string> FenEvent;
        public event EventHandler<string> DataEvent;
        public event EventHandler BasePositionEvent;
        public event EventHandler AwaitedPosition;
        public event EventHandler BatteryChangedEvent;
        public event EventHandler HelpRequestedEvent;

        protected AbstractEBoardWrapper(string name, string basePath)
        {
            Name = name;
            _basePath = basePath;

            try
            {
                _fileLogger = new FileLogger(Path.Combine(basePath, "log", $"{Name}_1.log"), 10, 10);
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
            name, basePath, comPortName, string.Empty, false, false, false, false, useChesstimation)
        {

        }

        protected AbstractEBoardWrapper(string name, string basePath,  string comPortName) : this(
            name, basePath, comPortName,string.Empty, false, false,false, false, false)
        {

        }

        protected AbstractEBoardWrapper(string name, string basePath, string comPortName,
                                        bool useBluetooth, bool useClock, bool showMovesOnly, bool switchClockSide):
            this(name,basePath,comPortName,string.Empty,useBluetooth, useClock, showMovesOnly, switchClockSide, false)
        {


        }

        protected AbstractEBoardWrapper(string name, string basePath, string comPortName,string baud,
                                        bool useBluetooth, bool useClock, bool showMovesOnly, bool switchClockSide, bool useChesstimation = false)
        {
            Name = name;
            _basePath = basePath;
            _comPortName = comPortName;
            _baud = baud;
            _useBluetooth = useBluetooth;
            _useClock = useClock;
            _showMovesOnly = showMovesOnly;
            _switchClockSide = switchClockSide;
            UseChesstimation = useChesstimation;
            try
            {
                _fileLogger = new FileLogger(Path.Combine(basePath, "log", $"{Name}_1.log"), 10, 10);
            }
            catch
            {
                _fileLogger = null;
            }

            Init();
        }

        public abstract bool Calibrate();
        public abstract void SendInformation(string message);

        public void RequestDump()
        {
            _board?.RequestDump();
        }

        public bool CheckCOMPort(string portName, string baud)
        {
            _fileLogger?.LogDebug($"C: Set COM-Port to: {portName} with {baud}");
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
            _board?.SetLedCorner(upperLeft,upperRight,lowerLeft,lowerRight);
        }

        public void SendCommand(string anyCommand)
        {
            _board?.SendCommand(anyCommand);
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

        public void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
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
            _fileLogger?.LogDebug($"C: Switch into demo mode: {inDemoMode}");
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
            _fileLogger?.LogDebug($"C: Switch into replay mode: {inReplayMode}");
            _inReplayMode = inReplayMode;
            _inDemoMode = inReplayMode;
        }

        /// <inheritdoc />
        public bool IsInDemoMode => _inDemoMode;

        /// <inheritdoc />
        public bool IsInReplayMode => _inReplayMode;

        /// <inheritdoc />
        public bool IsConnected => _board?.IsConnected ?? false;

        public void ShowMove(string allMoves, string startFenPosition, string promote, bool waitFor)
        {
            if (string.IsNullOrWhiteSpace(allMoves))
            {
                _fileLogger?.LogError("C: No moves given for ShowMove");
                return;
            }

         
            _fileLogger?.LogDebug($"C: Show Move for: {allMoves}");
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
                    _fileLogger?.LogError($"C: Invalid move {move}");
                    return;
                }

                promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                _internalChessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
            }
            var lastMove = moveList[moveList.Length - 1];
            promote = lastMove.Length == 4 ? string.Empty : lastMove.Substring(4, 1);
            _fileLogger?.LogDebug($"C: Show Move {lastMove}");
            var position = _internalChessBoard.GetPosition();
          
            if (waitFor && !_board.SelfControlled)
            {
                _fileLogger?.LogDebug($"C: Wait for: {position}");
                // Set LEDs on for received move and wait until board is in same position
                _waitForFen.Enqueue(position);
            }

            if (_board!= null && !_board.SelfControlled)
            {
                _board?.SetLedForFields(lastMove.Substring(0, 2), lastMove.Substring(2, 2), promote, false, true, string.Empty);
            }
            _stop = false;
        }

        public void ShowMove(string fromField, string toField, string promote, string displayString)
        {
            _internalChessBoard.MakeMove(fromField, toField, promote);
            var position = _internalChessBoard.GetPosition();
            _waitForFen.Enqueue(position);
            _board?.SetLedForFields(fromField, toField, promote, false, true, displayString);
            _stop = false;
        }

        public void SetLedsFor(string[] fields, bool thinking)
        {
            //_allLedOff =  _inDemoMode;
            _board?.SetLedForFields(fields,string.Empty, thinking, false, string.Empty);
        }

        public void SetAllLedsOff()
        {
            if (!_allLedOff)
            {
                _fileLogger?.LogDebug("Set all LEDs off");
                while (_waitForFen.Count > 0)
                {
                    _waitForFen.TryDequeue(out _);
                }
                _board?.SetAllLedsOff();
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
            _fileLogger?.LogDebug("C: New game");
            _board?.NewGame();
            _internalChessBoard = new InternalChessBoard();
            _internalChessBoard.NewGame();
            SetAllLedsOff();
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
                _fileLogger?.LogError("C: No fen position");
                return;
            }
            SetAllLedsOff();
            _fileLogger?.LogDebug($"C: set fen position: {fen}");
            _internalChessBoard = new InternalChessBoard();

            _internalChessBoard.NewGame();
            _internalChessBoard.SetPosition(fen);
            var dataFromBoard = _board?.GetPiecesFen();
            _fileLogger?.LogDebug($"C: current Fen: {dataFromBoard?.FromBoard}");
            _board?.SetFen(fen);
            var position = _internalChessBoard.GetPosition();

            if (string.IsNullOrWhiteSpace(allMoves))
            {
                _fileLogger?.LogDebug($"C: Wait for: {position}");
                // Set LEDs on for received fen and wait until board is in same position
                _waitForFen.Enqueue(position);
                _stop = false;
                return;
            }
            var moveList = allMoves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string promote = string.Empty;
            foreach (string move in moveList)
            {
                _fileLogger?.LogError($"C: move after fen: {move}");
                if (move.Length < 4)
                {
                    _fileLogger?.LogError($"C: Invalid move {move}");
                    return;
                }

                promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                _internalChessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
            }

            var lastMove = moveList[moveList.Length - 1];
            promote = lastMove.Length == 4 ? string.Empty : lastMove.Substring(4, 1);
            _fileLogger?.LogDebug($"C: Show Move {lastMove}");
            position = _internalChessBoard.GetPosition();
            _fileLogger?.LogDebug($"C: Wait for: {position}");
            // Set LEDs on for received move and wait until board is in same position
            _waitForFen.Enqueue(position);
            _board?.SetLedForFields(lastMove.Substring(0, 2), lastMove.Substring(2, 2),promote, false, true, string.Empty);
            _stop = false;
         
        }

        public void Close()
        {
            _stop = true;
            _allLedOff = false;
            SetAllLedsOff();
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
            _fileLogger?.LogDebug($"C: Play with white: {withWhite}");
            if (withWhite)
            {
                _board.PlayWithWhite();
                return;
            }
            _board.PlayWithBlack();
        }

        public bool PlayingWithWhite =>  _board?.PlayingWithWhite ?? true;


        public string GetBestMove()
        {
            return _internalChessBoard.GetBestMove();
        }

        public void SetCOMPort(string portName)
        {
            _fileLogger?.LogDebug($"C: Set COM-Port to: {portName}");
            _board.SetComPort(portName);
        }

        public bool CheckCOMPort(string portName)
        {
            _fileLogger?.LogDebug($"C: Set COM-Port to: {portName}");
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
            SetAllLedsOff();
            _handleBoardThread = new Thread(HandleBoard) { IsBackground = true };
            _handleBoardThread.Start();
            _internalChessBoard = new InternalChessBoard();
            _internalChessBoard.NewGame();
            _inDemoMode = false;
            _inReplayMode = false;
           
        }

        private void _board_BasePositionEvent(object sender, EventArgs e)
        {
            //if (!_forcedBasePosition)
            //{
            //    _fileLogger?.LogDebug("C: Forced base position received");
            //}
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
            _fileLogger?.LogDebug("C: Handle board");
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
                    _fileLogger?.LogDebug($"C: Wait for fen: {waitForFen}");
                }
                var piecesFen = GetPiecesFenAndSetLedsForInvalidField() ?? _board?.GetPiecesFen();
                if (piecesFen == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(piecesFen.FromBoard) || piecesFen.FromBoard.Equals(_board?.UnknownPieceCode))
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
                    _fileLogger?.LogDebug($"C: Fen {piecesFen.FromBoard} repeated {piecesFen.Repeated} for same position. OnMoveEvent for: {potentialMove}  {currentFen}");
                    potentialMove = string.Empty;
                    OnMoveEvent(currentFen);
                }
                // Something changed on board?
                if (!piecesFen.FromBoard.Equals(currentFen))
                {
                    if (changedFenHelper != $"{piecesFen.FromBoard}{currentFen}")
                    {
                        _fileLogger?.LogDebug($"C: Fen on board changed from {currentFen} to {piecesFen.FromBoard}");
                    }

                    changedFenHelper = $"{piecesFen.FromBoard}{currentFen}";
                }

                // Awaited position on board?
                if (!string.IsNullOrWhiteSpace(waitForFen) &&
                    !string.IsNullOrWhiteSpace(piecesFen.FromBoard) &&
                    waitForFen.StartsWith(piecesFen.FromBoard)) 
                {
                    _fileLogger?.LogDebug($"C: Awaited fen position on board received: {waitForFen}");
                    _board?.SetAllLedsOff();
                    currentFen = piecesFen.FromBoard;
                    waitForFen = string.Empty;
                    AwaitedPosition?.Invoke(this, null);
                }

                if (_forcedBasePosition || (piecesFen.Invalid && _piecesFenBasePosition))
                {
                    _forcedBasePosition = false;
                    BasePositionEvent?.Invoke(this, null);
                }

                if (!batteryLevel.Equals(_board?.BatteryLevel))
                {
                    batteryLevel = _board?.BatteryLevel;
                    BatteryChangedEvent?.Invoke(this, null);
                }
                if (!string.IsNullOrWhiteSpace(waitForFen))
                {
                    //_board?.SetAllLedsOff();
                    //_board?.SetLastLeds();
                
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
                    _fileLogger?.LogDebug($"C: Move detected: {move}");
                    
                    currentFen = piecesFen.FromBoard;
                    if (string.IsNullOrWhiteSpace(potentialMove))
                    {
                        _fileLogger?.LogDebug($"C: Set as potential move: {move}");
                        potentialMove = move;
                        continue;
                    }

                    if (potentialMove.Equals(move))
                    {
                        _fileLogger?.LogDebug($"C: OnMoveEvent: move equal to potential move: {move}");
                        potentialMove = string.Empty;
                        OnMoveEvent(piecesFen.FromBoard);
                   
                        continue;
                    }

                    if (potentialMove.StartsWith(move.Substring(0, 2)))
                    {
                        _fileLogger?.LogDebug($"C: Slow figure: new move {move} starts with potential move: {potentialMove}");
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
            _fileLogger?.LogDebug($"C: MoveEvent from board: {fenPosition}");
            // Calculate move from current position to new fen position
            var move = _internalChessBoard.GetMove(fenPosition, _inDemoMode);
            if (string.IsNullOrWhiteSpace(move))
            {
                _fileLogger?.LogDebug("C: Move is invalid");
                return;
            }
            _fileLogger?.LogDebug($"C: Move detected: {move}");
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
            _lastFenColor = c;
            _fileLogger?.LogDebug($"C: FenEvent from board: {fenPosition} {changedFigure}");
            FenEvent?.Invoke(this, fenPosition+" "+c);
        }

        private DataFromBoard GetPiecesFenAndSetLedsForInvalidField()
        {
            // Every position is possible
            if (_inDemoMode || _stop )
            {
                if (!_allLedOff)
                {
                    _board?.SetAllLedsOff();
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

                for (var f = InternalChessBoard.FA1; f <= InternalChessBoard.FH8; f++)
                {
                    if (!_internalChessBoard.GetFigureOnField(f).Equals(internalChessBoard.GetFigureOnField(f)))
                    {
                        //_fileLogger?.LogDebug($"C: Not equal on {f}: {_internalChessBoard.GetFigureOnField(f)} vs. {internalChessBoard.GetFigureOnField(f)}");
                        invalidFields.Add(InternalChessBoard.GetFieldName(f));
                    }
                }
            }

            if (invalidFields.Count > 0)
            {
             
                fenPosition.Invalid = true;
                _board?.SetLedForFields(invalidFields.ToArray(), string.Empty,false, false, string.Empty);
                _allLedOff = false;
            }
            else
            {
                if (_waitForFen.IsEmpty && !_stop )
                {
                    if (!_allLedOff && !fenPosition.IsFieldDump)
                    {
                        _fileLogger?.LogDebug("C: Waiting for fen is empty: Set all LEDs off");
                        _board?.SetAllLedsOff();
                        _allLedOff = true;
                    }
                }
            }

            return fenPosition;
        }

        #endregion
    }
}
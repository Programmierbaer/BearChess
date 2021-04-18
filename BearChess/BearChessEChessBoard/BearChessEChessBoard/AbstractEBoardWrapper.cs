using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public abstract class AbstractEBoardWrapper : IEBoardWrapper
    {

        protected bool _isFirstInstance;
        
        private bool _stopCommunication = false;
        private bool _allLedOff = true;

        public string Name { get; }
        protected IInternalChessBoard _internalChessBoard;
        protected readonly ILogging _fileLogger;
        protected IEBoard _board;
        protected bool _stop = false;
        protected readonly ConcurrentQueue<string> _waitForFen = new ConcurrentQueue<string>();
        protected Thread _handleBoardThread;
        private bool _inDemoMode = false;
        protected string _basePath;
        protected string _comPortName;
        protected bool _useBluetooth;
        private bool _piecesFenBasePosition;

        public event EventHandler<string> MoveEvent;
        public event EventHandler<string> FenEvent;
        public event EventHandler BasePositionEvent;
        public event EventHandler AwaitedPosition;

        protected AbstractEBoardWrapper(string name, string basePath)
        {
            Name = name;
            _basePath = basePath;
            _isFirstInstance = true;
            var number = _isFirstInstance ? 1 : 2;
            try
            {
                _fileLogger = new FileLogger(Path.Combine(basePath, "log", $"{Name}_{number}.log"), 10, 10);
            }
            catch
            {
                _fileLogger = null;
            }

            // ReSharper disable once VirtualMemberCallInConstructor
            _board = GetEBoard(true);
        }

        protected AbstractEBoardWrapper(string name, string basePath, bool isFirstInstance, string comPortName):this(name, basePath,isFirstInstance,comPortName, false)
        {

        }

        protected AbstractEBoardWrapper(string name, string basePath, bool isFirstInstance, string comPortName,
                                        bool useBluetooth)
        {
            Name = name;
            _basePath = basePath;
            _isFirstInstance = isFirstInstance;
            _comPortName = comPortName;
            _useBluetooth = useBluetooth;
            var number = _isFirstInstance ? 1 : 2;
            try
            {
                _fileLogger = new FileLogger(Path.Combine(basePath, "log", $"{Name}_{number}.log"), 10, 10);
            }
            catch
            {
                _fileLogger = null;
            }

            Init();
        }

        public abstract void Calibrate();

        public string GetCurrentCOMPort()
        {
            return _board?.GetCurrentCOMPort();
        }

        public bool IsOnBasePosition()
        {
            return _piecesFenBasePosition;
        }

        public abstract void DimLeds(bool dimLeds);
        public abstract void FlashInSync(bool flashSync);

        public void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            _board?.SetLedCorner(upperLeft,upperRight,lowerLeft,lowerRight);
        }

        protected abstract IEBoard GetEBoard();
        protected abstract IEBoard GetEBoard(bool check);

        public void SetDemoMode(bool inDemoMode)
        {
            _fileLogger?.LogDebug($"C: Switch into demo mode: {inDemoMode}");
            _inDemoMode = inDemoMode;
        }

        /// <inheritdoc />
        public bool IsInDemoMode => _inDemoMode;

        /// <inheritdoc />
        public bool IsConnected => _board?.IsConnected ?? false;

        public void ShowMove(string allMoves, bool waitFor)
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
            foreach (string move in moveList)
            {
                if (move.Length < 4)
                {
                    _fileLogger?.LogError($"C: Invalid move {move}");
                    return;
                }

                var promote = move.Length == 4 ? string.Empty : move.Substring(4, 1);
                _internalChessBoard.MakeMove(move.Substring(0, 2), move.Substring(2, 2), promote);
            }
            var lastMove = moveList[moveList.Length - 1];
            _fileLogger?.LogDebug($"C: Show Move {lastMove}");
            var position = _internalChessBoard.GetPosition();
            _fileLogger?.LogDebug($"C: Wait for: {position}");
            if (waitFor)
            {
                // Set LEDs on for received move and wait until board is in same position
                _waitForFen.Enqueue(position);
            }
            _board?.SetLedForFields(lastMove.Substring(0, 2), lastMove.Substring(2, 2));
            _stop = false;
        }

        public void ShowMove(string fromField, string toField)
        {
            _internalChessBoard.MakeMove(fromField, toField, string.Empty);
            var position = _internalChessBoard.GetPosition();
            _waitForFen.Enqueue(position);
            _board?.SetLedForFields(fromField,toField);
            _stop = false;
        }

        public void SetLedsFor(string[] fields)
        {
            _allLedOff =  _inDemoMode;
            _board?.SetLedForFields(fields);
        }

        public void SetAllLedsOff()
        {
            if (!_allLedOff)
            {
                _fileLogger.LogDebug("Set all LEDs off");
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
            _fileLogger?.LogDebug($"C: Show Move {lastMove}");
            position = _internalChessBoard.GetPosition();
            _fileLogger?.LogDebug($"C: Wait for: {position}");
            // Set LEDs on for received move and wait until board is in same position
            _waitForFen.Enqueue(position);
            _board?.SetLedForFields(lastMove.Substring(0, 2), lastMove.Substring(2, 2));
            _stop = false;
         
        }

        public void Close()
        {
            _stop = true;
            _allLedOff = false;
            SetAllLedsOff();
            _stopCommunication = true;
            _board?.Dispose();
            _board = null;
        }


        public void Stop()
        {
            _stop = true;
        }

        public void Continue()
        {
            _stop = false;
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
           
        }

        private void HandleBoard()
        {
            string waitForFen = string.Empty;
            string currentFen = string.Empty;
            string prevFen = string.Empty;
            string potentialMove = string.Empty;
            string changedFenHelper = string.Empty;
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
                    _fileLogger?.LogDebug($"C:  Fen {piecesFen.FromBoard} repeated {piecesFen.Repeated} for same position. OnMoveEvent for: {potentialMove}  {currentFen}");
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

                if (piecesFen.Invalid && _piecesFenBasePosition)
                {
                    BasePositionEvent?.Invoke(this, null);
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
                _fileLogger?.LogDebug($"C: Move is invalid");
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
            //_board?.SetAllLedsOff();
            var fenPosition = _board?.GetPiecesFen();
            if (fenPosition == null)
            {
                return null;
            }
            if (!string.IsNullOrWhiteSpace(fenPosition.FromBoard) && fenPosition.FromBoard.Equals(_board?.UnknownPieceCode))
            {
                return fenPosition;
            }
            var invalidFields = new List<string>();
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

            if (invalidFields.Count > 0)
            {
                //if (_internalChessBoard.IsBasePosition(fenPosition))
                //{
                //    _fileLogger?.LogDebug($"C: Base position detected. Reset to new game");
                //    NewGame();
                //    return;
                //}

                fenPosition.Invalid = true;
                _fileLogger?.LogDebug($"C: {fenPosition.Repeated} times Invalid fields for fen: {fenPosition.FromBoard}");
                _fileLogger?.LogDebug($"C: Invalid fields: {string.Join(" ", invalidFields)}");
                _fileLogger?.LogDebug($"C: Internal board: {_internalChessBoard.GetPosition()} vs. {fenPosition.FromBoard}");
                _board?.SetLedForFields(invalidFields.ToArray());
                _allLedOff = false;
            }
            else
            {
                if (_waitForFen.IsEmpty && !_stop)
                {
                    if (!_allLedOff)
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
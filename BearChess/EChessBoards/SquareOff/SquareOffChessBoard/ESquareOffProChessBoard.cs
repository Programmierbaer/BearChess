using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;


namespace www.SoLaNoSoft.com.BearChess.SquareOffChessBoard
{
    public class ESquareOffProChessBoard : AbstractEBoard
    {
        private class SOProFigure 
        {
            public int Figure
            {
                get; set;
            }
         
            public int Field
            {
                get; set;
            }
            
            public int FigureColor
            {
                get; set;
            }

            public string FieldName => Fields.GetFieldName(Field);

            public SOProFigure()
            {
                Figure = FigureId.NO_PIECE;
                Field = Fields.COLOR_EMPTY;
                FigureColor = FigureId.OUTSIDE_PIECE;
            }
        }

        private const string BASEPOSITION = "1100001111000011110000111100001111000011110000111100001111000011";

        private const string REQUEST_DUMP = "R*";
        private const string FIELD_CMD_PREFIX = "30#";
        private const string LED_CMD_PREFIX = "25#";
        private const string BATTERY_INFO_PREFIX = "4#";
        private readonly EChessBoardConfiguration _configuration;
        private readonly IChessBoard _chessBoard;
        private readonly IChessBoard _workingChessBoard;
        private readonly ConcurrentQueue<DataFromBoard> _fromBoard = new ConcurrentQueue<DataFromBoard>();
        private bool _release = false;
        private SOProFigure _liftUpFigure = null;
        private SOProFigure _liftUpEnemyFigure = null;
        private int _lastFromField = Fields.COLOR_OUTSIDE;
        private int _lastToField = Fields.COLOR_OUTSIDE;
        private string _prevRead = string.Empty;
        private string _prevSend = string.Empty;
        private int _equalReadCount = 0;
        private volatile int _delay = 50;

        public override event EventHandler BasePositionEvent;
        public override event EventHandler NewGamePositionEvent;
        public override event EventHandler HelpRequestedEvent;
        public override event EventHandler<string> DataEvent;
      
        private string _startFenPosition = string.Empty;
        private volatile int _dumpLoopWait;
        private bool _stopLoop;
        private DataFromBoard _dumpDataFromBoard = new DataFromBoard(string.Empty);
        private ConcurrentBag<string> _ledBag = new ConcurrentBag<string>();
        private volatile bool _dumpRequested;

        private readonly Dictionary<byte, string> _fieldByte2FieldName = new Dictionary<byte, string>()
        { { 0, "A1" }, { 1, "A2" }, { 2, "A3" }, { 3, "A4" }, { 4, "A5" }, { 5, "A6" }, { 6, "A7" }, { 7, "A8" },
            { 8, "B1" }, { 9, "B2" }, {10, "B3" }, {11, "B4" }, {12, "B5" }, {13, "B6" }, {14, "B7" }, {15, "B8" },
            {16, "C1" }, {17, "C2" }, {18, "C3" }, {19, "C4" }, {20, "C5" }, {21, "C6" }, {22, "C7" }, {23, "C8" },
            {24, "D1" }, {25, "D2" }, {26, "D3" }, {27, "D4" }, {28, "D5" }, {29, "D6" }, {30, "D7" }, {31, "D8" },
            {32, "E1" }, {33, "E2" }, {34, "E3" }, {35, "E4" }, {36, "E5" }, {37, "E6" }, {38, "E7" }, {39, "E8" },
            {40, "F1" }, {41, "F2" }, {42, "F3" }, {43, "F4" }, {44, "F5" }, {45, "F6" }, {46, "F7" }, {47, "F8" },
            {48, "G1" }, {49, "G2" }, {50, "G3" }, {51, "G4" }, {52, "G5" }, {53, "G6" }, {54, "G7" }, {55, "G8" },
            {56, "H1" }, {57, "H2" }, {58, "H3" }, {59, "H4" }, {60, "H5" }, {61, "H6" }, {62, "H7" }, {63, "H8" }
        };


        public ESquareOffProChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _configuration = configuration;
            _serialCommunication = new SerialCommunication(logger, _configuration.PortName);
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            ValidForAnalyse = false;
            MultiColorLEDs = true;
            _dumpLoopWait = 250;
            IsConnected = EnsureConnection();
            Information = Constants.SquareOffPro;
            _chessBoard = new BearChessBase.Implementations.ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _workingChessBoard = new BearChessBase.Implementations.ChessBoard();
            _workingChessBoard.Init();
            _workingChessBoard.NewGame();
            var handleFromBoardThread = new Thread(HandleFomBoard) { IsBackground = true };
            handleFromBoardThread.Start();
            var requestDumpThread = new Thread(RequestADumpLoop) { IsBackground = true };
            requestDumpThread.Start();
            var handleLedThread = new Thread(LedThreadHandle) { IsBackground = true };
            handleLedThread.Start();
        }
        public ESquareOffProChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "";
            Information = Constants.SquareOffPro;
            PieceRecognition = false;
            ValidForAnalyse = false;
        }

        private void LedThreadHandle()
        {
            while (!_release)
            {
                lock (_locker)
                {
                    if (_ledBag.Count > 0)
                    {
                        var fieldNames = string.Empty;
                        while (_ledBag.TryTake(out var fieldName))
                        {
                            fieldNames += fieldName;
                        }
                        _serialCommunication.Send($"{LED_CMD_PREFIX}{fieldNames}*");
                    }

                }
                Thread.Sleep(_delay);
            }
            while (_ledBag.TryTake(out _)) { }
        }


        private void RequestADumpLoop()
        {
            while (!_stopAll)
            {
                Thread.Sleep( _dumpLoopWait);
                if (!_stopLoop)
                {
                    _serialCommunication.Send($"{FIELD_CMD_PREFIX}{REQUEST_DUMP}");
                }
            }
        }
      
        public override void Reset()
        {
            //
        }

        protected override void SetToNewGame()
        {
            lock (_locker)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                _workingChessBoard.Init();
                _workingChessBoard.NewGame();
                _startFenPosition = string.Empty;
                _lastFromField = Fields.COLOR_OUTSIDE;
                _lastToField = Fields.COLOR_OUTSIDE;
                _liftUpFigure = null;
                _liftUpEnemyFigure = null;

                while (_ledBag.TryTake(out _)) { }

                _serialCommunication.Send("14#1*");
                _serialCommunication.Send("4#*");
            }
        }

        public override void Release()
        {
            _release = true;
        }

        public override void SetFen(string fen)
        {
            lock (_locker)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                _chessBoard.SetPosition(fen, true);
                _workingChessBoard.Init();
                _workingChessBoard.NewGame();
                _workingChessBoard.SetPosition(fen, true);
                _startFenPosition = _workingChessBoard.GetFenPosition();
                _lastFromField = Fields.COLOR_OUTSIDE;
                _lastToField = Fields.COLOR_OUTSIDE;
                while (_ledBag.TryTake(out _)) { }
            }
        }
      
        public override void SpeedLeds(int level)
        {
            //
        }

        public override void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
        {
            //
        }

        public override void StopClock()
        {
            //
        }

        public override void StartClock(bool white)
        {
            //
        }

        public override void DisplayOnClock(string display)
        {
            //
        }

        public override void SetCurrentColor(int currentColor)
        {
            //
        }

        public override void SetEngineColor(int color)
        {
            //
        }

        public override bool CheckComPort(string portName)
        {
            return true;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return true;
        }

        public override void BlackWins()
        {
            _serialCommunication.Send("27#bl*");
        }
     
        public override void WhiteWins()
        {
            _serialCommunication.Send("27#wt*");
        }

        public override void IsADraw()
        {
            _serialCommunication.Send("27#dw*");
        }

        public override void SetLedForFields(SetLEDsParameter ledsParameter)
        {

            if (!EnsureConnection())
            {
                return;
            }
            if (ledsParameter.FieldNames.Length == 0 && ledsParameter.HintFieldNames.Length == 0 &&
               ledsParameter.InvalidFieldNames.Length == 0)
            {
                return;
            }

            lock (_locker)
            {
                while (_ledBag.TryTake(out _)){}
            }
            _serialCommunication.ClearToBoard();
            lock (_locker)
            {
                if (ledsParameter.IsProbing)
                {
                    if (_configuration.ShowPossibleMovesEval)
                    {
                        var firstOrDefault =
                            ledsParameter.ProbingMoves.OrderByDescending(p => p.Score).FirstOrDefault();
                        if (firstOrDefault != null)
                        {
                            _ledBag.Add(firstOrDefault.FieldName.ToLower());
                        }
                    }
                    else
                    {
                        foreach (var ledsParameterProbingMove in ledsParameter.ProbingMoves)
                        {
                            _ledBag.Add(ledsParameterProbingMove.FieldName.ToLower());
                        }
                    }
                }
                else
                {
                    if (ledsParameter.FieldNames.Length > 0)
                    {
                        if (ledsParameter.FieldNames.Length == 2 && _configuration.ShowMoveLine)
                        {
                            var moveLine = MoveLineHelper.GetMoveLine(ledsParameter.FieldNames[0], ledsParameter.FieldNames[1]);
                            foreach (var fieldName in moveLine)
                            {
                                _ledBag.Add(fieldName.ToLower());
                            }
                        }
                        else
                        {
                            foreach (var parameterFieldName in ledsParameter.FieldNames)
                            {
                                _ledBag.Add(parameterFieldName.ToLower());
                            }
                        }
                    }
                    else
                    {
                        foreach (var parameterFieldName in ledsParameter.InvalidFieldNames)
                        {
                            _ledBag.Add(parameterFieldName.ToLower());
                        }

                        foreach (var parameterFieldName in ledsParameter.HintFieldNames)
                        {
                            _ledBag.Add(parameterFieldName.ToLower());
                        }
                    }
                }
            }
        }
       
        public override void SetAllLedsOff(bool forceOff)
        {
            lock (_locker)
            {
                while (_ledBag.TryTake(out _)) { }
            }
            _serialCommunication.Send($"{LED_CMD_PREFIX}*");
        }

        public override void SetAllLedsOn()
        {
           // _serialCommunication.Send($"{LED_CMD_PREFIX}e2e4d2d4*");
        }

        public override void DimLeds(bool dimLeds)
        {
            // ignore
        }

        public override void DimLeds(int level)
        {
            // ignore
        }

        public override void SetScanTime(int scanTime)
        {
            _dumpLoopWait = scanTime > 0 ? scanTime : 250;
        }

        public override void SetDebounce(int debounce)
        {
            if (debounce >= 10)
            {
                _delay = debounce;
            }
        }
        public override void FlashMode(EnumFlashMode flashMode)
        {
            // ignore
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            // ignore
        }

        public override void Calibrate()
        {
            // ignore
        }

        public override void SendInformation(string message)
        {
            // ignore
        }

        public override void AdditionalInformation(string information)
        {
            if (information.StartsWith("stop"))
            {
                _stopLoop = true;
                return;
            }
            if (information.StartsWith("go"))
            {
                _stopLoop = false;
                return;
            }

            if (!information.StartsWith("SCORE"))
            {
                _serialCommunication.Send(information);
            }
        }

        public override void RequestDump()
        {
            _dumpRequested = true;
        }

        public override DataFromBoard GetPiecesFen()
        {
            if (!_fromBoard.TryDequeue(out var dataFromBoard))
            {
                return new DataFromBoard(_prevSend, 3);
            }

            if (dataFromBoard.FromBoard.StartsWith(FIELD_CMD_PREFIX))
            {
                var fieldNames = dataFromBoard.FromBoard.Substring(3, dataFromBoard.FromBoard.Length - 3).Replace("*", string.Empty);
                    
                if (_dumpRequested)
                {
                    var bc = new BoardCodeConverter(fieldNames);
                    _dumpDataFromBoard = new DataFromBoard(string.Join(",", bc.GetFieldsWithPieces()), 3)
                    {
                        IsFieldDump = true,
                        BasePosition =  fieldNames.StartsWith(BASEPOSITION)
                    };
                }
                if (_prevRead.Equals(dataFromBoard.FromBoard))
                {
                    _equalReadCount++;
                }
                else
                {
                    _logger?.LogDebug($"GetPiecesFen: Changes from {_prevRead} to {dataFromBoard.FromBoard}");
                    _prevRead = dataFromBoard.FromBoard;
                    _equalReadCount = 0;
                }

                if (_equalReadCount < 5)
                {
                    if (string.IsNullOrWhiteSpace(_prevSend))
                    {
                        _prevSend = _workingChessBoard.GetFenPosition()
                            .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    }

                    _logger?.LogDebug($"GetPiecesFen: _equalReadCount {_equalReadCount} return {_prevSend}");
                    return new DataFromBoard(_prevSend, 3);
                }

                // _logger?.LogDebug($"GetPiecesFen: fromBoard: {dataFromBoard.FromBoard}");
                if (fieldNames.StartsWith(BASEPOSITION))
                {
                    _chessBoard.Init();
                    _chessBoard.NewGame();
                    _workingChessBoard.Init();
                    _workingChessBoard.NewGame();
                    _startFenPosition = string.Empty;
                    BasePositionEvent?.Invoke(this, null);
                    var basePos = _workingChessBoard.GetFenPosition()
                        .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];                       
                    _prevSend = basePos;
                    return new DataFromBoard(_prevSend, 3)
                    {
                        BasePosition = true,
                        Invalid = false,
                        IsFieldDump = false
                    };
                }

                var boardCodeConverter = new BoardCodeConverter(fieldNames);
                // _logger?.LogDebug($"Field names: {string.Join(" ",boardCodeConverter.GetFieldsWithPieces())}");
                IChessFigure liftUpFigure = null;
                var liftDownField = Fields.COLOR_OUTSIDE;
                foreach (var boardField in Fields.BoardFields)
                {
                    var isFigureOnBoard = boardCodeConverter.IsFigureOn(boardField);
                    var chessFigure = _workingChessBoard.GetFigureOn(boardField);
                    if (isFigureOnBoard && chessFigure.Color == Fields.COLOR_EMPTY)
                    {
                            
                        _logger?.LogDebug($"GetPiecesFen: Downfield: {boardField}/{Fields.GetFieldName(boardField)}");
                        liftDownField = boardField;
                    }

                    if (!isFigureOnBoard && chessFigure.Color != Fields.COLOR_EMPTY)
                    {
                        _logger?.LogDebug(
                            $"GetPiecesFen: Lift up field: {boardField}/{Fields.GetFieldName(boardField)} {FigureId.FigureIdToEnName[chessFigure.FigureId]}");

                        liftUpFigure = chessFigure;

                    }
                }

                // Nothing changed?
                if (liftDownField == Fields.COLOR_OUTSIDE && liftUpFigure == null)
                {
                    return new DataFromBoard(_prevSend, 3);
                }

                var codeConverter = new BoardCodeConverter();
                var chessFigures = _chessBoard.GetFigures(Fields.COLOR_WHITE);
                foreach (var chessFigure in chessFigures)
                {
                    codeConverter.SetFigureOn(chessFigure.Field);
                }

                chessFigures = _chessBoard.GetFigures(Fields.COLOR_BLACK);
                foreach (var chessFigure in chessFigures)
                {
                    codeConverter.SetFigureOn(chessFigure.Field);
                }

                if (boardCodeConverter.SamePosition(codeConverter))
                {
                    _workingChessBoard.Init(_chessBoard);
                    _prevSend = _chessBoard.GetFenPosition()
                        .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    _logger?.LogDebug($"GetPiecesFen: back to current position: {_prevSend}");
                    _liftUpFigure = null;
                    _liftUpEnemyFigure = null;
                    _lastFromField = Fields.COLOR_OUTSIDE;
                    _lastToField = Fields.COLOR_OUTSIDE;
                    return new DataFromBoard(_prevSend, 3);
                }

                if (!_inDemoMode && _liftUpEnemyFigure != null)
                {
                    if (liftDownField == _liftUpEnemyFigure.Field && (_liftUpFigure == null && liftUpFigure==null))
                    {
                        _logger?.LogDebug(
                            $"GetPiecesFen: Equal lift up/down enemy field: {_liftUpEnemyFigure.Field} == {liftDownField}");
                        _workingChessBoard.Init(_chessBoard);
                        _liftUpFigure = null;
                        _liftUpEnemyFigure = null;
                        _prevSend = _chessBoard.GetFenPosition()
                            .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                        _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");

                        return new DataFromBoard(_prevSend, 3);

                    }
                }

                if (_liftUpEnemyFigure != null &&
                    (!_inDemoMode && _allowTakeBack && liftDownField.Equals(_lastFromField) &&
                     _liftUpEnemyFigure.Field.Equals(_lastToField)))
                {
                    _logger?.LogInfo("TC: Take move back. Replay all previous moves");
                    var playedMoveList = _chessBoard.GetPlayedMoveList();
                    _chessBoard.Init();
                    _chessBoard.NewGame();
                    if (!string.IsNullOrWhiteSpace(_startFenPosition))
                    {
                        _chessBoard.SetPosition(_startFenPosition, false);
                    }

                    for (var i = 0; i < playedMoveList.Length - 1; i++)
                    {
                        _logger?.LogDebug($"TC: Move {playedMoveList[i]}");
                        _chessBoard.MakeMove(playedMoveList[i]);
                        _lastFromField = playedMoveList[i].FromField;
                        _lastToField = playedMoveList[i].ToField;
                    }

                    _workingChessBoard.Init(_chessBoard);
                    _prevSend = _chessBoard.GetFenPosition()
                        .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");
                    _liftUpFigure = null;
                    _liftUpEnemyFigure = null;
                    return new DataFromBoard(_prevSend, 3);
                }

                if (_liftUpFigure != null)
                {
                    // Put the same figure back
                    if (liftDownField == _liftUpFigure.Field)
                    {
                        _logger?.LogDebug(
                            $"GetPiecesFen: Equal lift up/down field: {_liftUpFigure.Field} == {liftDownField}");
                        _workingChessBoard.Init(_chessBoard);
                        _liftUpFigure = null;
                        _liftUpEnemyFigure = null;
                        _prevSend = _chessBoard.GetFenPosition()
                            .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                        _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");

                        return new DataFromBoard(_prevSend, 3);
                    }

                    if (liftDownField != Fields.COLOR_OUTSIDE)
                    {
                        if (!_inDemoMode)
                        {
                            if (!_inDemoMode && _allowTakeBack && liftDownField.Equals(_lastFromField) &&
                                _liftUpFigure.Field.Equals(_lastToField))
                            {

                                _logger?.LogInfo("TC: Take move back. Replay all previous moves");
                                var playedMoveList = _chessBoard.GetPlayedMoveList();
                                _chessBoard.Init();
                                _chessBoard.NewGame();
                                if (!string.IsNullOrWhiteSpace(_startFenPosition))
                                {
                                    _chessBoard.SetPosition(_startFenPosition, false);
                                }

                                for (var i = 0; i < playedMoveList.Length - 1; i++)
                                {
                                    _logger?.LogDebug($"TC: Move {playedMoveList[i]}");
                                    _chessBoard.MakeMove(playedMoveList[i]);
                                    _lastFromField = playedMoveList[i].FromField;
                                    _lastToField = playedMoveList[i].ToField;
                                }

                                _workingChessBoard.Init(_chessBoard);
                                _prevSend = _chessBoard.GetFenPosition()
                                    .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                                _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");
                                _liftUpFigure = null;
                                _liftUpEnemyFigure = null;

                                return new DataFromBoard(_prevSend, 3);
                            }

                            if (_chessBoard.MoveIsValid(_liftUpFigure.Field, liftDownField))
                            {
                                if (_awaitingMoveFromField != Fields.COLOR_OUTSIDE)
                                {
                                    if (_awaitingMoveFromField != _liftUpFigure.Field ||
                                        _awaitingMoveToField != liftDownField)
                                    {
                                        _logger?.LogDebug($"GetPiecesFen: move not awaited!");
                                        _logger?.LogDebug(
                                            $"GetPiecesFen: Awaited: {Fields.GetFieldName(_awaitingMoveFromField)} {Fields.GetFieldName(_awaitingMoveToField)}");
                                        _logger?.LogDebug(
                                            $"GetPiecesFen: Read: {Fields.GetFieldName(_liftUpFigure.Field)} {Fields.GetFieldName(liftDownField)}");
                                        return new DataFromBoard(_prevSend, 3);
                                    }
                                }

                                _logger?.LogDebug(
                                    $"GetPiecesFen: Make move: {Fields.GetFieldName(_liftUpFigure.Field)} {Fields.GetFieldName(liftDownField)}");
                                _lastFromField = _liftUpFigure.Field;
                                _lastToField = liftDownField;

                                _chessBoard.MakeMove(_liftUpFigure.Field, liftDownField);
                                _awaitingMoveFromField = Fields.COLOR_OUTSIDE;
                                _awaitingMoveToField = Fields.COLOR_OUTSIDE;

                                _workingChessBoard.Init(_chessBoard);
                                _liftUpFigure = null;
                                _liftUpEnemyFigure = null;
                                _prevSend = _chessBoard.GetFenPosition()
                                    .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                                _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");

                                return new DataFromBoard(_prevSend, 3);
                            }

                            _lastFromField = _liftUpFigure.Field;
                            _lastToField = liftDownField;
                            //_logger?.LogDebug($"1. Remove working chessboard from {_liftUpFigure.Field} and set to {liftDownField}");   
                            _workingChessBoard.RemoveFigureFromField(_liftUpFigure.Field);
                            _workingChessBoard.SetFigureOnPosition(_liftUpFigure.Figure, liftDownField);
                            _workingChessBoard.CurrentColor = _liftUpFigure.FigureColor == Fields.COLOR_WHITE
                                ? Fields.COLOR_BLACK
                                : Fields.COLOR_WHITE;

                            _liftUpFigure = null;
                            _liftUpEnemyFigure = null;
                            _prevSend = _workingChessBoard.GetFenPosition()
                                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                            _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");
                            return new DataFromBoard(_prevSend, 3);
                        }

                        _lastFromField = _liftUpFigure.Field;
                        _lastToField = liftDownField;
                        // _logger?.LogDebug($"2. Remove working chessboard from {_liftUpFigure.Field} and set to {liftDownField}");
                        _chessBoard.RemoveFigureFromField(_liftUpFigure.Field);
                        _chessBoard.SetFigureOnPosition(_liftUpFigure.Figure, liftDownField);
                        _chessBoard.CurrentColor = _liftUpFigure.FigureColor == Fields.COLOR_WHITE
                            ? Fields.COLOR_BLACK
                            : Fields.COLOR_WHITE;
                        _workingChessBoard.Init(_chessBoard);
                        _liftUpFigure = null;
                        _liftUpEnemyFigure = null;
                        _prevSend = _chessBoard.GetFenPosition()
                            .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                        _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");
                        return new DataFromBoard(_prevSend, 3);
                    }
                }

                if (liftUpFigure != null)
                {
                    // _logger?.LogDebug($"3. Remove working chessboard from {liftUpFigure.Field} ");
                    _workingChessBoard.RemoveFigureFromField(liftUpFigure.Field);
                    if (_inDemoMode || liftUpFigure.Color == _chessBoard.CurrentColor)
                    {
                        if (_liftUpFigure == null || _liftUpFigure.Field != liftUpFigure.Field)
                        {
                            _liftUpFigure = new SOProFigure()
                            {
                                Field = liftUpFigure.Field, 
                                Figure = liftUpFigure.FigureId,
                                FigureColor = liftUpFigure.Color
                            };
                            _logger?.LogDebug(
                                $"GetPiecesFen: new _liftUpFigure {FigureId.FigureIdToEnName[_liftUpFigure.Figure]}");
                        }
                    }
                    else
                    {
                        _liftUpEnemyFigure = new SOProFigure
                        {
                            Field = liftUpFigure.Field, 
                            Figure = liftUpFigure.FigureId,
                            FigureColor = liftUpFigure.Color
                        };
                        _logger?.LogDebug(
                            $"GetPiecesFen: new _liftUpEnemyFigure {FigureId.FigureIdToEnName[_liftUpEnemyFigure.Figure]}");
                    }
                }
            }

            var newSend = _workingChessBoard.GetFenPosition()
                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            if (!newSend.Equals((_prevSend)))
            {
                _logger?.LogDebug($"GetPiecesFen: return changes from {_prevSend} to {newSend}");
            }

            _prevSend = newSend;

            return new DataFromBoard(_prevSend, 3);
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            try
            {
                var newSend = _workingChessBoard.GetFenPosition()
                    .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                return new DataFromBoard(newSend, 3);
            }
            catch
            {
                return new DataFromBoard(_prevSend, 3);
            }
            return _dumpDataFromBoard;
            
        }

        private void HandleFomBoard()
        {
            while (!_release)
            {
                Thread.Sleep(10);
                var dataFromBoard = _serialCommunication.GetFromBoard();
                _fromBoard.Enqueue(dataFromBoard);
            }

            while (_fromBoard.TryDequeue(out _))
            {
            }
        }            
    }
}

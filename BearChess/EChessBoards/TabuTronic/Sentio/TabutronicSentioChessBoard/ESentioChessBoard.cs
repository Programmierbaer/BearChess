using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.ChessBoard
{
    public class ESentioChessBoard : AbstractEBoard
    {

        private class SentioFigure
        {
            public SentioFigure()
            {
                Figure = FigureId.NO_PIECE;
                Field = Fields.COLOR_EMPTY;
                FigureColor = FigureId.OUTSIDE_PIECE;
            }
            public int Figure { get; set; }
            public int Field { get; set; }
            public int FigureColor { get; set; }

            public string FieldName => Fields.GetFieldName(Field);
        }

        private const string BASE_POSITION = "255 255 0 0 0 0 255 255";

        private readonly Dictionary<string, byte> _colName2ColByte = new Dictionary<string, byte>()
        {
            {"A", ColA},
            {"B", ColB},
            {"C", ColC},
            {"D", ColD},
            {"E", ColE},
            {"F", ColF},
            {"G", ColG},
            {"H", ColH},
        };

        private readonly Dictionary<string, byte> _flippedColName2ColByte = new Dictionary<string, byte>()
        {
            {"H", ColA},
            {"G", ColB},
            {"F", ColC},
            {"E", ColD},
            {"D", ColE},
            {"C", ColF},
            {"B", ColG},
            {"A", ColH},
        };

        private static byte ColA = 0x1;
        private static byte ColB = 0x1 << 1;
        private static byte ColC = 0x1 << 2;
        private static byte ColD = 0x1 << 3;
        private static byte ColE = 0x1 << 4;
        private static byte ColF = 0x1 << 5;
        private static byte ColG = 0x1 << 6;
        private static byte ColH = 0x1 << 7;
        private static byte[] AllOff = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private static byte[] AllOn = { 255, 255, 255, 255, 255, 255, 255, 255 };

        private readonly EChessBoardConfiguration _configuration;
        private readonly IChessBoard _chessBoard;
        private readonly IChessBoard _workingChessBoard;
        private string _prevJoinedString = string.Empty;
        private readonly ConcurrentQueue<DataFromBoard> _fromBoard = new ConcurrentQueue<DataFromBoard>();
        private bool _release = false;
        private SentioFigure _liftUpFigure = null;
        private SentioFigure _liftUpEnemyFigure = null;
        private bool _flashLeds;
        private int _lastFromField = Fields.COLOR_OUTSIDE;
        private int _lastToField = Fields.COLOR_OUTSIDE;
        private string _prevRead = string.Empty;
        private string _prevSend = string.Empty;
        private int _equalReadCount = 0;

        public override event EventHandler BasePositionEvent;
        public override event EventHandler NewGamePositionEvent;
        public override event EventHandler HelpRequestedEvent;
        public override event EventHandler<string> DataEvent;
        private readonly object _lockThinking = new object();
        private string _lastThinking0 = string.Empty;
        private string _lastThinking1 = string.Empty;
        private readonly ConcurrentQueue<ProbingMove[]> _probingFields = new ConcurrentQueue<ProbingMove[]>();
        private string _startFenPosition = string.Empty;
        private readonly bool _useBluetooth;
        private ResourceManager _rm;

        public ESentioChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _configuration = configuration;
            _useBluetooth = configuration.UseBluetooth; ;
            _serialCommunication = new SerialCommunication(logger, _configuration.PortName, _configuration.UseBluetooth);
            _rm = SpeechTranslator.ResourceManager;
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = _rm.GetString("Full");
            PieceRecognition = false;
            ValidForAnalyse = false;
            MultiColorLEDs = true;
            IsConnected = EnsureConnection();
            Information = Constants.TabutronicSentio;
            _chessBoard = new BearChessBase.Implementations.ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _workingChessBoard = new BearChessBase.Implementations.ChessBoard();
            _workingChessBoard.Init();
            _workingChessBoard.NewGame();
            var handleFromBoardThread = new Thread(HandleFomBoard) { IsBackground = true };
            handleFromBoardThread.Start();
            var handleThinkingLeDsThread = new Thread(HandleThinkingLeds) { IsBackground = true };
            handleThinkingLeDsThread.Start();
            var probingThread = new Thread(ShowProbingMoves) { IsBackground = true };
            probingThread.Start();
        }

        public ESentioChessBoard(ILogging logger)
        {
            _logger = logger;
            _rm = SpeechTranslator.ResourceManager;
            BatteryLevel = "---";
            BatteryStatus = _rm.GetString("Full");
            Information = Constants.TabutronicSentio;
            PieceRecognition = false;
            ValidForAnalyse = false;
        }

        public override void Reset()
        {
            //
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
                _probingFields.TryDequeue(out _);
            }
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
            _serialCommunication = new SerialCommunication(_logger, portName, _useBluetooth);
            if (_serialCommunication.CheckConnect(portName))
            {
                string readLine = string.Empty;
                int count = 0;
                while (string.IsNullOrWhiteSpace(readLine) && count < 10)
                {
                    readLine = _serialCommunication.GetRawFromBoard(string.Empty);
                    count++;
                    Thread.Sleep(10);
                }
                _serialCommunication.DisConnectFromCheck();
                return readLine.Length > 0;
            }
            _serialCommunication.DisConnectFromCheck();
            return false;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return CheckComPort(portName);
        }

        public override void SetLedForFields(SetLEDsParameter ledsParameter)
        {

            if (!EnsureConnection())
            {
                return;
            }

            lock (_locker)
            {
                // _logger?.LogDebug($"B: set LEDs  {ledsParameter}");
                if (!ledsParameter.IsThinking)
                {
                    lock (_lockThinking)
                    {
                        _lastThinking0 = string.Empty;
                        _lastThinking1 = string.Empty;

                    }
                }
                if (ledsParameter.IsThinking)
                {
                    lock (_lockThinking)
                    {
                        
                        _probingFields.TryDequeue(out _);
                        if (_lastThinking0 != ledsParameter.FieldNames[0] ||
                            _lastThinking1 != ledsParameter.FieldNames[1])
                        {
                            _lastThinking0 = ledsParameter.FieldNames[0];
                            _lastThinking1 = ledsParameter.FieldNames[1];
                        }
                    }

                }
                if (ledsParameter.IsProbing && (_configuration.ShowPossibleMoves || _configuration.ShowPossibleMovesEval))
                {
                   // _logger?.LogDebug($"B: set LEDs for probing {ledsParameter}");
                    _probingFields.TryDequeue(out _);
                    _probingFields.Enqueue(ledsParameter.ProbingMoves);
                    lock (_lockThinking)
                    {
                        _lastThinking0 = string.Empty;
                        _lastThinking1 = string.Empty;
                    }

                    return;
                }

                string[] fieldNames = ledsParameter.FieldNames.Length > 0
                    ? new List<string>(ledsParameter.FieldNames).ToArray()
                    : new List<string>(ledsParameter.InvalidFieldNames).ToArray();
                if (fieldNames.Length == 0)
                {
                    fieldNames = new List<string>(ledsParameter.BookFieldNames).ToArray();
                    if (fieldNames.Length == 0)
                    {
                        return;
                    }

                }
                _probingFields.TryDequeue(out _);
                var joinedString = string.Join(" ", fieldNames);

                if (_prevJoinedString.Equals(joinedString))
                {
                    return;
                }
                _logger?.LogDebug($"B: Set LEDs for {joinedString}");
                _prevJoinedString = joinedString;
                byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                if (fieldNames.Length == 2 && _configuration.ShowMoveLine)
                {
                    string[] moveLine = MoveLineHelper.GetMoveLine(fieldNames[0], fieldNames[1]);
                    foreach (string fieldName in moveLine)
                    {
                        result = UpdateLedsForField(fieldName, result);
                    }
                }
                else
                {
                    foreach (string fieldName in fieldNames)
                    {
                        result = UpdateLedsForField(fieldName, result);
                    }
                }
                _serialCommunication.Send(result,joinedString);
            }
        }

        public override void SetAllLedsOff(bool forceOff)
        {
            if (!EnsureConnection())
            {
                return;
            }
            _logger?.LogDebug("B: Send all off");
            _probingFields.TryDequeue(out _);
            lock (_lockThinking)
            {
                _lastThinking0 = string.Empty;
                _lastThinking1 = string.Empty;
            }

            _serialCommunication.ClearToBoard();
            _serialCommunication.Send(AllOff, forceOff, "All off");
        }

        public override void SetAllLedsOn()
        {
            if (!EnsureConnection())
            {
                return;
            }
            lock (_lockThinking)
            {
                _lastThinking0 = string.Empty;
                _lastThinking1 = string.Empty;
            }

            _probingFields.TryDequeue(out _);
            _logger?.LogDebug("B: Send all on");
            _serialCommunication.Send(AllOn,"All on");
        }

        public override void DimLeds(bool dimLeds)
        {
            //
        }

        public override void DimLeds(int level)
        {
            //
        }

        public override void SetScanTime(int scanTime)
        {
            //
        }

        public override void SetDebounce(int debounce)
        {
            //
        }

        public override void FlashMode(EnumFlashMode flashMode)
        {
            _flashLeds = flashMode == EnumFlashMode.FlashSync;
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            //
        }

        public override void Calibrate()
        {
            IsCalibrated = true;
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override void AdditionalInformation(string information)
        {
         //
        }

        public override void RequestDump()
        {
           //
        }

        public override DataFromBoard GetPiecesFen()
        {
            // return GetDumpPiecesFen();
            string[] changes = { "", "" };
            if (_fromBoard.TryDequeue(out DataFromBoard dataFromBoard))
            {
                var strings = dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length < 8)
                {
                    return new DataFromBoard(_workingChessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0], 3);
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
                        _prevSend = _workingChessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    }

                    // _logger?.LogDebug($"GetPiecesFen: _equalReadCount {_equalReadCount} return {_prevSend}");
                    return new DataFromBoard(_prevSend, 3);
                }
                // _logger?.LogDebug($"GetPiecesFen: fromBoard: {dataFromBoard.FromBoard}");
                if (dataFromBoard.FromBoard.StartsWith(BASE_POSITION))
                {
                    _chessBoard.Init();
                    _chessBoard.NewGame();
                    _workingChessBoard.Init();
                    _workingChessBoard.NewGame();
                    _startFenPosition = string.Empty;
                    BasePositionEvent?.Invoke(this, null);
                    string basePos = _workingChessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    //if (_configuration.SayLiftUpDownFigure && _prevSend != basePos)
                   

                    _prevSend = basePos;
                    return new DataFromBoard(_prevSend, 3)
                    {
                        BasePosition = true,
                        Invalid = false,
                        IsFieldDump = false
                    };
                }
            

                var boardCodeConverter = new BoardCodeConverter(strings, _playWithWhite);
                // _logger?.LogDebug($"strings: {string.Join(" ",strings)}");
                changes[0] = string.Empty;
                changes[1] = string.Empty;
                IChessFigure liftUpFigure = null;
                int liftDownField = Fields.COLOR_OUTSIDE;
                foreach (var boardField in Fields.BoardFields)
                {
                    var isFigureOnBoard = boardCodeConverter.IsFigureOn(boardField);
                    var chessFigure = _workingChessBoard.GetFigureOn(boardField);
                    if (isFigureOnBoard && chessFigure.Color == Fields.COLOR_EMPTY)
                    {
                        changes[1] = Fields.GetFieldName(boardField);
                        _logger?.LogDebug($"GetPiecesFen: Downfield: {boardField}/{changes[1]}");
                        liftDownField = boardField;
                      
                    }

                    if (!isFigureOnBoard && chessFigure.Color != Fields.COLOR_EMPTY)
                    {
                        changes[0] = Fields.GetFieldName(boardField);
                        _logger?.LogDebug($"GetPiecesFen: Lift up field: {boardField}/{changes[0]} {FigureId.FigureIdToEnName[chessFigure.FigureId]}");
                      
                        liftUpFigure = chessFigure;

                    }
                }
                // Nothing changed?
                if (liftDownField == Fields.COLOR_OUTSIDE && liftUpFigure == null)
                {
                    return new DataFromBoard(_prevSend, 3);
                }

                var codeConverter = new BoardCodeConverter(_playWithWhite);
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
                    if (liftDownField == _liftUpEnemyFigure.Field && _liftUpFigure == null)
                    {
                        _logger?.LogDebug($"GetPiecesFen: Equal lift up/down field: {_liftUpEnemyFigure.Field} == {liftDownField}");
                        _workingChessBoard.Init(_chessBoard);
                        _liftUpFigure = null;
                        _liftUpEnemyFigure = null;
                        //    _logger?.LogDebug($"GetPiecesFen: return valid {fenPosition1}");
                        _prevSend = _chessBoard.GetFenPosition()
                            .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                        _logger?.LogDebug($"GetPiecesFen: return {_prevSend}");

                        return new DataFromBoard(_prevSend, 3);

                    }
                }

                if (_liftUpEnemyFigure != null && (!_inDemoMode && _allowTakeBack && liftDownField.Equals(_lastFromField) &&
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

                    for (int i = 0; i < playedMoveList.Length - 1; i++)
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
                        _logger?.LogDebug($"GetPiecesFen: Equal lift up/down field: {_liftUpFigure.Field} == {liftDownField}");
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
                                for (int i = 0; i < playedMoveList.Length - 1; i++)
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
                                        _logger?.LogDebug($"GetPiecesFen: Awaited: {Fields.GetFieldName(_awaitingMoveFromField)} {Fields.GetFieldName(_awaitingMoveToField)}");
                                        _logger?.LogDebug($"GetPiecesFen: Read: {Fields.GetFieldName(_liftUpFigure.Field)} {Fields.GetFieldName(liftDownField)}");
                                        return new DataFromBoard(_prevSend, 3);
                                    }
                                }
                                _logger?.LogDebug($"GetPiecesFen: Make move: {Fields.GetFieldName(_liftUpFigure.Field)} {Fields.GetFieldName(liftDownField)}");
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
                            _workingChessBoard.CurrentColor = _liftUpFigure.FigureColor == Fields.COLOR_WHITE ? Fields.COLOR_BLACK : Fields.COLOR_WHITE;

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
                        _chessBoard.CurrentColor = _liftUpFigure.FigureColor == Fields.COLOR_WHITE ? Fields.COLOR_BLACK : Fields.COLOR_WHITE;
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
                            _liftUpFigure = new SentioFigure() { Field = liftUpFigure.Field, Figure = liftUpFigure.FigureId, FigureColor = liftUpFigure.Color };
                            _logger?.LogDebug($"GetPiecesFen: new _liftUpFigure {FigureId.FigureIdToEnName[_liftUpFigure.Figure]}");

                        }
                    }
                    else
                    {
                        _liftUpEnemyFigure = new SentioFigure { Field = liftUpFigure.Field, Figure = liftUpFigure.FigureId, FigureColor = liftUpFigure.Color };
                        _logger?.LogDebug($"GetPiecesFen: new _liftUpEnemyFigure {FigureId.FigureIdToEnName[_liftUpEnemyFigure.Figure]}");
                    }
                }
            }
            var newSend = _workingChessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            if (!newSend.Equals((_prevSend)))
            {
                _logger?.LogDebug($"GetPiecesFen: return changes from {_prevSend} to {newSend}");
            }
            _prevSend = newSend;
            return new DataFromBoard(_prevSend, 3);
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            if (_fromBoard.TryDequeue(out DataFromBoard dataFromBoard))
            {
                var strings =
                    dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length < 8)
                {
                    return new DataFromBoard(string.Empty, 3);
                }

                var boardCodeConverter = new BoardCodeConverter(strings, _playWithWhite);
                List<string> dumpFields = new List<string>();
                foreach (var boardField in Fields.BoardFields)
                {

                    {
                        if (boardCodeConverter.IsFigureOn(boardField))

                        {
                            dumpFields.Add(Fields.GetFieldName(boardField));
                        }

                    }

                }

                return new DataFromBoard(string.Join(",", dumpFields), 3)
                    { IsFieldDump = true, BasePosition = false };
            }
            return new DataFromBoard(string.Empty, 3);
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
                lock (_lockThinking)
                {
                    _lastThinking0 = string.Empty;
                    _lastThinking1 = string.Empty;
                }
                _probingFields.TryDequeue(out _);

            }
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        private byte[] UpdateLedsForField(string fieldName, byte[] current)
        {
            // Exact two letters expected, e.g. "E2"
            if (string.IsNullOrWhiteSpace(fieldName) || fieldName.Length != 2)
            {
                return current;
            }

            var colName = fieldName[0].ToString().ToUpper();
            if (!int.TryParse(fieldName[1].ToString(), out int rowNumber))
            {
                return current;
            }
            // Don't manipulate parameters
            byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
            Array.Copy(current, result, current.Length);
            if (_playWithWhite)
            {
                result[8 - rowNumber] |= _colName2ColByte[colName];
            }
            else
            {
                result[rowNumber - 1] |= _flippedColName2ColByte[colName];
            }
            return result;
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

        private void HandleThinkingLeds()
        {
            while (!_release) 
            {
                Thread.Sleep(10);
                lock (_lockThinking)
                {
                    if (!string.IsNullOrWhiteSpace(_lastThinking0) && !string.IsNullOrWhiteSpace(_lastThinking1))
                    {
                        
                        var fieldName = _lastThinking0;
                        byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                        Array.Copy(AllOff, result, AllOff.Length);
                        result = UpdateLedsForField(fieldName, result);
                        //Array.Copy(result, _lastSendBytes, result.Length);
                        _serialCommunication.Send(result, $"Thinking {fieldName}");
                        Thread.Sleep(200);
                        fieldName = _lastThinking1;
                        result = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                        Array.Copy(AllOff, result, AllOff.Length);
                        result = UpdateLedsForField(fieldName, result);
                        // Array.Copy(result, _lastSendBytes, result.Length);
                        _serialCommunication.Send(result, $"Thinking {fieldName}");
                        //_lastThinking0 = string.Empty;
                        ///lastThinking1 = string.Empty;
                        Thread.Sleep(200);
                    }
                }
            }
        }

        private void ShowProbingMoves()
        {
            var switchSide = false;

            while (!_release)
            {
                if (_probingFields.TryPeek(out ProbingMove[] fields))
                {
                    if (!_acceptProbingMoves)
                    {
                        _probingFields.TryDequeue(out _);
                        // SetAllLedsOff(true);
                        continue;
                    }
                    var probingMove = fields.OrderByDescending(f => f.Score).First();
                    byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                    if (switchSide)
                    {
                        foreach (var field in fields)
                        {
                            if (field.FieldName.Equals(probingMove.FieldName))
                            {
                                continue;
                            }

                            if (_configuration.ShowPossibleMoves)
                            {
                                result = UpdateLedsForField(field.FieldName, result);
                            }
                        }

                    }
                    else
                    {
                        foreach (var field in fields)
                        {
                            if (_configuration.ShowPossibleMovesEval && field.FieldName.Equals(probingMove.FieldName))
                            {
                                result = UpdateLedsForField(field.FieldName, result);
                            }

                            if (_configuration.ShowPossibleMoves)
                            {
                                result = UpdateLedsForField(field.FieldName, result);
                            }
                        }
                    }
                    switchSide = !switchSide;
                    var joinedString = string.Join(" ", fields.Select(f => f.FieldName));
                    _serialCommunication.Send(result,$"Probing {joinedString}");
                    Thread.Sleep(200);
                    continue;
                }

                Thread.Sleep(10);
            }
        }
    }
}

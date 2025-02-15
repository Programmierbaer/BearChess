using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChess.TabuTronic.Tactum.ChessBoard
{
    public class ETactumChessBoard : AbstractEBoard
    {
        private class TactumFigure
        {
            public TactumFigure()
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
        private const string NEW_GAME_POSITION = "255 255 0 0 8 0 255 247";

      
        private readonly EChessBoardConfiguration _configuration;
        private readonly IChessBoard _chessBoard;
        private readonly IChessBoard _workingChessBoard;
        private string _prevJoinedString = string.Empty;
        private readonly ConcurrentQueue<DataFromBoard> _fromBoard = new ConcurrentQueue<DataFromBoard>();
        private bool _release = false;
        private TactumFigure _liftUpFigure = null;
        private TactumFigure _liftUpEnemyFigure = null;
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
        private string _startFenPosition = string.Empty;
        private readonly bool _useBluetooth;
        private readonly ISpeech _synthesizer;
        private ResourceManager _rm;
        private readonly string _speechLanguageTag;
        private string _lastThinking0 = string.Empty;
        private string _lastThinking1 = string.Empty;
        private string _lastProbing = string.Empty;


        public ETactumChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _configuration = configuration;
            _useBluetooth = configuration.UseBluetooth;
            _rm = SpeechTranslator.ResourceManager;
            _serialCommunication = new SerialCommunication(logger, _configuration.PortName, _configuration.UseBluetooth);
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = _rm.GetString("Full");
            PieceRecognition = false;
            ValidForAnalyse = true;
            MultiColorLEDs = true;
            UseFieldDumpForFEN = true;
            IsConnected = EnsureConnection();
            Information = Constants.TabutronicTactum;
            _chessBoard = new BearChessBase.Implementations.ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _workingChessBoard = new BearChessBase.Implementations.ChessBoard();
            _workingChessBoard.Init();
            _workingChessBoard.NewGame();
            var handleFromBoardThread = new Thread(HandleFomBoard) { IsBackground = true };
            handleFromBoardThread.Start();
            _synthesizer = BearChessSpeech.Instance;
            _speechLanguageTag = Configuration.Instance.GetConfigValue("selectedSpeechCulture", CultureInfo.CurrentCulture.IetfLanguageTag).ToLower();
        }

        public ETactumChessBoard(ILogging logger)
        {
            _logger = logger;
            _rm = SpeechTranslator.ResourceManager;
            BatteryLevel = "---";
            BatteryStatus = _rm.GetString("Full");
            Information = Constants.TabutronicTactum;
            PieceRecognition = false;
            ValidForAnalyse = true;
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
                var readLine = string.Empty;
                var count = 0;
                while (string.IsNullOrWhiteSpace(readLine) && count < 10)
                {
                    readLine = _serialCommunication.GetRawFromBoard(string.Empty);
                    count++;
                    Thread.Sleep(10);
                }
                _serialCommunication.DisConnectFromCheck();
                return !string.IsNullOrEmpty(readLine);
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
                _logger?.LogDebug($"B: set LEDs  {ledsParameter}");
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
                        if (_lastThinking0 != ledsParameter.FieldNames[0] ||
                            _lastThinking1 != ledsParameter.FieldNames[1])
                        {
                            _lastThinking0 = ledsParameter.FieldNames[0];
                            _lastThinking1 = ledsParameter.FieldNames[1];

                            if (_configuration.ShowPossibleMovesEval)
                            {
                                _synthesizer?.SpeakAsync(
                                    $"{_rm.GetString("BestMove")} {ledsParameter.FieldNames[0]} {ledsParameter.FieldNames[1]}");
                            }
                        }
                        return;
                    }
                }
                if (ledsParameter.IsProbing && (_configuration.ShowPossibleMoves || _configuration.ShowPossibleMovesEval))
                {
                    var probingMove = ledsParameter.ProbingMoves.OrderByDescending(f => f.Score).First();
                    if (_lastProbing != probingMove.FieldName)
                    {
                        _lastProbing = probingMove.FieldName;
                        _synthesizer?.SpeakAsync($"{_rm.GetString("BestField")} {probingMove.FieldName}" );
                        lock (_lockThinking)
                        {
                            _lastThinking0 = string.Empty;
                            _lastThinking1 = string.Empty;
                        }
                    }

                    return;
                }

                var fieldNames = new List<string>(ledsParameter.FieldNames);
                fieldNames.AddRange(ledsParameter.InvalidFieldNames);
                //string[] fieldNames = new List<string>(ledsParameter.FieldNames).AddRange(ledsParameter.InvalidFieldNames.ToList())
                if (fieldNames.Count == 0)
                {
                    fieldNames.AddRange(ledsParameter.BookFieldNames);
                    if (fieldNames.Count == 0)
                    {
                        return;
                    }

                }

                var joinedString = string.Join(" ", fieldNames);

                if (_prevJoinedString.Equals(joinedString))
                {
                    return;
                }
                if (ledsParameter.InvalidFieldNames.Length > 0)
                {
                    for (int i = 0; i < ledsParameter.InvalidFieldNames.Length; i++)
                    {
                        _synthesizer?.SpeakAsync($"{_rm.GetString("Invalid")} {ledsParameter.InvalidFieldNames[i]}");
                    }
                }
                _logger?.LogDebug($"Set LEDs for {joinedString}");
                _prevJoinedString = joinedString;
                byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }

        public override void SetAllLedsOff(bool forceOff)
        {
            if (!EnsureConnection())
            {
                return;
            }
            _logger?.LogDebug("B: Send all off");

            lock (_lockThinking)
            {
                _lastThinking0 = string.Empty;
                _lastThinking1 = string.Empty;
                _lastProbing = string.Empty;
            }

            //_serialCommunication.ClearToBoard();
            //_serialCommunication.Send(AllOff, forceOff, "All off");
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
                _lastProbing = string.Empty;
            }


            //_logger?.LogDebug("B: Send all on");
            //_serialCommunication.Send(AllOn, "All on");
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
            //
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

        public override string RequestInformation(string message)
        {
            return _chessBoard.GetFenPosition();
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

                if (_equalReadCount < 15)
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
                    string basePos =_workingChessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    //if (_configuration.SayLiftUpDownFigure && _prevSend != basePos)
                    if (_prevSend != basePos)
                    {
                        _synthesizer?.SpeakForce($"{SpeechTranslator.ResourceManager.GetString("BasePosition")}");
                        
                    }

                    _prevSend = basePos;
                    return new DataFromBoard(_prevSend, 3)
                    {
                        BasePosition = true,
                        Invalid = false,
                        IsFieldDump = false
                    };
                }
                if (dataFromBoard.FromBoard.StartsWith(NEW_GAME_POSITION))
                {
                   
                  //  NewGamePositionEvent?.Invoke(this, null);
                    //string basePos = _workingChessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    if (_equalReadCount == 15)
                    {
                        _synthesizer?.SpeakForce($"{SpeechTranslator.ResourceManager.GetString("NewGame")}");
                        _synthesizer?.SpeakForce($"{SpeechTranslator.ResourceManager.GetString("GoBackToBasePosition")}");
                        NewGamePositionEvent?.Invoke(this, null);
                    }

                    //_prevSend = basePos;
                    DataEvent?.Invoke(this, "NewGame");
                    //return new DataFromBoard(_prevSend, 3)
                    //{
                       
                    //    Invalid = false,
                    //    IsFieldDump = false,
                       
                    //};
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
                        if (_configuration.SayLiftUpDownFigure)
                        {
                            _synthesizer?.SpeakAsync(changes[1]);
                        }
                    }

                    if (!isFigureOnBoard && chessFigure.Color != Fields.COLOR_EMPTY)
                    {
                        changes[0] = Fields.GetFieldName(boardField);
                        _logger?.LogDebug($"GetPiecesFen: Lift up field: {boardField}/{changes[0]} {FigureId.FigureIdToEnName[chessFigure.FigureId]}");
                        if (_configuration.SayLiftUpDownFigure)
                        {
                            _synthesizer?.SpeakAsync(SpeechTranslator.GetFigureName(chessFigure.FigureId,_speechLanguageTag,Configuration.Instance)+ " " + changes[0]);
                        }
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
                                // _synthesizer?.Clear();
                                _synthesizer?.SpeakAsync(Fields.GetFieldName(liftDownField),true);
                                //SpeakMove(_liftUpFigure.Figure,FigureId.NO_PIECE,_liftUpFigure.FieldName,Fields.GetFieldName(liftDownField),FigureId.NO_PIECE,string.Empty);
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
                            _liftUpFigure = new TactumFigure { Field = liftUpFigure.Field, Figure = liftUpFigure.FigureId, FigureColor = liftUpFigure.Color };
                            _logger?.LogDebug($"GetPiecesFen: new _liftUpFigure {FigureId.FigureIdToEnName[_liftUpFigure.Figure]}");
                          
                        }
                    }
                    else
                    {
                        _liftUpEnemyFigure = new TactumFigure { Field = liftUpFigure.Field, Figure = liftUpFigure.FigureId, FigureColor = liftUpFigure.Color };
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

        private void SpeakMove(int fromFieldFigureId, int toFieldFigureId, string fromFieldFieldName, string toFieldFieldName, int promoteFigureId, string shortMoveIdentifier)
        {

            var isDone = false;
            switch (fromFieldFigureId)
            {
                case FigureId.WHITE_KING:
                    {
                        if (fromFieldFieldName.Equals("E1"))
                        {
                            if (toFieldFieldName.Equals("G1"))
                            {
                                _synthesizer.SpeakAsync($"{_rm.GetString("YourMove")} { SpeechTranslator.GetCastleKingsSide(_speechLanguageTag, Configuration.Instance)}");
                                isDone = true;
                            }
                            if (toFieldFieldName.Equals("C1"))

                            {
                                _synthesizer.SpeakAsync($"{_rm.GetString("YourMove")} {SpeechTranslator.GetCastleQueensSide(_speechLanguageTag, Configuration.Instance)}");
                                isDone = true;
                            }
                        }

                        break;
                    }
                case FigureId.BLACK_KING:
                    {
                        if (fromFieldFieldName.Equals("E8"))
                        {
                            if (toFieldFieldName.Equals("G8"))
                            {
                                _synthesizer.SpeakAsync($"{_rm.GetString("YourMove")} {SpeechTranslator.GetCastleKingsSide(_speechLanguageTag, Configuration.Instance)}");
                                isDone = true;
                            }
                            if (toFieldFieldName.Equals("C8"))
                            {
                                _synthesizer.SpeakAsync($"{_rm.GetString("YourMove")} {SpeechTranslator.GetCastleQueensSide(_speechLanguageTag, Configuration.Instance)}");
                                isDone = true;
                            }
                        }

                        break;
                    }
            }

            if (!isDone)
            {

                if (toFieldFigureId == FigureId.NO_PIECE)
                {

                    _synthesizer.SpeakAsync(
                        $"{_rm.GetString("YourMove")} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, Configuration.Instance)} {SpeechTranslator.GetFrom(_speechLanguageTag, Configuration.Instance)} {fromFieldFieldName}, " +
                        $"{SpeechTranslator.GetTo(_speechLanguageTag, Configuration.Instance)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, Configuration.Instance)}");
                }
                else
                {
                    _synthesizer.SpeakAsync(
                        $"{_rm.GetString("YourMove")} {SpeechTranslator.GetFigureName(fromFieldFigureId, _speechLanguageTag, Configuration.Instance)} {SpeechTranslator.GetFrom(_speechLanguageTag, Configuration.Instance)} {fromFieldFieldName}, " +
                        $"{SpeechTranslator.GetCapture(_speechLanguageTag, Configuration.Instance)} {SpeechTranslator.GetFigureName(toFieldFigureId, _speechLanguageTag, Configuration.Instance)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, _speechLanguageTag, Configuration.Instance)}");
                }
            }
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
                    if (boardCodeConverter.IsFigureOn(boardField))

                    {
                        dumpFields.Add(Fields.GetFieldName(boardField));
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
                    _lastProbing = string.Empty;
                }
            }
        }

        public override void SpeedLeds(int level)
        {
            //
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
                ;
            }
        }

    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.ChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly bool _useBluetooth;

        private readonly string _basePosition = "255 255 0 0 0 0 255 255";

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

      
     
        private readonly byte[] _lastSendBytes = { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static byte ColA = 0x1;
        public static byte ColB = 0x1 << 1;
        public static byte ColC = 0x1 << 2;
        public static byte ColD = 0x1 << 3;
        public static byte ColE = 0x1 << 4;
        public static byte ColF = 0x1 << 5;
        public static byte ColG = 0x1 << 6;
        public static byte ColH = 0x1 << 7;
        public static byte[] AllOff = { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static byte[] AllOn = { 255, 255, 255, 255, 255, 255, 255, 255 };
        private bool _flashLeds;
        private readonly IChessBoard _chessBoard;
        private string _prevRead = string.Empty;
        private string _fromField = string.Empty;
        private string _toField = string.Empty;
        private string _lastFromField = string.Empty;
        private string _lastToField = string.Empty;
        private string _lastLogMessage;
        private ulong _repeated = 0;
        private readonly ConcurrentQueue<DataFromBoard> _fromBoard = new ConcurrentQueue<DataFromBoard>();
        private string _lastMove;
        private byte[] _lastSend = Array.Empty<byte>();
        private readonly List<string>_requiredLeds = new List<string>();
        private readonly List<string>_thinkingLeds = new List<string>();
        private readonly List<string>_takeBackLeds = new List<string>();
        private ulong _deBounce;
        private readonly object _lock = new object();
        private readonly object _lockThinking = new object();
        private bool _beginHelpRequested;
        private int _helpRequestedField;

        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth)
        {
            _useBluetooth = useBluetooth;
            _serialCommunication = new SerialCommunication(logger, portName, useBluetooth);
          
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            PieceRecognition = false;
            SelfControlled = true;
            IsConnected = EnsureConnection();
            Information = Constants.TabutronicSentio;
            _chessBoard = new BearChessBase.Implementations.ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _fromField = string.Empty;
            _toField = string.Empty;
            var handleLeDsThread = new Thread(HandleLeds) { IsBackground = true };
            handleLeDsThread.Start();
            var handleFromBoardThread = new Thread(HandleFomBoard) { IsBackground = true };
            handleFromBoardThread.Start();
            var handleThinkingLeDsThread = new Thread(HandleThinkingLeds) { IsBackground = true };
            handleThinkingLeDsThread.Start();
            _lastMove = string.Empty;
            _requiredLeds.Clear();
            _takeBackLeds.Clear();
            _thinkingLeds.Clear();
            _deBounce = 20;
        
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            Information = Constants.TabutronicSentio;
            _fromField = string.Empty;
            _toField = string.Empty;
            PieceRecognition = false;
            SelfControlled = true;
            _lastMove = string.Empty;
            _requiredLeds.Clear();
            _takeBackLeds.Clear();
            _thinkingLeds.Clear();
            _deBounce = 20;
        }


        public override void Reset()
        {
            //
        
        }

        public override bool CheckComPort(string portName)
        {
            _serialCommunication = new SerialCommunication(_logger, portName, _useBluetooth);
            if (_serialCommunication.CheckConnect(portName))
            {
                var readLine = _serialCommunication.GetRawFromBoard(string.Empty);
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

        private void HandleThinkingLeds()
        {
            while (true)
            {
                Thread.Sleep(10);
                lock (_lockThinking)
                {
                    if (_thinkingLeds.Count > 1)
                    {
                        _beginHelpRequested = false;
                        string fieldName = _thinkingLeds[0];
                        byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                        Array.Copy(AllOff, result, AllOff.Length);
                        result = UpdateLedsForField(fieldName, result);
                        Array.Copy(result, _lastSendBytes, result.Length);
                        _serialCommunication.Send(result);
                        Thread.Sleep(200);
                        fieldName = _thinkingLeds[1];
                        result = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                        Array.Copy(AllOff, result, AllOff.Length);
                        result = UpdateLedsForField(fieldName, result);
                        Array.Copy(result, _lastSendBytes, result.Length);
                        _serialCommunication.Send(result);
                        Thread.Sleep(200);
                    }
                }
            }
        }

        private void HandleLeds()
        {
            while (true)
            {
                Thread.Sleep(10);
                if (_fromBoard.TryDequeue(out DataFromBoard dataFromBoard))
                {
                    var strings = dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length < 8)
                    {
                        continue;
                    }

                    var invalidFields = GetInvalidFields(strings);
                    if (invalidFields.Length > 0)
                    {
                        byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                        Array.Copy(AllOff, result, AllOff.Length);

                        foreach (var fieldName in invalidFields)
                        {
                            result = UpdateLedsForField(fieldName, result);
                        }

                        Array.Copy(result, _lastSendBytes, result.Length);
                        if (!result.SequenceEqual(_lastSend))
                        {
                            _lastSend = result;
                            _serialCommunication.Send(result);
                        }
                    }
                    else
                    {
                        if (!AllOff.SequenceEqual(_lastSend))
                        {
                            _lastSend = AllOff;
                            SetAllLedsOff();
                        }
                    }
                }
                else
                {
                    if (_requiredLeds.Count > 0)
                    {
                        byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                        Array.Copy(AllOff, result, AllOff.Length);

                        foreach (string fieldName in _requiredLeds)
                        {
                            result = UpdateLedsForField(fieldName, result);
                        }
                        Array.Copy(result, _lastSendBytes, result.Length);
                        if (!result.SequenceEqual(_lastSend))
                        {
                            _lastSend = result;
                            _serialCommunication.Send(result);
                        }
                    }
                }
            }
        }


        private string[] GetInvalidFields(string[] boardCodes)
        {
            var invalidFields = new List<string>();
            var boardCodeConverter = new BoardCodeConverter(boardCodes, _playWithWhite);
            foreach (var boardField in Fields.BoardFields)
            {
                var isFigureOnBoard = boardCodeConverter.IsFigureOn(boardField);
                var chessFigure = _chessBoard.GetFigureOn(boardField);
                if (chessFigure == null)
                {
                    continue;
                }

                if ((isFigureOnBoard && chessFigure.Color == Fields.COLOR_EMPTY)
                    || (!isFigureOnBoard && chessFigure.Color != Fields.COLOR_EMPTY))
                {
                    var fieldName = Fields.GetFieldName(boardField);
                    if (!_requiredLeds.Contains(fieldName))
                    {
                        invalidFields.Add(fieldName);
                    }
                }
            }
            if (_requiredLeds.Count>1)
              invalidFields.AddRange(_requiredLeds);
            invalidFields.AddRange(_takeBackLeds);
            return invalidFields.ToArray();
        }

        private void HandleFomBoard()
        {
            while (true)
            {
                Thread.Sleep(10);
                var dataFromBoard = _serialCommunication.GetFromBoard();
                _fromBoard.Enqueue(dataFromBoard);
            }
        }

        public override void SetLedForFields(string[] fieldNames, string promote, bool thinking, bool isMove, string displayString)
        {
            if (!EnsureConnection())
            {
                return;
            }
            
            lock (_locker)
            {
                if (!thinking)
                {
                    lock (_lockThinking)
                    {
                        if (_thinkingLeds.Count > 0)
                        {
                            _thinkingLeds.Clear();
                            _serialCommunication.Send(AllOff);
                        }
                    }
                }
                if (fieldNames.Length > 1)
                {
                    if (thinking)
                    {
                        lock (_lockThinking)
                        {
                            _thinkingLeds.Clear();
                            _thinkingLeds.Add(fieldNames[0]);
                            _thinkingLeds.Add(fieldNames[1]);
                            return;
                        }
                    }
                    _requiredLeds.Clear();
                    string m = $"{fieldNames[0]}{fieldNames[1]}";
                    _logger?.LogDebug($"Set LEDs for {m}  Last move was: {_lastMove}");
                    if (m.Equals(_lastMove))
                    {
                        return;
                    }
                   
                    var fieldNumberFrom = Fields.GetFieldNumber(fieldNames[0]);
                    var fieldNumberTo = Fields.GetFieldNumber(fieldNames[1]);
                    if (_chessBoard.MoveIsValid(fieldNumberFrom, fieldNumberTo))
                    {
                        _requiredLeds.Add(fieldNames[0]);
                        _requiredLeds.Add(fieldNames[1]);
                        _takeBackLeds.Clear();
                    }
                }
            }
        }

        public override void SetLastLeds()
        {
            if (!EnsureConnection())
            {
                return;
            }

            lock (_locker)
            {
                _serialCommunication.Send(_lastSendBytes);
            }
        }

        public override void SetAllLedsOff()
        {
            if (!EnsureConnection())
            {
                return;
            }
            _logger?.LogDebug("B: Send all off");
            lock (_locker)
            {
                _requiredLeds.Clear();
            }

            _serialCommunication.ClearToBoard();
            _serialCommunication.Send(AllOff);
        }

        public override void SetAllLedsOn()
        {
            if (!EnsureConnection())
            {
                return;
            }
            _logger?.LogDebug("B: Send all on");
            _serialCommunication.Send(AllOn);
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
            // ignore
        }

        public override void SetDebounce(int debounce)
        {
            _deBounce = (ulong)debounce;
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        public override void FlashMode(EnumFlashMode flashMode)
        {
            _flashLeds = flashMode == EnumFlashMode.FlashSync;
        }

       

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            // ignore
        }

        public override void Calibrate()
        {
            IsCalibrated = true;
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override void RequestDump()
        {
           //
        }

        public override DataFromBoard GetDumpPiecesFen() 
        {
            if (_fromBoard.TryDequeue(out DataFromBoard dataFromBoard))
            {
                var strings =
                    dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length < 8)
                {
                    return new DataFromBoard(string.Empty,3);
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

        public override DataFromBoard GetPiecesFen()
        {
            string[] changes = { "", "" };
            lock (_lock)
            {
                while (true)
                {
                    if (_fromBoard.TryDequeue(out DataFromBoard dataFromBoard))
                    {
                       
                        if (!dataFromBoard.FromBoard.Equals(_prevRead))
                        {
                            _logger?.LogDebug($"TC: Read from board: {dataFromBoard.FromBoard}");
                        }
                        
                        _prevRead = dataFromBoard.FromBoard;

                        var strings =  dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (strings.Length < 8)
                        {
                            break;
                        }

                        if (dataFromBoard.FromBoard.StartsWith(_basePosition))
                        {
                            _requiredLeds.Clear();
                            _takeBackLeds.Clear();
                            lock (_lockThinking)
                            {
                                _thinkingLeds.Clear();
                            }
                            _chessBoard.Init();
                            _chessBoard.NewGame();
                            _repeated = 0;
                            BasePositionEvent?.Invoke(this, null);
                            return new DataFromBoard(_chessBoard.GetFenPosition(), 3)
                                   {
                                       BasePosition = true,
                                       Invalid = false,
                                       IsFieldDump = false
                            };
                        }


                        var boardCodeConverter = new BoardCodeConverter(strings, _playWithWhite);
                    
                        changes[0] = string.Empty;
                        changes[1] = string.Empty;
                        foreach (var boardField in Fields.BoardFields)
                        {
                            var isFigureOnBoard = boardCodeConverter.IsFigureOn(boardField);
                            var chessFigure = _chessBoard.GetFigureOn(boardField);
                            if (isFigureOnBoard && chessFigure.Color == Fields.COLOR_EMPTY)
                            {
                                changes[1] = Fields.GetFieldName(boardField);
                            }

                            if (!isFigureOnBoard && chessFigure.Color != Fields.COLOR_EMPTY)
                            {
                                changes[0] = Fields.GetFieldName(boardField);
                            }
                        }
                        if (string.IsNullOrEmpty(changes[0]) && string.IsNullOrEmpty(changes[1]))
                        {
                            if (_beginHelpRequested)
                            {
                                if (boardCodeConverter.IsFigureOn(_helpRequestedField))
                                {
                                    _logger?.LogDebug("SC: Help requested");
                                    HelpRequestedEvent?.Invoke(this, EventArgs.Empty);
                                }
                            }
                            _beginHelpRequested = false;
                        }

                        var chessFigures = _chessBoard.GetFigures(_chessBoard.CurrentColor);
                        foreach (var chessFigure in chessFigures)
                        {
                            if (!boardCodeConverter.IsFigureOn(chessFigure.Field))
                            {
                                var fieldName = Fields.GetFieldName(chessFigure.Field);
                                if (string.IsNullOrWhiteSpace(_fromField) || fieldName != _fromField)
                                {
                                    _fromField = fieldName;
                                    _toField = string.Empty;
                                    if (chessFigure.GeneralFigureId==FigureId.KING)
                                    {
                                        _beginHelpRequested = true;
                                        _helpRequestedField = chessFigure.Field;
                                    }
                                }

                                break;
                            }

                            _fromField = string.Empty;

                        }

                        chessFigures = _chessBoard.GetFigures(_chessBoard.EnemyColor);
                        var captureMove = false;
                        foreach (var chessFigure in chessFigures)
                        {
                            if (!boardCodeConverter.IsFigureOn(chessFigure.Field))
                            {
                                _repeated++;
                                var fieldName = Fields.GetFieldName(chessFigure.Field);
                                if (!string.IsNullOrWhiteSpace(_toField) && fieldName != _toField)
                                {
                                    _repeated = 0;
                                }

                                _toField = fieldName;
                                captureMove = true;
                                break;
                            }
                        }

                        if (!captureMove)
                        {
                            foreach (var boardField in Fields.BoardFields)
                            {
                                var isFigureOnBoard = boardCodeConverter.IsFigureOn(boardField);
                                var chessFigure = _chessBoard.GetFigureOn(boardField);
                                if (isFigureOnBoard && chessFigure.Color == Fields.COLOR_EMPTY)
                                {
                                    _repeated++;
                                    var fieldName = Fields.GetFieldName(chessFigure.Field);
                                    if (!string.IsNullOrWhiteSpace(_toField) && fieldName != _toField)
                                    {
                                        _repeated = 0;
                                    }

                                    _toField = Fields.GetFieldName(chessFigure.Field);
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(_toField))
                        {
                            return new DataFromBoard(_chessBoard.GetFenPosition(), 3);
                        }

                        break;
                    }
                }

                var logMessage = $"TC: Current: [{_fromField}-{_toField}]   Last: [{_lastFromField}-{_lastToField}] ";
                if (!logMessage.Equals(_lastLogMessage))
                {
                    _logger?.LogDebug(logMessage);
                    _lastLogMessage = logMessage;
                }


                if (!_inDemoMode && _allowTakeBack && !string.IsNullOrWhiteSpace(_lastToField) &&
                    !string.IsNullOrWhiteSpace(_toField) &&
                    changes[0].Equals(_lastToField, StringComparison.OrdinalIgnoreCase) &&
                    changes[1].Equals(_lastFromField, StringComparison.OrdinalIgnoreCase))
                {
                    _takeBackLeds.Clear();
                    _logger?.LogInfo("TC: Take move back. Replay all previous moves");
                    var playedMoveList = _chessBoard.GetPlayedMoveList();
                    _chessBoard.Init();
                    _chessBoard.NewGame();
                    for (int i = 0; i < playedMoveList.Length - 1; i++)
                    {
                        _logger?.LogDebug($"TC: Move {playedMoveList[i]}");
                        _chessBoard.MakeMove(playedMoveList[i]);
                        _lastFromField = playedMoveList[i].FromFieldName.ToLower();
                        _lastToField = playedMoveList[i].ToFieldName.ToLower();
                    }

                    _takeBackLeds.Add(_lastFromField);
                    _takeBackLeds.Add(_lastToField);
                    _fromField = string.Empty;
                    _toField = string.Empty;

                }

                if (!string.IsNullOrWhiteSpace(_fromField) && !string.IsNullOrWhiteSpace(_toField))
                {
                    string fromField = _fromField;
                    var fromFieldNumber = Fields.GetFieldNumber(fromField);
                    var toFieldNumber = Fields.GetFieldNumber(_toField);
                    var fromFieldColor = _chessBoard.GetFigureOn(fromFieldNumber).Color;
                    var toFieldColor = _chessBoard.GetFigureOn(toFieldNumber).Color;

                    if (fromFieldColor == _chessBoard.CurrentColor &&
                        (toFieldColor == Fields.COLOR_EMPTY || toFieldColor == _chessBoard.EnemyColor))
                    {

                        if (_chessBoard.MoveIsValid(fromFieldNumber, toFieldNumber))
                        {
                            if (_repeated > _deBounce)
                            {
                                _logger?.LogDebug($"TC: MakePgnMove: {fromField} {_toField}");
                                _lastMove = _fromField + _toField;
                                _chessBoard.MakeMove(fromField, _toField);
                                _lastFromField = fromField;
                                _lastToField = _toField;
                                _fromField = string.Empty;
                                _toField = string.Empty;
                                _repeated = 0;
                                _takeBackLeds.Clear();
                            }
                        }
                        else
                        {

                            var chessBoard = new BearChessBase.Implementations.ChessBoard();
                            chessBoard.Init(_chessBoard);
                            var chessFigure = chessBoard.GetFigureOn(fromFieldNumber);
                            chessBoard.RemoveFigureFromField(fromFieldNumber);
                            chessBoard.RemoveFigureFromField(toFieldNumber);
                            chessBoard.SetFigureOnPosition(chessFigure.FigureId, toFieldNumber);
                            chessBoard.CurrentColor = chessFigure.EnemyColor;

                            var fenPosition = chessBoard.GetFenPosition();
                            //              _logger?.LogDebug($"GetPiecesFen: return invalid {fenPosition}"); 
                            return new DataFromBoard(fenPosition, 3);
                        }
                    }
                }

                var fenPosition1 = _chessBoard.GetFenPosition();
                //    _logger?.LogDebug($"GetPiecesFen: return valid {fenPosition1}");
                return new DataFromBoard(fenPosition1, 3);
                //            return new DataFromBoard(_chessBoard.GetFenPosition(), 3);
            }

        }

        protected override void SetToNewGame()
        {
            lock (_lock)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();

                _fromField = string.Empty;
                _toField = string.Empty;
                _requiredLeds.Clear();
                _takeBackLeds.Clear();
                lock (_lockThinking)
                {
                    _thinkingLeds.Clear();
                }
            }
        }


        public override void Release()
        {
            //
        }

        public override void SetFen(string fen)
        {
            lock (_lock)
            {
                _chessBoard.Init();
                _chessBoard.NewGame();
                _chessBoard.SetPosition(fen, true);
                _fromField = string.Empty;
                _toField = string.Empty;
                _requiredLeds.Clear();
                _takeBackLeds.Clear();
                lock (_lockThinking)
                {
                    _thinkingLeds.Clear();
                }
            }
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

        public override event EventHandler BasePositionEvent;
        public override event EventHandler<string> DataEvent;
        public override event EventHandler HelpRequestedEvent;

        public override void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
        {
            //
        }


        #region private


        private byte[] GetLedForFields(string fromFieldName, string toFieldName)
        {
            return UpdateLedsForField(toFieldName, UpdateLedsForField(fromFieldName, AllOff));
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

  
        #endregion

    }
}

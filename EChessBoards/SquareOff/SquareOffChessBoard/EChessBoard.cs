using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.SquareOffChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly IChessBoard _chessBoard;
        private bool _withLeds = false;
        private bool _stopLoop = false;
        private string _lastSendFields = string.Empty;
        private string _prevRead = string.Empty;
        private readonly string[] _fromField = {string.Empty,string.Empty};
        private string _toField = string.Empty;
        private string _lastFromField = string.Empty;
        private string _lastToField = string.Empty;
        private string _prevFieldNames = string.Empty;
        private bool _prevIsBasePosition = false;
                                               
        private readonly string _basePosition = "1100001111000011110000111100001111000011110000111100001111000011";

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

        private readonly Dictionary<byte, string> _invertedFieldByte2FieldName = new Dictionary<byte, string>()
            { { 0, "H1" }, { 1, "G1" }, { 2, "F1" }, { 3, "E1" }, { 4, "D1" }, { 5, "C1" }, { 6, "B1" }, { 7, "A1" },
                { 8, "H2" }, { 9, "G2" }, {10, "F2" }, {11, "E2" }, {12, "D2" }, {13, "C2" }, {14, "B2" }, {15, "A2" },
                {16, "H3" }, {17, "G3" }, {18, "F3" }, {19, "E3" }, {20, "D3" }, {21, "C3" }, {22, "B3" }, {23, "A3" },
                {24, "H4" }, {25, "G4" }, {26, "F4" }, {27, "E4" }, {28, "D4" }, {29, "C4" }, {30, "B4" }, {31, "A4" },
                {32, "H5" }, {33, "G5" }, {34, "F5" }, {35, "E5" }, {36, "D5" }, {37, "C5" }, {38, "B5" }, {39, "A5" },
                {40, "H6" }, {41, "G6" }, {42, "F6" }, {43, "E6" }, {44, "D6" }, {45, "C6" }, {46, "B6" }, {47, "A6" },
                {48, "H7" }, {49, "G7" }, {50, "F7" }, {51, "E7" }, {52, "D7" }, {53, "C7" }, {54, "B7" }, {55, "A7" },
                {56, "H8" }, {57, "G8" }, {58, "F8" }, {59, "E8" }, {60, "D8" }, {61, "C8" }, {62, "B8" }, {63, "A8" }
            };

        private readonly Dictionary<string, string> _invertedFieldName = new Dictionary<string, string>()
            { { "A1", "H8" }, { "B1", "G8" }, { "C1", "F8" }, { "D1", "E8" }, { "E1", "D8" }, {"F1", "C8" }, { "G1", "B8" }, { "H1", "A8" }, 
              { "A2", "H7" }, { "B2", "G7" }, { "C2", "F7" }, { "D2", "E7" }, { "E2", "D7" }, {"F2", "C7" }, { "G2", "B7" }, { "H2", "A7" },
              { "A3", "H6" }, { "B3", "G6" }, { "C3", "F6" }, { "D3", "E6" }, { "E3", "D6" }, {"F3", "C6" }, { "G3", "B6" }, { "H3", "A6" },
              { "A4", "H5" }, { "B4", "G5" }, { "C4", "F5" }, { "D4", "E5" }, { "E4", "D5" }, {"F4", "C5" }, { "G4", "B5" }, { "H4", "A5" },
              { "A5", "H4" }, { "B5", "G4" }, { "C5", "F4" }, { "D5", "E4" }, { "E5", "D4" }, {"F5", "C4" }, { "G5", "B4" }, { "H5", "A4" },
              { "A6", "H3" }, { "B6", "G3" }, { "C6", "F3" }, { "D6", "E3" }, { "E6", "D3" }, {"F6", "C3" }, { "G6", "B3" }, { "H6", "A3" },
              { "A7", "H2" }, { "B7", "G2" }, { "C7", "F2" }, { "D7", "E2" }, { "E7", "D2" }, {"F7", "C2" }, { "G7", "B2" }, { "H7", "A2" },
              { "A8", "H1" }, { "B8", "G1" }, { "C8", "F1" }, { "D8", "E1" }, { "E8", "D1" }, {"F8", "C1" }, { "G8", "B1" }, { "H8", "A1" },
            };

        private readonly int[] _fieldOrders =
        {
            Fields.FA1, Fields.FA2, Fields.FA3, Fields.FA4, Fields.FA5, Fields.FA6,
            Fields.FA7, Fields.FA8,
            Fields.FB1, Fields.FB2, Fields.FB3, Fields.FB4, Fields.FB5, Fields.FB6,
            Fields.FB7, Fields.FB8,
            Fields.FC1, Fields.FC2, Fields.FC3, Fields.FC4, Fields.FC5, Fields.FC6,
            Fields.FC7, Fields.FC8,
            Fields.FD1, Fields.FD2, Fields.FD3, Fields.FD4, Fields.FD5, Fields.FD6,
            Fields.FD7, Fields.FD8,
            Fields.FE1, Fields.FE2, Fields.FE3, Fields.FE4, Fields.FE5, Fields.FE6,
            Fields.FE7, Fields.FE8,
            Fields.FF1, Fields.FF2, Fields.FF3, Fields.FF4, Fields.FF5, Fields.FF6,
            Fields.FF7, Fields.FF8,
            Fields.FG1, Fields.FG2, Fields.FG3, Fields.FG4, Fields.FG5, Fields.FG6,
            Fields.FG7, Fields.FG8,
            Fields.FH1, Fields.FH2, Fields.FH3, Fields.FH4, Fields.FH5, Fields.FH6,
            Fields.FH7, Fields.FH8
        };


        private object _lastLogMessage;
        private bool _dumpRequested;
        private DataFromBoard _dumpDataFromBoard = new DataFromBoard(string.Empty);
        private volatile int _dumpLoopWait;
        private bool _boardSendUpDown;
        private const string REQUEST_DUMP = "R*";
        private const string FIELD_CMD_PREFIX = "30#";
        private const string LED_CMD_PREFIX = "25#";
        private const string BATTERY_INFO_PREFIX = "4#";

        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth, string boardName)
        {
            _withLeds = boardName.Equals(Constants.SquareOffPro);
            _serialCommunication = new SerialCommunication(logger, portName, useBluetooth,boardName);

            _boardSendUpDown = false;
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            ValidForAnalyse = false;
            SelfControlled = false;
            MultiColorLEDs = false;
            Information = boardName;
            IsConnected = EnsureConnection();
            _serialCommunication.Send("14#1*");
            _serialCommunication.Send("4#*");
            _serialCommunication.Send($"{FIELD_CMD_PREFIX}{REQUEST_DUMP}");
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            _dumpLoopWait = 250;
            var requestDumpThread = new Thread(RequestADumpLoop) { IsBackground = true };
            requestDumpThread.Start();
        }

        private void RequestADumpLoop()
        {
            while (!_stopAll)
            {
                Thread.Sleep(_boardSendUpDown ? 1000 : _dumpLoopWait);
                if (!_stopLoop)
                {
                    _serialCommunication.Send($"{FIELD_CMD_PREFIX}{REQUEST_DUMP}");
                }
            }
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            ValidForAnalyse = false;
            SelfControlled = false;
            Information = string.Empty;
            _fromField[0] = string.Empty;
            _fromField[1] = string.Empty;
            _toField = string.Empty;
            _boardSendUpDown = false;
        }
        public override void Reset()
        {
            _chessBoard.Init();
            _chessBoard.NewGame();
            _fromField[0] = string.Empty;
            _fromField[1] = string.Empty;
            _toField = string.Empty;
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
            if (ledsParameter.FieldNames == null || ledsParameter.FieldNames.Length == 0)
            {
                return;
            }
            var fieldNamesLength = ledsParameter.FieldNames.Length;
            if (ledsParameter.IsThinking && !_withLeds)
            {
                return;
            }
            if (fieldNamesLength != 2 && !_withLeds)
            {
                return;
            }
            //_serialCommunication.Send($"25#e2e4*");
            var sendFields = string.Join("", ledsParameter.FieldNames);
            _logger?.LogDebug($"SQ: Request set LED for fields: {sendFields} IsThinking: {ledsParameter.IsThinking}");
            if (sendFields.Equals(_lastSendFields))
            {
                _logger?.LogDebug($"SQ: Ignored equals last send {_lastSendFields}");
                return;
            }
            if (ledsParameter.IsThinking && fieldNamesLength > 1)
            {

                SetLedForFields(new SetLEDsParameter()
                                {
                                    FieldNames = new[] { ledsParameter.FieldNames[0] },
                                    Promote = string.Empty,
                                    IsThinking = true,
                                    IsMove = ledsParameter.IsMove,
                                    DisplayString = string.Empty

                });
                SetLedForFields(new SetLEDsParameter()
                                {
                                    FieldNames = new[] { ledsParameter.FieldNames[1] },
                                    Promote = string.Empty,
                                    IsThinking = true,
                                    IsMove = ledsParameter.IsMove,
                                    DisplayString = string.Empty

                                });
                return;
            }

            _lastSendFields = sendFields;
            if (fieldNamesLength == 2 && _withLeds)
            {
                if (_lastFromField.Equals(ledsParameter.FieldNames[0], StringComparison.OrdinalIgnoreCase) &&
                    _lastToField.Equals(ledsParameter.FieldNames[1], StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogDebug($"SQ: Ignore set LED for fields: {_lastSendFields}: Equals last move {_lastFromField} {_lastToField}");
                    return;
                }

            }
            _logger?.LogDebug($"SQ: Set LED for fields: {_lastSendFields} IsThinking: {ledsParameter.IsThinking}");
            _serialCommunication.Send($"{LED_CMD_PREFIX}{sendFields.ToLower()}*");
        }

        public override void SetAllLedsOff(bool forceOff)
        {
           _serialCommunication.Send($"{LED_CMD_PREFIX}*");
           
        }

        public override void SetAllLedsOn()
        {
            _serialCommunication.Send($"{LED_CMD_PREFIX}a1a2a3a4a5a6a7a8b1b2b3b4b5b6b7b8c1c2c3c4c5c6c7c8d1d2d3d4d5d6d7d8e1e2e3e4e5e6e7e8f1f2f3f4f5f6f7f8g1g2g3g4g5g6g7g8h1h2h3h4h5h6h7h8*");
        }

        public override void DimLeds(bool dimLeds)
        {
            //throw new NotImplementedException();
        }

        public override void DimLeds(int level)
        {
            //throw new NotImplementedException();
        }

        public override void SetScanTime(int scanTime)
        {
            _dumpLoopWait = scanTime > 0 ? scanTime : 1000;
        }

        public override void SetDebounce(int debounce)
        {
            // ignore
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

            while (true)
            {
                var dataFromBoard = _serialCommunication.GetFromBoard();
                if (!dataFromBoard.FromBoard.Equals(_prevRead))
                {
                    _logger?.LogDebug($"SQ: Read from board: {dataFromBoard.FromBoard}");
                    _prevRead = dataFromBoard.FromBoard;
                }


                if (dataFromBoard.FromBoard.Length == 0)
                {
                    break;
                }

                if (dataFromBoard.FromBoard.StartsWith("0#"))
                {
                    if (!_boardSendUpDown)
                    {
                        _fromField[0] = string.Empty;
                        _fromField[1] = string.Empty;
                        _toField = string.Empty;
                    }

                    _boardSendUpDown = true;
                    var fieldName = dataFromBoard.FromBoard.Substring(2, 2);
                    if (!PlayingWithWhite)
                    {
                        fieldName = _invertedFieldName[fieldName.ToUpper()];
                    }

                    if (dataFromBoard.FromBoard.EndsWith("u*"))
                    {
                        if (string.IsNullOrWhiteSpace(_fromField[0]))
                        {
                            _fromField[0] = fieldName;
                        }
                        else
                        {
                            _fromField[1] = fieldName;
                        }

                        _toField = string.Empty;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(_fromField[0]))
                            _toField = fieldName;
                    }

                    break;
                }

                if (dataFromBoard.FromBoard.StartsWith("14#"))
                {
                    break;
                }

               

                if (dataFromBoard.FromBoard.StartsWith(BATTERY_INFO_PREFIX))
                {
                    var battArray = dataFromBoard.FromBoard.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (battArray.Length > 1)
                    {
                        BatteryLevel = battArray[1].Replace("*",string.Empty);
                        BatteryStatus = "🔋";
                    }
                    break;
                }

                if (dataFromBoard.FromBoard.StartsWith(FIELD_CMD_PREFIX))
                {
                    var fieldName = dataFromBoard.FromBoard.Substring(3, dataFromBoard.FromBoard.Length - 3)
                        .Replace("*", string.Empty);
                    var isBasePosition = fieldName.Equals(_basePosition);
                    var dumpFields = new List<string>();

                    for (byte i = 0; i < 64; i++)
                    {
                        var key = fieldName.Substring(i, 1);
                        if (key == "1")
                        {
                            dumpFields.Add(_fieldByte2FieldName[i]);
                        }

                    }


                    if (!_prevIsBasePosition.Equals(isBasePosition))
                    {
                        _logger?.LogDebug("SQ: Base position ");
                    }

                    _prevIsBasePosition = isBasePosition;

                    var fromBoard = string.Join(",", dumpFields);
                    if (!_boardSendUpDown && !_prevFieldNames.Equals(fromBoard))
                    {
                        if (!string.IsNullOrWhiteSpace(_prevFieldNames))
                        {

                            var fieldChanges = FieldChangeHelper.GetFieldChanges(_prevFieldNames, fromBoard);

                            _logger?.LogDebug("SQ: Field changes: ");
                            foreach (var fieldChangesAddedField in fieldChanges.AddedFields)
                            {
                                _logger?.LogDebug($"SQ: Added: {fieldChangesAddedField} ");
                            }

                            foreach (var fieldChangesRemovedField in fieldChanges.RemovedFields)
                            {
                                _logger?.LogDebug($"SQ:  Removed: {fieldChangesRemovedField} ");
                            }

                            if (fieldChanges.RemovedFields.Length == 1)
                            {
                                if (!string.IsNullOrWhiteSpace(_fromField[0]) &&
                                    !string.IsNullOrWhiteSpace(_fromField[1]))
                                {
                                    _fromField[0] = string.Empty;
                                    _fromField[1] = string.Empty;
                                    _toField = string.Empty;
                                }

                                fieldName = fieldChanges.RemovedFields[0];
                                if (string.IsNullOrWhiteSpace(_fromField[0]))
                                {
                                    _fromField[0] = fieldName;
                                }
                                else
                                {
                                    _fromField[1] = fieldName;
                                }

                                _toField = string.Empty;
                            }
                            else
                            {
                                if (fieldChanges.AddedFields.Length == 1)
                                {
                                    if (!string.IsNullOrWhiteSpace(_fromField[0]))
                                        _toField = fieldChanges.AddedFields[0];
                                }
                            }
                        }

                        _prevFieldNames = fromBoard;
                    }

                    if (isBasePosition)
                    {
                        BasePositionEvent?.Invoke(this, null);
                    }

                    if (_dumpRequested)
                    {
                        _dumpRequested = false;

                        _dumpDataFromBoard = new DataFromBoard(fromBoard, 3)
                            { IsFieldDump = true, BasePosition = isBasePosition };
                    }
                }

                break;
            }

            var logMessage =
                $"SQ: Current: [{_fromField[0]}-{_toField}]  Last: [{_lastFromField}-{_lastToField}] ";
            if (!logMessage.Equals(_lastLogMessage))
            {
                _logger?.LogDebug(logMessage);
                _lastLogMessage = logMessage;
            }


            if (!_inDemoMode && _allowTakeBack && !string.IsNullOrWhiteSpace(_lastToField) &&
                !string.IsNullOrWhiteSpace(_toField) &&
                _fromField[0].Equals(_lastToField) &&
                _toField.Equals(_lastFromField))
            {
                _logger?.LogInfo("SQ: Take move back. Replay all previous moves");
                var playedMoveList = _chessBoard.GetPlayedMoveList();
                _chessBoard.Init();
                _chessBoard.NewGame();
                for (int i = 0; i < playedMoveList.Length - 1; i++)
                {
                    _logger?.LogDebug($"SQ: Move {playedMoveList[i]}");
                    _chessBoard.MakeMove(playedMoveList[i]);
                    _lastFromField = playedMoveList[i].FromFieldName.ToLower();
                    _lastToField = playedMoveList[i].ToFieldName.ToLower();

                }

                _fromField[0] = string.Empty;
                _fromField[1] = string.Empty;
                _toField = string.Empty;
                //  return new DataFromBoard(_chessBoard.GetFenPosition(), 3);

            }


            if (!string.IsNullOrWhiteSpace(_fromField[0]) && !string.IsNullOrWhiteSpace(_toField))
            {
                var fromField = _fromField[0];
                if (fromField.Equals(_toField) && !string.IsNullOrWhiteSpace(_fromField[1]))
                {
                    fromField = _fromField[1];
                }

                var fromFieldNumber = Fields.GetFieldNumber(fromField);
                var toFieldNumber = Fields.GetFieldNumber(_toField);
                var fromFieldColor = _chessBoard.GetFigureOn(fromFieldNumber).Color;
                var toFieldColor = _chessBoard.GetFigureOn(toFieldNumber).Color;

                if (fromFieldColor == _chessBoard.CurrentColor &&
                    (toFieldColor == Fields.COLOR_EMPTY || toFieldColor == _chessBoard.EnemyColor))
                {
                    if (_chessBoard.MoveIsValid(fromFieldNumber, toFieldNumber))
                    {
                        _logger?.LogDebug($"SQ: Move is valid. Make move for: {fromField} {_toField}");
                        _chessBoard.MakeMove(fromField, _toField);
                        _lastFromField = fromField;
                        _lastToField = _toField;
                        _fromField[0] = string.Empty;
                        _fromField[1] = string.Empty;
                        _toField = string.Empty;
                        var validFen = _chessBoard.GetFenPosition();
                        _logger?.LogDebug($"SQ: Return game fen: {validFen}");
                        return new DataFromBoard(validFen, 3);
                    }
                    else
                    {

                        _logger?.LogDebug($"SQ: Move is NOT valid. Set board for: {fromField} {_toField}");
                        var chessBoard = new ChessBoard();
                        chessBoard.Init(_chessBoard);
                        var chessFigure = chessBoard.GetFigureOn(fromFieldNumber);
                        chessBoard.RemoveFigureFromField(fromFieldNumber);
                        chessBoard.RemoveFigureFromField(toFieldNumber);
                        chessBoard.SetFigureOnPosition(chessFigure.FigureId, toFieldNumber);
                        chessBoard.CurrentColor = chessFigure.EnemyColor;
                        _fromField[0] = string.Empty;
                        _fromField[1] = string.Empty;
                        _toField = string.Empty;
                        var fen = chessBoard.GetFenPosition();
                        _logger?.LogDebug($"SQ: Return position fen: {fen}");
                        return new DataFromBoard(fen, 3);
                    }
                }

                _logger?.LogDebug($"SQ: Move is NOT valid. Set working board for: {fromField} {_toField}");
                var chessBoard2 = new ChessBoard();
                chessBoard2.Init(_chessBoard);
                var chessFigure2 = chessBoard2.GetFigureOn(fromFieldNumber);
                chessBoard2.RemoveFigureFromField(fromFieldNumber);
                chessBoard2.RemoveFigureFromField(toFieldNumber);
                chessBoard2.SetFigureOnPosition(chessFigure2.FigureId, toFieldNumber);
                chessBoard2.CurrentColor = chessFigure2.EnemyColor;
                _fromField[0] = string.Empty;
                _fromField[1] = string.Empty;
                _toField = string.Empty;
                var fen2 = chessBoard2.GetFenPosition();
                _logger?.LogDebug($"SQ: Return position fen: {fen2}");
                return new DataFromBoard(fen2, 3);

            }

            return new DataFromBoard(_chessBoard.GetFenPosition(), 3);
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return _dumpDataFromBoard;
        }

        protected override void SetToNewGame()
        {
            _chessBoard.Init();
            _chessBoard.NewGame();
            _serialCommunication.Send("14#1*");
            _serialCommunication.Send("4#*");
        }

     

        public override void Release()
        {
            //
        }

        public override void SetFen(string fen)
        {
            var setCommand = FIELD_CMD_PREFIX;
            _chessBoard.Init();
            _chessBoard.NewGame();
            _chessBoard.SetPosition(fen,true);
            foreach (var fieldOrder in _fieldOrders)
            {
                setCommand += _chessBoard.GetFigureOn(fieldOrder).GeneralFigureId == FigureId.NO_PIECE ? "0" : "1";
            }
            _serialCommunication.Send(setCommand);
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

        public override event EventHandler BasePositionEvent;
        public override event EventHandler NewGamePositionEvent;
        public override event EventHandler<string> DataEvent;
        public override event EventHandler HelpRequestedEvent;

        public override void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
        {
            //
        }

      
        public override void SpeedLeds(int level)
        {
            //throw new NotImplementedException();
        }
    }
}

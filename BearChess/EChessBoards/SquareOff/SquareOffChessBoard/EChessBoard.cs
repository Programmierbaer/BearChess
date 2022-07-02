using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.SquareOffChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly IChessBoard _chessBoard;
        private bool _withLeds = false;
        private string _lastSendFields = string.Empty;
        private string _prevRead = string.Empty;
        private readonly string[] _fromField = {string.Empty,string.Empty};
        private string _toField = string.Empty;
        private string _lastFromField = string.Empty;
        private string _lastToField = string.Empty;
      
        private readonly string _basePosition = "1100001111000011110000111100001111000011110000111100001111000011";

        private readonly Dictionary<byte, string> _fielByte2FieldName = new Dictionary<byte, string>()
                                                                        { { 0, "A1" }, { 1, "A2" }, { 2, "A3" }, { 3, "A4" }, { 4, "A5" }, { 5, "A6" }, { 6, "A7" }, { 7, "A8" },
                                                                            { 8, "B1" }, { 9, "B2" }, {10, "B3" }, {11, "B4" }, {12, "B5" }, {13, "B6" }, {14, "B7" }, {15, "B8" },
                                                                            {16, "C1" }, {17, "C2" }, {18, "C3" }, {19, "C4" }, {20, "C5" }, {21, "C6" }, {22, "C7" }, {23, "C8" },
                                                                            {24, "D1" }, {25, "D2" }, {26, "D3" }, {27, "D4" }, {28, "D5" }, {29, "D6" }, {30, "D7" }, {31, "D8" },
                                                                            {32, "E1" }, {33, "E2" }, {34, "E3" }, {35, "E4" }, {36, "E5" }, {37, "E6" }, {38, "E7" }, {39, "E8" },
                                                                            {40, "F1" }, {41, "F2" }, {42, "F3" }, {43, "F4" }, {44, "F5" }, {45, "F6" }, {46, "F7" }, {47, "F8" },
                                                                            {48, "G1" }, {49, "G2" }, {50, "G3" }, {51, "G4" }, {52, "G5" }, {53, "G6" }, {54, "G7" }, {55, "G8" },
                                                                            {56, "H1" }, {57, "H2" }, {58, "H3" }, {59, "H4" }, {60, "H5" }, {61, "H6" }, {62, "H7" }, {63, "H8" }
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

        public EChessBoard(string basePath, ILogging logger, bool isFirstInstance, string portName, bool useBluetooth, string boardName)
        {
            _withLeds = boardName.Equals(Constants.SquareOffPro);
            _isFirstInstance = isFirstInstance;
            _serialCommunication = new SerialCommunication(isFirstInstance, new FileLogger(Path.Combine(basePath, "log", $"SC_1.log"), 10, 10), portName, useBluetooth,boardName);

            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            Information = string.Empty;
            IsConnected = EnsureConnection();
            _serialCommunication.Send("14#1*");
            _serialCommunication.Send("4#*");
            //_serialCommunication.Send(_resetBoard);
            //_serialCommunication.Send(_startReading);
            //_serialCommunication.Send(_requestTrademark);
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            var requestDumpThread = new Thread(RequestADumpLoop) { IsBackground = true };
            requestDumpThread.Start();
        }

        private void RequestADumpLoop()
        {
            while (!_stopAll)
            {
                Thread.Sleep(1000);
                RequestDump();
            }
        }

        public EChessBoard(ILogging logger)
        {
            _isFirstInstance = true;
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            Information = string.Empty;
            _fromField[0] = string.Empty;
            _fromField[1] = string.Empty;
            _toField = string.Empty;
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

        public override void SetLedForFields(string[] fieldNames, bool thinking, bool isMove, string displayString)
        {
            
            if (fieldNames == null || fieldNames.Length == 0)
            {
                return;
            }
            var fieldNamesLength = fieldNames.Length;
            if (thinking && !_withLeds)
            {
                return;
            }
            if (fieldNamesLength != 2 && !_withLeds)
            {
                return;
            }
            //_serialCommunication.Send($"25#e2e4*");
            var sendFields = string.Join("", fieldNames);
            _logger?.LogDebug($"SQ: Request set LED for fields: {sendFields} Thinking: {thinking}");
            if (sendFields.Equals(_lastSendFields))
            {
                _logger?.LogDebug($"SQ: Ignored equals last send {_lastSendFields}");
                return;
            }
            if (thinking && fieldNamesLength > 1)
            {
                SetLedForFields(new[] { fieldNames[0] }, true, isMove, string.Empty);
                SetLedForFields(new[] { fieldNames[1] }, true, isMove, string.Empty);
                return;
            }
            
            _lastSendFields = sendFields;
            if (fieldNamesLength == 2 && _withLeds)
            {
                if (_lastFromField.Equals(fieldNames[0], StringComparison.OrdinalIgnoreCase) &&
                    _lastToField.Equals(fieldNames[1], StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogDebug($"SQ: Ignore set LED for fields: {_lastSendFields}: Equals last move {_lastFromField} {_lastToField}");
                    return;
                }
            
            }
            _logger?.LogDebug($"SQ: Set LED for fields: {_lastSendFields} Thinking: {thinking}");
            _serialCommunication.Send($"25#{sendFields.ToLower()}*");
            //_serialCommunication.Send("4#*");
            //throw new NotImplementedException();
        }

        public override void SetLastLeds()
        {
            //throw new NotImplementedException();
        }

        public override void SetAllLedsOff()
        {
           _serialCommunication.Send("25#*");
        }

        public override void SetAllLedsOn()
        {
            _serialCommunication.Send("25#a1a2a3a4a5a6a7a8b1b2b3b4b5b6b7b8c1c2c3c4c5c6c7c8d1d2d3d4d5d6d7d8e1e2e3e4e5e6e7e8f1f2f3f4f5f6f7f8g1g2g3g4g5g6g7g8h1h2h3h4h5h6h7h8*");
        }

        public override void DimLeds(bool dimLeds)
        {
            //throw new NotImplementedException();
        }

        public override void DimLeds(int level)
        {
            //throw new NotImplementedException();
        }

        public override void FlashSync(bool flashSync)
        {
            // throw new NotImplementedException();
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            //throw new NotImplementedException();
        }

        public override void Calibrate()
        {
            //throw new NotImplementedException();
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override void RequestDump()
        {
            _serialCommunication.Send("30#R*");
        }

        public override DataFromBoard GetPiecesFen()
        {
            ulong repeated = 0;
          
            while (true)
            {
                var dataFromBoard = _serialCommunication.GetFromBoard();
                if (!dataFromBoard.FromBoard.Equals(_prevRead))
                {
                    _logger?.LogDebug($"SQ: Read from board: {dataFromBoard.FromBoard}");
                    _prevRead = dataFromBoard.FromBoard;
                }

                repeated = dataFromBoard.Repeated;
                if (dataFromBoard.FromBoard.Length == 0)
                {
                    break;
                }

                if (dataFromBoard.FromBoard.StartsWith("0#"))
                {
                    var fieldName = dataFromBoard.FromBoard.Substring(2, 2);
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
                if (dataFromBoard.FromBoard.StartsWith("22#"))
                {
                    var fieldName = dataFromBoard.FromBoard.Substring(5, dataFromBoard.FromBoard.Length-5);
                    if (int.TryParse(fieldName.Replace("*", string.Empty), out int battery))
                    {
                        BatteryLevel = battery.ToString();
                        BatteryStatus = "🔋";
                    }

                    break;
                }
                if (dataFromBoard.FromBoard.StartsWith("30#"))
                {
                    var fieldName = dataFromBoard.FromBoard.Substring(3, dataFromBoard.FromBoard.Length - 3).Replace("*",string.Empty);
                    bool isBasePosition = fieldName.Equals(_basePosition);
                    List<string> dumpFields = new List<string>();
                      
                    for (byte i = 0; i < 64; i++)
                    {
                        var key = fieldName.Substring(i, 1);
                        if (key == "1")
                        {
                            dumpFields.Add(_fielByte2FieldName[i]);
                        }

                    }
                    return new DataFromBoard(string.Join(",", dumpFields), 3)
                           { IsFieldDump = true, BasePosition = isBasePosition };
                    
                }
                break;
            }
            var logMessage =
                $"SQ: Current: [{_fromField[0]}-{_toField}]   Last: [{_lastFromField}-{_lastToField}] ";
            if (!logMessage.Equals(_lastLogMessage))
            {
                _logger?.LogDebug(logMessage);
                _lastLogMessage = logMessage;
            }


            if (!_inDemoMode && _allowTakeBack && !string.IsNullOrWhiteSpace(_lastToField) && !string.IsNullOrWhiteSpace(_toField) &&
                _fromField[0].Equals(_lastToField) &&
                _toField.Equals(_lastFromField))
            {
                _logger?.LogInfo("SQ: Take move back. Replay all previous moves");
                var playedMoveList = _chessBoard.GetPlayedMoveList();
                string setCommand = "30#";
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
                string fromField = _fromField[0];
                if (fromField.Equals(_toField) && !string.IsNullOrWhiteSpace(_fromField[1]))
                {
                    fromField = _fromField[1];
                }
                var fromFieldNumber =  Fields.GetFieldNumber(fromField);
                var toFieldNumber = Fields.GetFieldNumber(_toField);
                var fromFieldColor = _chessBoard.GetFigureOn(fromFieldNumber).Color;
                var toFieldColor = _chessBoard.GetFigureOn(toFieldNumber).Color;
              
                if (fromFieldColor == _chessBoard.CurrentColor &&
                    (toFieldColor == Fields.COLOR_EMPTY || toFieldColor == _chessBoard.EnemyColor))
                {
                  
                    if (_chessBoard.MoveIsValid(fromFieldNumber, toFieldNumber))
                    {
                        _logger?.LogDebug($"SQ: MakePgnMove: {fromField} {_toField}");
                        _chessBoard.MakeMove(fromField, _toField);
                        _lastFromField = fromField;
                        _lastToField = _toField;
                        _fromField[0] = string.Empty;
                        _fromField[1] = string.Empty;
                        _toField = string.Empty;

                    }
                    else
                    {

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
                        return new DataFromBoard(chessBoard.GetFenPosition(), 3);
                    }
                }
            }
            return new DataFromBoard(_chessBoard.GetFenPosition(), 3);
        }

        protected override void SetToNewGame()
        {
            _chessBoard.Init();
            _chessBoard.NewGame();
            _serialCommunication.Send("14#1*");
            _serialCommunication.Send("4#*");
        }

        protected override void Release()
        {
            //
        }

        public override void SetFen(string fen)
        {
            string setCommand = "30#";
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

        public override void SetClock(int hourWhite, int minuteWhite, int minuteSec, int hourBlack, int minuteBlack, int secondBlack)
        {
            //
        }

        private void SendFen(string fen)
        {
            string setCommand = "30#";

            _chessBoard.Init();
            _chessBoard.NewGame();
            _chessBoard.SetPosition(fen, true);
            foreach (var fieldOrder in _fieldOrders)
            {
                setCommand += _chessBoard.GetFigureOn(fieldOrder).GeneralFigureId == FigureId.NO_PIECE ? "0" : "1";
            }
            _serialCommunication.Send(setCommand);
        }

        public override void SpeedLeds(int level)
        {
            //throw new NotImplementedException();
        }
    }
}

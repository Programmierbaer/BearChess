﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.PegasusChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly byte[] _allLEDsOff = { 96, 2, 0, 0 };
        private readonly byte[] _resetBoard = { 64 };
        private readonly byte[] _batteryState = { 76 };
        private readonly byte[] _startReading = { 68 };
        private readonly byte[] _requestTrademark = { 71 };
        private readonly byte[] _initialize = { 99, 7, 190, 245, 174, 221, 169, 95, 0 };


        private readonly Dictionary<string, byte> _fieldName2FieldByte = new Dictionary<string, byte>()
                                                { { "A8",  0 }, { "B8",  1 }, { "C8",  2 }, { "D8",  3 }, { "E8",  4 }, { "F8",  5 }, { "G8",  6 }, { "H8",  7 },
                                                  { "A7",  8 }, { "B7",  9 }, { "C7", 10 }, { "D7", 11 }, { "E7", 12 }, { "F7", 13 }, { "G7", 14 }, { "H7", 15 },
                                                  { "A6", 16 }, { "B6", 17 }, { "C6", 18 }, { "D6", 19 }, { "E6", 20 }, { "F6", 21 }, { "G6", 22 }, { "H6", 23 },
                                                  { "A5", 24 }, { "B5", 25 }, { "C5", 26 }, { "D5", 27 }, { "E5", 28 }, { "F5", 29 }, { "G5", 30 }, { "H5", 31 },
                                                  { "A4", 32 }, { "B4", 33 }, { "C4", 34 }, { "D4", 35 }, { "E4", 36 }, { "F4", 37 }, { "G4", 38 }, { "H4", 39 },
                                                  { "A3", 40 }, { "B3", 41 }, { "C3", 42 }, { "D3", 43 }, { "E3", 44 }, { "F3", 45 }, { "G3", 46 }, { "H3", 47 },
                                                  { "A2", 48 }, { "B2", 49 }, { "C2", 50 }, { "D2", 51 }, { "E2", 52 }, { "F2", 53 }, { "G2", 54 }, { "H2", 55 },
                                                  { "A1", 56 }, { "B1", 57 }, { "C1", 58 }, { "D1", 59 }, { "E1", 60 }, { "F1", 61 }, { "G1", 62 }, { "H1", 63 }
                                                 };

        private readonly Dictionary<byte, string> _fielByte2FieldName = new Dictionary<byte, string>()
                                              { { 0, "A8" }, { 1, "B8" }, { 2, "C8" }, { 3, "D8" }, { 4, "E8" }, { 5, "F8" }, { 6, "G8" }, { 7, "H8" },
                                                { 8, "A7" }, { 9, "B7" }, {10, "C7" }, {11, "D7" }, {12, "E7" }, {13, "F7" }, {14, "G7" }, {15, "H7" },
                                                {16, "A6" }, {17, "B6" }, {18, "C6" }, {19, "D6" }, {20, "E6" }, {21, "F6" }, {22, "G6" }, {23, "H6" },
                                                {24, "A5" }, {25, "B5" }, {26, "C5" }, {27, "D5" }, {28, "E5" }, {29, "F5" }, {30, "G5" }, {31, "H5" },
                                                {32, "A4" }, {33, "B4" }, {34, "C4" }, {35, "D4" }, {36, "E4" }, {37, "F4" }, {38, "G4" }, {39, "H4" },
                                                {40, "A3" }, {41, "B3" }, {42, "C3" }, {43, "D3" }, {44, "E3" }, {45, "F3" }, {46, "G3" }, {47, "H3" },
                                                {48, "A2" }, {49, "B2" }, {50, "C2" }, {51, "D2" }, {52, "E2" }, {53, "F2" }, {54, "G2" }, {55, "H2" },
                                                {56, "A1" }, {57, "B1" }, {58, "C1" }, {59, "D1" }, {60, "E1" }, {61, "F1" }, {62, "G1" }, {63, "H1" }
                                               };

        private readonly IChessBoard _chessBoard;

        string _fromField = string.Empty;
        string _toField = string.Empty;
        string _lastLine = string.Empty;
        private string _lastFromField = string.Empty;
        private string _lastToField = string.Empty;
        private string _tmpFromField = string.Empty;
        private string _tmpToField = string.Empty;
        private string _lastSendFields;
        private byte _currentSpeed = 3;
        private byte _currentTimes = 0;
        private byte _currentIntensity = 2;
        private string _lastLogMessage = string.Empty;

        public EChessBoard(string basePath, ILogging logger, bool isFirstInstance, string portName, bool useBluetooth)
        {
            _isFirstInstance = isFirstInstance;
            _serialCommunication = new SerialCommunication(isFirstInstance, logger, portName, useBluetooth);
            
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            Information = string.Empty;
            IsConnected = EnsureConnection();
            _serialCommunication.Send(_initialize);
            _serialCommunication.Send(_resetBoard);
            _serialCommunication.Send(_startReading);
            _serialCommunication.Send(_requestTrademark);
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();

        }

        public EChessBoard(ILogging logger)
        {
            _isFirstInstance = true;
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            Information = string.Empty;
        }


        public override bool CheckComPort(string portName)
        {
            return true;
        }

        public override void SetLedForFields(string[] fieldNames)
        {
            
            List<byte> allBytes = new List<byte>();
            var fieldNamesLength = fieldNames.Length;
            string sendFields = string.Join(" ", fieldNames);
            if (sendFields.Equals(_lastSendFields))
            {
                return;
            }

            _lastSendFields = sendFields;
            _logger?.LogDebug("PS: Set LED for fields: " + _lastSendFields);
           
            byte anzahl = byte.Parse((fieldNamesLength + 5).ToString());
            allBytes.Add(96);
            allBytes.Add(anzahl);
            allBytes.Add(5);
            allBytes.Add(_currentSpeed); // Speed
            allBytes.Add(_currentTimes); // Times
            allBytes.Add(_currentIntensity); // Intensity
            foreach (var fieldName in fieldNames)
            {
                allBytes.Add(_fieldName2FieldByte[fieldName.ToUpper()]);
            }
            allBytes.Add(0);
            _serialCommunication.Send(allBytes.ToArray());
            _serialCommunication.Send(_batteryState);
            if (string.IsNullOrWhiteSpace(Information))
            {
                _serialCommunication.Send(_requestTrademark);
            }
        }

        public override void SetLastLeds()
        {
            //
        }

        public override void SetAllLedsOff()
        {
            _serialCommunication.Send(_allLEDsOff);
            if (string.IsNullOrWhiteSpace(Information))
            {
                _serialCommunication.Send(_requestTrademark);
            }
        }

        public override void SetAllLedsOn()
        {
            byte currentSpeed = _currentSpeed;
            byte currentTimes = _currentTimes;
            byte currentIntensity = _currentIntensity;
            _currentSpeed = 10;
            _currentTimes = 3;
            _currentIntensity = 1;
            SetLedForFields(new string[]
                            {
                                "A1","H1","H8","A8"
                            });
            Thread.Sleep(3000);
            _currentSpeed = currentSpeed;
            _currentTimes = currentTimes; 
            _currentIntensity = currentIntensity;
        }

        public override void DimLeds(bool dimLeds)
        {
            _currentIntensity = dimLeds ? (byte)4 : (byte)1;
        }

        public override void DimLeds(int level)
        {
            _currentIntensity = (byte)level;
        }

        public override void SpeedLeds(int level)
        {
            _currentSpeed = (byte)level;
        }

        public override void FlashSync(bool flashSync)
        {
            //
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            //
        }

        public override void Calibrate()
        {
            //
        }

        public override DataFromBoard GetPiecesFen()
        {

            while (true)
            {
                var dataFromBoard = _serialCommunication.GetFromBoard();
                if (_ignoreReading)
                {
                    _fromField = string.Empty;
                    _toField = string.Empty;
                    _lastFromField = string.Empty;
                    _lastToField = string.Empty;
                    _tmpFromField = string.Empty;
                    _tmpToField = string.Empty;
                    if (!string.IsNullOrWhiteSpace(dataFromBoard.FromBoard))
                    {
                        var strings =
                            dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (strings.Length > 3)
                        {
                            if (strings[0] == "146")
                            {
                                string trademark = string.Empty;
                                for (int i = 3; i < strings.Length; i++)
                                {
                                    trademark += Encoding.UTF8.GetString(new byte[] { byte.Parse(strings[i]) });
                                }

                                Information = trademark;
                            }
                        }
                    }

                    return new DataFromBoard(_chessBoard.GetFenPosition(), 3);
                }
                if (!string.IsNullOrWhiteSpace(dataFromBoard.FromBoard) && dataFromBoard.Repeated==0)
                {
                    var strings = dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (strings.Length == 5)
                    {
                        if (strings[0] == "142" && strings[1] == "0" && strings[2] == "5")
                        {
                            if (strings[4] == "0")
                            {
                                var fromField = _fielByte2FieldName[byte.Parse(strings[3])];
                                _logger?.LogDebug($"PS: Read Fromfield: {fromField}");
                                if (fromField.Equals(_lastToField))
                                {
                                    _tmpToField = fromField;
                                }
                                if (_inDemoMode)
                                {
                                    _fromField = fromField;
                                }
                                else
                                {
                                    if (_chessBoard.GetFigureOn(Fields.GetFieldNumber(fromField)).Color ==
                                        _chessBoard.CurrentColor)
                                    {
                                        _fromField = fromField;
                                    }
                                }

                            }
                            else
                            {
                                _toField = _fielByte2FieldName[byte.Parse(strings[3])];
                                _logger?.LogDebug($"PS: Read ToField: {_toField}");
                                if (_toField.Equals(_lastFromField))
                                {
                                    _tmpFromField = _toField;
                                }
                                if (!_inDemoMode && _chessBoard.GetFigureOn(Fields.GetFieldNumber(_toField)).Color ==
                                    _chessBoard.CurrentColor)
                                {
                                    _toField = string.Empty;
                                }
                            }
                        }
                    }

                    if (strings.Length == 12)
                    {
                        if (strings[0] == "160" && strings[1] == "0" && strings[2] == "12")
                        {
                            BatteryLevel = strings[3];
                            BatteryStatus =  strings[11].Equals("1") ? "🔋" : "";
                        }
                    }
                    if (strings.Length > 3)
                    {
                        if (strings[0] == "146" )
                        {
                            string trademark = string.Empty;
                            for (int i = 3; i < strings.Length; i++)
                            {
                                trademark += Encoding.UTF8.GetString(new byte[] { byte.Parse(strings[i]) });
                            }

                            Information = trademark;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(_fromField) && !string.IsNullOrWhiteSpace(_toField))
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(_tmpFromField) && !string.IsNullOrWhiteSpace(_tmpToField))
                    {
                        if (_allowTakeBack)
                        {
                            break;
                        }
                    }

                    if (dataFromBoard.FromBoard.Equals(_lastLine))
                    {
                        break;
                    }

                    _lastLine = dataFromBoard.FromBoard;
                }
                else
                {
                    break;
                }
            }

            var logMessage = $"PS: Current: [{_fromField}-{_toField}]   Last: [{_lastFromField}-{_lastToField}]  Temp: [{_tmpFromField}- {_tmpToField}]";
            if (!logMessage.Equals(_lastLogMessage))
            {
                _logger?.LogDebug(logMessage);
                _lastLogMessage =logMessage;
            }

            if (!_inDemoMode && _allowTakeBack && !string.IsNullOrWhiteSpace(_tmpFromField) && !string.IsNullOrWhiteSpace(_tmpToField))
            {
                _logger?.LogInfo("PS: Take move back. Replay all previous moves");
                var playedMoveList = _chessBoard.GetPlayedMoveList();
                _chessBoard.Init();
                _chessBoard.NewGame();
                for (int i = 0; i < playedMoveList.Length - 1; i++)
                {
                    _logger?.LogDebug($"PS: Move {playedMoveList[i]}");
                    _chessBoard.MakeMove(playedMoveList[i]);
                    _lastFromField = playedMoveList[i].FromFieldName.ToUpper();
                    _lastToField = playedMoveList[i].ToFieldName.ToUpper();
                }
                _fromField = string.Empty;
                _toField = string.Empty;
                _tmpFromField = string.Empty;
                _tmpToField = string.Empty;
            }
            if (!string.IsNullOrWhiteSpace(_fromField) && !string.IsNullOrWhiteSpace(_toField))
            {
                _logger?.LogDebug($"PS: Read move: Fromfield: {_fromField}  ToField: {_toField}");
                var fromFieldNumber = Fields.GetFieldNumber(_fromField);
                var toFieldNumber = Fields.GetFieldNumber(_toField);
                var fromFieldColor = _chessBoard.GetFigureOn(fromFieldNumber).Color;
                var toFieldColor = _chessBoard.GetFigureOn(toFieldNumber).Color;
                var switchedFromColor = toFieldColor;
                var switchedToColor = fromFieldColor;
                var switchFromField = _toField;
                var switchToField = _fromField;
                if (_inDemoMode)
                {
                    var chessFigure = _chessBoard.GetFigureOn(fromFieldNumber);
                    _chessBoard.RemoveFigureFromField(fromFieldNumber);
                    _chessBoard.RemoveFigureFromField(toFieldNumber);
                    _chessBoard.SetFigureOnPosition(chessFigure.FigureId, toFieldNumber);
                    _chessBoard.CurrentColor = chessFigure.EnemyColor;
                    _fromField = string.Empty;
                    _toField = string.Empty;
                    _lastFromField = string.Empty;
                    _lastToField = string.Empty;
                    _tmpFromField = string.Empty;
                    _tmpToField = string.Empty;

                }
                else
                {
                    if (fromFieldColor == _chessBoard.CurrentColor &&
                        (toFieldColor == Fields.COLOR_EMPTY || toFieldColor == _chessBoard.EnemyColor))
                    {
                        if (_chessBoard.MoveIsValid(fromFieldNumber, toFieldNumber))
                        {
                            _logger?.LogDebug($"PS: MakeMove: {_fromField} {_toField}");
                            _chessBoard.MakeMove(_fromField, _toField);
                            _lastFromField = _fromField;
                            _lastToField = _toField;
                            _fromField = string.Empty;
                            _toField = string.Empty;
                            _tmpFromField = string.Empty;
                            _tmpToField = string.Empty;
                        }
                    }
                    else
                    {
                        if (switchedFromColor == _chessBoard.CurrentColor &&
                            (toFieldColor == Fields.COLOR_EMPTY || switchedToColor == _chessBoard.EnemyColor))
                        {
                            if (_chessBoard.MoveIsValid(Fields.GetFieldNumber(switchFromField),
                                                        Fields.GetFieldNumber(switchToField)))
                            {
                                _logger?.LogDebug(
                                    $"PS: Switched MakeMove: {switchFromField} {switchToField}");
                                _chessBoard.MakeMove(switchFromField, switchToField);
                                _fromField = string.Empty;
                                _toField = string.Empty;
                            }
                        }
                    }
                }
            }
            return new DataFromBoard(_chessBoard.GetFenPosition(), 3);
        }

        protected override void SetToNewGame()
        {
            _chessBoard.Init();
            _chessBoard.NewGame();
            _serialCommunication.Send(_resetBoard);
            _serialCommunication.Send(_allLEDsOff);
            _serialCommunication.Send(_startReading);
            _serialCommunication.Send(_batteryState);
        }

        public override void SetFen(string fen)
        {
            _chessBoard.Init();
            _chessBoard.SetPosition(fen);
        }
    }
}
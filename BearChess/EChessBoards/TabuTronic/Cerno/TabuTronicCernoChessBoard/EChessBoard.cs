using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.ChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly bool _useBluetooth;

        private readonly ICalibrateStorage _calibrateStorage;

        private readonly Dictionary<string, string> _boardCodesToChessPiece = new Dictionary<string, string>();

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

        private string _prevJoinedString = string.Empty;
        private int _prevLedField = 0;


        private readonly Dictionary<string, int> unKnowCodeCounter = new Dictionary<string, int>();
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
        private ConcurrentQueue<string[]> _flashFields = new ConcurrentQueue<string[]>();
        private bool _release = false;


        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth)
        {
            _useBluetooth = useBluetooth;
            _serialCommunication = new SerialCommunication(logger, portName, useBluetooth);
            _calibrateStorage = new CalibrateStorage(basePath);
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            PieceRecognition = true;
            SelfControlled = false;
            var calibrationData = _calibrateStorage.GetCalibrationData();
            if (!string.IsNullOrWhiteSpace(calibrationData.BasePositionCodes))
            {
                _logger?.LogDebug("B: Stored calibration data available");
                IsCalibrated = Calibrate(calibrationData);
            }
            IsConnected = EnsureConnection();
            Information = Constants.TabutronicCerno;
            var thread = new Thread(FlashLeds) { IsBackground = true };
            thread.Start();
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            Information = Constants.TabutronicCerno;
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

            return false;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return CheckComPort(portName);
        }

        private void FlashLeds()
        {
            bool switchSide = false;
            while (!_release)
            {
                if (_flashFields.TryPeek(out string[] fields))
                {
                    byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                    if (switchSide)
                    {
                        result = UpdateLedsForField(fields[1], result);
                        result = UpdateLedsForField(fields[1], result);
                    }
                    else
                    {
                        result = UpdateLedsForField(fields[0], result);
                        result = UpdateLedsForField(fields[0], result);
                    }
                    switchSide = !switchSide;
                    _serialCommunication.Send(result);
                    Thread.Sleep(500);

                }

                Thread.Sleep(10);
            }
        }


        public override void SetLedForFields(string[] fieldNames, string promote, bool thinking, bool isMove, string displayString)
        {
            if (!EnsureConnection())
            {
                return;
            }
            _flashFields.TryDequeue(out _);
            if (fieldNames.Length < 2)
            {
                return;
            }
            lock (_locker)
            {
                var joinedString = string.Join(" ", fieldNames);
                if (thinking)
                {
                    _prevLedField = _prevLedField == 1 ? 0 : 1;
                }
                else
                {
                    if (_prevJoinedString.Equals(joinedString))
                    {
                        return;
                    }
                }
                _logger?.LogDebug($"B: set leds for {joinedString}");
                if (thinking && fieldNames.Length > 1)
                {
                    _flashFields.Enqueue(fieldNames);
                    return;
                }
                
                _prevJoinedString = joinedString;

                byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
                Array.Copy(AllOff, result, AllOff.Length);
                if (thinking && fieldNames.Length == 2)
                {
                    result = UpdateLedsForField(fieldNames[_prevLedField], result);
                }
                else
                {
                    foreach (string fieldName in fieldNames)
                    {
                        result = UpdateLedsForField(fieldName, result);
                    }
                }

                Array.Copy(result, _lastSendBytes, result.Length);
                _serialCommunication.Send(result);
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

            lock (_locker)
            {
                _logger?.LogDebug("B: Send all off");
                _serialCommunication.ClearToBoard();
                _serialCommunication.Send(AllOff);
                _prevJoinedString = string.Empty;
            }
        }

        public override void SetAllLedsOn()
        {
            if (!EnsureConnection())
            {
                return;
            }

            lock (_locker)
            {
                _logger?.LogDebug("B: Send all on");
                _serialCommunication.Send(AllOn);
            }
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
            // ignore
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        public override void FlashMode(EnumFlashMode flashMode)
        {
            _flashLeds = flashMode==EnumFlashMode.FlashSync;
        }

     
        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            // ignore
        }

        public override void Calibrate()
        {
            if (!EnsureConnection())
            {
                return;
            }

            _logger?.LogDebug("B: start calibrate ");
            SetLedForFields(new[] { "A1", "B1", "C1", "D1", "E1", "F1", "G1", "H1", "A2", "B2", "C2", "D2", "E2", "F2", "G2", "H2", "A8", "B8", "C8", "D8", "E8", "F8", "G8", "H8", "A7", "B7", "C7", "D7", "E7", "F7", "G7", "H7", "D3", "D6" }, string.Empty, false, false, string.Empty);
            var boardData = _serialCommunication.GetCalibrateData();
            _logger?.LogDebug($"B: calibrate data: {boardData}");
            if (!Calibrate(boardData))
            {
                IsCalibrated = false;
                return;
            }

            var calibrateData = new CalibrateData();
            foreach (var key in _boardCodesToChessPiece.Keys)
            {
                if (_boardCodesToChessPiece[key].Equals(FenCodes.BlackQueen))
                {
                    calibrateData.BlackQueenCodes = string.IsNullOrEmpty(calibrateData.BlackQueenCodes) ? key : calibrateData.BlackQueenCodes + '#' + key;
                    boardData = boardData.Replace($"0 {key} 0", "0 0 0 0 0 0 0");
                    continue;
                }
                if (_boardCodesToChessPiece[key].Equals(FenCodes.WhiteQueen))
                {
                    calibrateData.WhiteQueenCodes = string.IsNullOrEmpty(calibrateData.WhiteQueenCodes) ? key : calibrateData.WhiteQueenCodes + '#' + key;
                    boardData = boardData.Replace($"0 {key} 0", "0 0 0 0 0 0 0");
                }
            }
            calibrateData.BasePositionCodes = boardData;
            _calibrateStorage.SaveCalibrationData(calibrateData);
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

        private string GetPiecesFen(string[] dataArray)
        {
            var codes = new string[40];
            var fenLine = string.Empty;
            string[] unknownCodes;
            if (_playWithWhite)
            {
                Array.Copy(dataArray, 0, codes, 0, 40);
                fenLine = GetFenLine(codes, out unknownCodes);
                if (unknownCodes.Length == 1)
                {
                    if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                    {
                        unKnowCodeCounter[unknownCodes[0]]++;
                    }
                    else
                    {
                        unKnowCodeCounter[unknownCodes[0]] = 1;
                    }
                    var calibrationData = _calibrateStorage.GetCalibrationData();
                    if (!calibrationData.WhiteQueenCodes.Contains("#"))
                    {
                        if (unKnowCodeCounter[unknownCodes[0]] > 10)
                        {
                            _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.WhiteQueen;
                            calibrationData.WhiteQueenCodes += "#" + unknownCodes[0];
                            _calibrateStorage.SaveCalibrationData(calibrationData);
                            unKnowCodeCounter.Clear();
                            _logger?.LogDebug($"Add new white queen code: {unknownCodes[0]}");
                        }
                    }
                }
                Array.Copy(dataArray, 40, codes, 0, 40);
                fenLine += GetFenLine(codes, out _);
                Array.Copy(dataArray, 80, codes, 0, 40);
                fenLine += GetFenLine(codes, out _);
                Array.Copy(dataArray, 120, codes, 0, 40);
                fenLine += GetFenLine(codes, out _);
                Array.Copy(dataArray, 160, codes, 0, 40);
                fenLine += GetFenLine(codes, out _);
                Array.Copy(dataArray, 200, codes, 0, 40);
                fenLine += GetFenLine(codes, out _);
                Array.Copy(dataArray, 240, codes, 0, 40);
                fenLine += GetFenLine(codes, out _);
                Array.Copy(dataArray, 280, codes, 0, 40);
                fenLine += GetFenLine(codes, out unknownCodes).Replace("/", string.Empty);
                if (unknownCodes.Length == 1)
                {
                    if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                    {
                        unKnowCodeCounter[unknownCodes[0]]++;
                    }
                    else
                    {
                        unKnowCodeCounter[unknownCodes[0]] = 1;
                    }
                    var calibrationData = _calibrateStorage.GetCalibrationData();
                    if (!calibrationData.BlackQueenCodes.Contains("#"))
                    {
                        if (unKnowCodeCounter[unknownCodes[0]] > 10)
                        {
                            _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.BlackQueen;
                            calibrationData.BlackQueenCodes += "#" + unknownCodes[0];
                            _calibrateStorage.SaveCalibrationData(calibrationData);
                            unKnowCodeCounter.Clear();
                            _logger?.LogDebug($"Add new black queen code: {unknownCodes[0]}");
                        }
                    }
                }
            }
            else
            {
                try
                {
                    Array.Copy(dataArray, 280, codes, 0, 40);
                    fenLine = GetFenLine(codes, out unknownCodes);
                    if (unknownCodes.Length == 1)
                    {
                        if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                        {
                            unKnowCodeCounter[unknownCodes[0]]++;
                        }
                        else
                        {
                            unKnowCodeCounter[unknownCodes[0]] = 1;
                        }
                        var calibrationData = _calibrateStorage.GetCalibrationData();
                        if (!calibrationData.WhiteQueenCodes.Contains("#"))
                        {
                            if (unKnowCodeCounter[unknownCodes[0]] > 10)
                            {
                                _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.WhiteQueen;
                                calibrationData.WhiteQueenCodes += "#" + unknownCodes[0];
                                _calibrateStorage.SaveCalibrationData(calibrationData);
                                unKnowCodeCounter.Clear();
                                _logger?.LogDebug($"Add new white queen code: {unknownCodes[0]}");
                            }
                        }
                    }
                    Array.Copy(dataArray, 240, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 200, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 160, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 120, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 80, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 40, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 0, codes, 0, 40);
                    fenLine += GetFenLine(codes, out unknownCodes).Replace("/", string.Empty);
                    if (unknownCodes.Length == 1)
                    {
                        if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                        {
                            unKnowCodeCounter[unknownCodes[0]]++;
                        }
                        else
                        {
                            unKnowCodeCounter[unknownCodes[0]] = 1;
                        }
                        var calibrationData = _calibrateStorage.GetCalibrationData();
                        if (!calibrationData.BlackQueenCodes.Contains("#"))
                        {
                            if (unKnowCodeCounter[unknownCodes[0]] > 10)
                            {
                                _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.BlackQueen;
                                calibrationData.BlackQueenCodes += "#" + unknownCodes[0];
                                _calibrateStorage.SaveCalibrationData(calibrationData);
                                unKnowCodeCounter.Clear();
                                _logger?.LogDebug($"Add new black queen code: {unknownCodes[0]}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"B: GetPiecesFen: {ex.Message} ");
                }
            }

            return fenLine.Contains(UnknownPieceCode) ? UnknownPieceCode : fenLine;
        }


        public override DataFromBoard GetPiecesFen()
        {
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }

            lock (_locker)
            {

                DataFromBoard boardData = null;
                while (true)
                {
                    boardData = _serialCommunication.GetFromBoard();
                    if (boardData.FromBoard.Trim().Length > 1)
                    {
                        break;

                    }
                    Thread.Sleep(5);
                }

                //var dataArray = boardData.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var dataArray = boardData.FromBoard.Replace('\0', ' ').Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var retries = 0;
                while (retries < 10)
                {
                    if (dataArray.Length < 320)
                    {
                        boardData = _serialCommunication.GetFromBoard();
                        dataArray = boardData.FromBoard.Replace('\0', ' ').Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        retries++;
                        continue;
                    }

                    break;
                }

                if (dataArray.Length < 320)
                {
                    return new DataFromBoard(UnknownPieceCode, boardData.Repeated);
                }

                var codes = new string[40];
                var fenLine = string.Empty;
                string[] unknownCodes;
                if (string.Join(" ", dataArray).Contains("0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0"))
                {
                    bool playWithWhite = _playWithWhite;
                    _playWithWhite = true;
                    var piecesFen = GetPiecesFen(dataArray);
                    if (FenCodes.BasePosition.StartsWith(piecesFen))
                    {

                        return new DataFromBoard(piecesFen.Contains(UnknownPieceCode) ? UnknownPieceCode : piecesFen,
                                                 boardData.Repeated);
                    }
                    _playWithWhite = false;
                    piecesFen = GetPiecesFen(dataArray);
                    if (FenCodes.BasePosition.StartsWith(piecesFen))
                    {

                        return new DataFromBoard(piecesFen.Contains(UnknownPieceCode) ? UnknownPieceCode : piecesFen,
                                                 boardData.Repeated);
                    }

                    _playWithWhite = playWithWhite;
                }
                if (_playWithWhite)
                {
                    Array.Copy(dataArray, 0, codes, 0, 40);
                    fenLine = GetFenLine(codes, out unknownCodes);
                    if (unknownCodes.Length == 1)
                    {
                        if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                        {
                            unKnowCodeCounter[unknownCodes[0]]++;
                        }
                        else
                        {
                            unKnowCodeCounter[unknownCodes[0]] = 1;
                        }
                        var calibrationData = _calibrateStorage.GetCalibrationData();
                        if (!calibrationData.WhiteQueenCodes.Contains("#"))
                        {
                            if (unKnowCodeCounter[unknownCodes[0]] > 10)
                            {
                                _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.WhiteQueen;
                                calibrationData.WhiteQueenCodes += "#" + unknownCodes[0];
                                _calibrateStorage.SaveCalibrationData(calibrationData);
                                unKnowCodeCounter.Clear();
                                _logger?.LogDebug($"Add new white queen code: {unknownCodes[0]}");
                            }
                        }
                    }
                    Array.Copy(dataArray, 40, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 80, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 120, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 160, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 200, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 240, codes, 0, 40);
                    fenLine += GetFenLine(codes, out _);
                    Array.Copy(dataArray, 280, codes, 0, 40);
                    fenLine += GetFenLine(codes, out unknownCodes).Replace("/", string.Empty);
                    if (unknownCodes.Length == 1)
                    {
                        if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                        {
                            unKnowCodeCounter[unknownCodes[0]]++;
                        }
                        else
                        {
                            unKnowCodeCounter[unknownCodes[0]] = 1;
                        }
                        var calibrationData = _calibrateStorage.GetCalibrationData();
                        if (!calibrationData.BlackQueenCodes.Contains("#"))
                        {
                            if (unKnowCodeCounter[unknownCodes[0]] > 10)
                            {
                                _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.BlackQueen;
                                calibrationData.BlackQueenCodes += "#" + unknownCodes[0];
                                _calibrateStorage.SaveCalibrationData(calibrationData);
                                unKnowCodeCounter.Clear();
                                _logger?.LogDebug($"Add new black queen code: {unknownCodes[0]}");
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        Array.Copy(dataArray, 280, codes, 0, 40);
                        fenLine = GetFenLine(codes, out unknownCodes);
                        if (unknownCodes.Length == 1)
                        {
                            if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                            {
                                unKnowCodeCounter[unknownCodes[0]]++;
                            }
                            else
                            {
                                unKnowCodeCounter[unknownCodes[0]] = 1;
                            }
                            var calibrationData = _calibrateStorage.GetCalibrationData();
                            if (!calibrationData.WhiteQueenCodes.Contains("#"))
                            {
                                if (unKnowCodeCounter[unknownCodes[0]] > 10)
                                {
                                    _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.WhiteQueen;
                                    calibrationData.WhiteQueenCodes += "#" + unknownCodes[0];
                                    _calibrateStorage.SaveCalibrationData(calibrationData);
                                    unKnowCodeCounter.Clear();
                                    _logger?.LogDebug($"Add new white queen code: {unknownCodes[0]}");
                                }
                            }
                        }
                        Array.Copy(dataArray, 240, codes, 0, 40);
                        fenLine += GetFenLine(codes, out _);
                        Array.Copy(dataArray, 200, codes, 0, 40);
                        fenLine += GetFenLine(codes, out _);
                        Array.Copy(dataArray, 160, codes, 0, 40);
                        fenLine += GetFenLine(codes, out _);
                        Array.Copy(dataArray, 120, codes, 0, 40);
                        fenLine += GetFenLine(codes, out _);
                        Array.Copy(dataArray, 80, codes, 0, 40);
                        fenLine += GetFenLine(codes, out _);
                        Array.Copy(dataArray, 40, codes, 0, 40);
                        fenLine += GetFenLine(codes, out _);
                        Array.Copy(dataArray, 0, codes, 0, 40);
                        fenLine += GetFenLine(codes, out unknownCodes).Replace("/", string.Empty);
                        if (unknownCodes.Length == 1)
                        {
                            if (unKnowCodeCounter.ContainsKey(unknownCodes[0]))
                            {
                                unKnowCodeCounter[unknownCodes[0]]++;
                            }
                            else
                            {
                                unKnowCodeCounter[unknownCodes[0]] = 1;
                            }
                            var calibrationData = _calibrateStorage.GetCalibrationData();
                            if (!calibrationData.BlackQueenCodes.Contains("#"))
                            {
                                if (unKnowCodeCounter[unknownCodes[0]] > 10)
                                {
                                    _boardCodesToChessPiece[unknownCodes[0]] = FenCodes.BlackQueen;
                                    calibrationData.BlackQueenCodes += "#" + unknownCodes[0];
                                    _calibrateStorage.SaveCalibrationData(calibrationData);
                                    unKnowCodeCounter.Clear();
                                    _logger?.LogDebug($"Add new black queen code: {unknownCodes[0]}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"B: GetPiecesFen: {ex.Message} ");
                    }
                }

                return new DataFromBoard(fenLine.Contains(UnknownPieceCode) ? UnknownPieceCode : fenLine,
                    boardData.Repeated);
            }
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return GetPiecesFen();
        }

        protected override void SetToNewGame()
        {
            //
        }


        public override void Release()
        {
            _release = true;
        }

        public override void SetFen(string fen)
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

        public override event EventHandler BasePositionEvent;
        public override event EventHandler<string> DataEvent;
        public override event EventHandler HelpRequestedEvent;

        public override void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
        {
            //
        }


        #region private

      

        private bool Calibrate(CalibrateData codes)
        {
            if (!Calibrate(codes.BasePositionCodes))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(codes.BlackQueenCodes))
            {
                foreach (var key in _boardCodesToChessPiece.Keys)
                {
                    if (_boardCodesToChessPiece[key].Equals(FenCodes.BlackQueen))
                    {
                        codes.BlackQueenCodes = key;
                    }

                    if (_boardCodesToChessPiece[key].Equals(FenCodes.WhiteQueen))
                    {
                        codes.WhiteQueenCodes = key;
                    }
                }
                _calibrateStorage.SaveCalibrationData(codes);
            }
            else
            {
                var queenCodes = codes.BlackQueenCodes.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var queenCode in queenCodes)
                {
                    _boardCodesToChessPiece[queenCode] = FenCodes.BlackQueen;
                }
                queenCodes = codes.WhiteQueenCodes.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var queenCode in queenCodes)
                {
                    _boardCodesToChessPiece[queenCode] = FenCodes.WhiteQueen;
                }
            }
            return true;

        }

        private bool Calibrate(string codes)
        {
            var dataArray = codes.Replace('\0', ' ').Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length < 320)
            {
                _logger?.LogWarning($"B: Calibrate failed: at least 320 codes expected: {dataArray.Length}");
                return false;
            }
            string[] code = new string[5];
            _boardCodesToChessPiece.Clear();
            _boardCodesToChessPiece["0 0 0 0 0"] = string.Empty;
            Array.Copy(dataArray, 0, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackRook;
            Array.Copy(dataArray, 5, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackKnight;
            Array.Copy(dataArray, 10, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackBishop;
            Array.Copy(dataArray, 15, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackQueen;
            Array.Copy(dataArray, 20, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackKing;
            Array.Copy(dataArray, 25, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackBishop;
            Array.Copy(dataArray, 30, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackKnight;
            Array.Copy(dataArray, 35, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackRook;
            for (int i = 40; i < 80; i += 5)
            {
                Array.Copy(dataArray, i, code, 0, 5);
                _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackPawn;
            }

            Array.Copy(dataArray, 95, code, 0, 5);
            if (!code.All(f => f.Equals("0")))
            {
                _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.BlackQueen;
            }

            Array.Copy(dataArray, 215, code, 0, 5);
            if (!code.All(f => f.Equals("0")))
            {
                _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteQueen;
            }

            for (int i = 240; i < 280; i += 5)
            {
                Array.Copy(dataArray, i, code, 0, 5);
                _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhitePawn;
            }

            Array.Copy(dataArray, 280, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteRook;
            Array.Copy(dataArray, 285, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteKnight;
            Array.Copy(dataArray, 290, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteBishop;
            Array.Copy(dataArray, 295, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteQueen;
            Array.Copy(dataArray, 300, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteKing;
            Array.Copy(dataArray, 305, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteBishop;
            Array.Copy(dataArray, 310, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteKnight;
            Array.Copy(dataArray, 315, code, 0, 5);
            _boardCodesToChessPiece[string.Join(" ", code)] = FenCodes.WhiteRook;
            return true;
        }

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

        private string GetFenLine(string[] codes, out string[] unknownCodes)
        {
            var line = string.Empty;
            var noFigureCounter = 0;
            var code = new string[5];
            List<string> unknown = new List<string>();
            if (!_playWithWhite)
            {
                // _logger?.LogDebug($"codes: {string.Join(" ", codes)}");
                for (int i = 35; i >= 0; i -= 5)
                {
                    Array.Copy(codes, i, code, 0, 5);
                    // _logger?.LogDebug($"code: {string.Join(" ", code)}");
                    var pieceFromCode = GetPieceFenFromCode(string.Join(" ", code));
                    if (pieceFromCode.Equals(UnknownPieceCode))
                    {
                        unknown.Add(string.Join(" ", code));
                    }
                    if (string.IsNullOrWhiteSpace(pieceFromCode))
                    {
                        noFigureCounter++;
                    }
                    else
                    {
                        if (noFigureCounter > 0)
                        {
                            line += noFigureCounter;
                        }
                        noFigureCounter = 0;
                        line += pieceFromCode;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 40; i += 5)
                {
                    Array.Copy(codes, i, code, 0, 5);
                    var pieceFromCode = GetPieceFenFromCode(string.Join(" ", code));
                    if (pieceFromCode.Equals(UnknownPieceCode))
                    {
                        unknown.Add(string.Join(" ", code));
                    }
                    if (string.IsNullOrWhiteSpace(pieceFromCode))
                    {
                        noFigureCounter++;
                    }
                    else
                    {
                        if (noFigureCounter > 0)
                        {
                            line += noFigureCounter;
                        }

                        noFigureCounter = 0;
                        line += pieceFromCode;
                    }
                }
            }

            if (noFigureCounter > 0)
            {
                line += noFigureCounter;
            }

            unknownCodes = unknown.ToArray();
            return line + "/";
        }

        private string GetPieceFenFromCode(string code)
        {
            if (!EnsureConnection())
            {
                return string.Empty;
            }

            if (_boardCodesToChessPiece.ContainsKey(code))
            {
                return _boardCodesToChessPiece[code];
            }
            return string.Empty;
        }

        #endregion

    }
}

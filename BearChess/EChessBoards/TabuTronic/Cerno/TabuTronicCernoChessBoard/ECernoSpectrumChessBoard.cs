using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.ChessBoard
{
    public class ECernoSpectrumChessBoard : AbstractEBoard
    {
        private readonly bool _useBluetooth;

        private readonly ICalibrateStorage _calibrateStorage;

        private readonly Dictionary<string, string> _boardCodesToChessPiece = new Dictionary<string, string>();

        private int _currentColor;
        private int _prevColor;
        private int _currentEvalColor;
        private string _currentEval = "0";

        private readonly byte[] _lastSendBytes = Enumerable.Repeat<byte>(0, 243).ToArray();
     
        private static readonly byte[] AllOff = Enumerable.Repeat<byte>(0, 247).ToArray();
        private static readonly byte[] AllOn = Enumerable.Repeat<byte>(125, 247).ToArray();
        private static readonly byte[] FieldArray = Enumerable.Repeat<byte>(0, 243).ToArray();
        private static readonly byte[] SendArray = Enumerable.Repeat<byte>(0, 247).ToArray();
        private bool _release = false;
        private readonly bool _showMoveLine;
        private readonly bool _showEvaluationValue;
        private readonly bool _showCurrentColor;
        private readonly EChessBoardConfiguration _boardConfiguration;
        private readonly Dictionary<int, List<int>> _whiteFieldNamesToLedArray = new Dictionary<int, List<int>>();
        private readonly Dictionary<int, List<int>> _blackFieldNamesToLedArray = new Dictionary<int, List<int>>();
        private static readonly byte _whiteOnMoveFieldByte = 1;
        private static readonly byte _blackOnMoveFieldByte = 73;
        private static readonly byte[] _whiteAdvantageFieldBytes = { 9, 8, 7, 6, 5, 4, 3, 2 };
        private static readonly byte[] _blackAdvantageFieldBytes = { 81, 80, 79, 78, 77, 76, 75,74 };
        private readonly ExtendedEChessBoardConfiguration _extendedConfiguration;
        private static readonly object _lock = new object();
        private readonly ConcurrentQueue<string[]> _flashFields = new ConcurrentQueue<string[]>();

        public ECernoSpectrumChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _boardConfiguration = configuration;
            _extendedConfiguration = configuration.ExtendedConfig.First(e => e.IsCurrent);
            _showEvaluationValue = _extendedConfiguration.ShowEvaluationValue;
            _showMoveLine = _extendedConfiguration.ShowMoveLine;
            _showCurrentColor = _extendedConfiguration.ShowCurrentColor;
            _useBluetooth = configuration.UseBluetooth;
            _currentColor = Fields.COLOR_WHITE;
            _prevColor = Fields.COLOR_BLACK;
            _serialCommunication = new SerialCommunication(logger, configuration.PortName, _useBluetooth, Constants.TabutronicCernoSpectrum);
            _calibrateStorage = new CalibrateStorage(basePath);
            _logger = logger;
            SendArray[0] = 255;
            SendArray[1] = 85;
            SendArray[245] = 13;
            SendArray[246] = 10;
            AllOff[0] = 255;
            AllOff[1] = 85;
            AllOff[245] = 13;
            AllOff[246] = 10;
            AllOn[0] = 255;
            AllOn[1] = 85;
            AllOn[245] = 13;
            AllOn[246] = 10;
            InitFieldNamesToLedArray();
            BatteryLevel = "---";
            BatteryStatus = "Full";
            PieceRecognition = true;
            ValidForAnalyse = true;
            SelfControlled = false;
            MultiColorLEDs = true;
            var calibrationData = _calibrateStorage.GetCalibrationData();
            if (!string.IsNullOrWhiteSpace(calibrationData.BasePositionCodes))
            {
                _logger?.LogDebug("B: Stored calibration data available");
                IsCalibrated = Calibrate(calibrationData);
            }
            IsConnected = EnsureConnection();
            Information = Constants.TabutronicCernoSpectrum;
        
            _acceptProbingMoves = true;
            var thread = new Thread(FlashLEDs) { IsBackground = true };
            thread.Start();
        }

        private void InitFieldNamesToLedArray()
        {
            var count = 1;
            for (var i = 1; i < 9; i++)
            {
                foreach (var boardField in Fields.RowFields(i).OrderByDescending((f => f)))
                {
                    _whiteFieldNamesToLedArray[boardField] = new List<int> { count, count+1, count+9, count+10 };
                    count++;
                }
                count++;
            }
            count = 1;
            for (var i = 8; i > 0; i--)
            {
                foreach (var boardField in Fields.RowFields(i).OrderBy((f => f)))
                {
                    _blackFieldNamesToLedArray[boardField] = new List<int> { count, count + 1, count + 9, count + 10 };
                    count++;
                }
                count++;
            }
        }


        public ECernoSpectrumChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            Information = Constants.TabutronicCernoSpectrum;
        }


        public override void Reset()
        {
            //
        }

        public override bool CheckComPort(string portName)
        {
            _serialCommunication = new SerialCommunication(_logger, portName, _useBluetooth, Constants.TabutronicCernoSpectrum);
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
            lock (_lock)
            {
                //    _logger.LogDebug($"EB: Request set LEDs for {ledsParameter}");
                var fieldNamesList = new List<string>();
                var allFieldNamesList = new List<string>();
                var rgbMoveFrom = _extendedConfiguration.RGBMoveFrom;
                var rgbMoveTo = _extendedConfiguration.RGBMoveTo;
                var rgbInvalid = _extendedConfiguration.RGBInvalid;
                var rgbHelp = _extendedConfiguration.RGBHelp;
                var rgbTakeBack = _extendedConfiguration.RGBTakeBack;
                var rgbBookMove = _extendedConfiguration.RGBBookMove;
                if (ledsParameter.IsProbing && _extendedConfiguration.ShowPossibleMoves)
                {
                    _flashFields.TryDequeue(out _);
                    ResetLastSendBytes();
                    var currentBestMove = string.Empty;
                    var currentBestSore = decimal.Zero;
                    if (ledsParameter.ProbingMoves.Length < 1 || !_extendedConfiguration.ShowPossibleMovesEval)
                    {
                        SetLedForFields(ledsParameter.FieldNames, rgbMoveFrom);
                        if (ledsParameter.HintFieldNames.Length > 1)
                        {
                            SetLedForFields(ledsParameter.HintFieldNames, _extendedConfiguration.RGBPossibleMoves);
                        }
                        SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove);
                    }
                    else
                    {
                        SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove);

                        SetLedForFields(ledsParameter.FieldNames, rgbMoveFrom);
                        var strings = ledsParameter.ProbingMoves.Where(p => p.Score <= 1 && p.Score >= -1)
                            .OrderByDescending(p => p.Score).Select(p => p.FieldName).ToArray();
                        if (strings.Length > 0)
                        {
                            currentBestMove = strings[0];
                            currentBestSore = ledsParameter.ProbingMoves.First(p => p.FieldName.Equals(currentBestMove))
                                .Score;
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesPlayable);
                        }
                        strings = ledsParameter.ProbingMoves.Where(p => p.Score > 1).OrderByDescending(p => p.Score)
                            .Select(p => p.FieldName).ToArray();
                        if (strings.Length > 0)
                        {
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesGood);
                        }
                        else
                        {
                            if (currentBestSore > 0)
                            {
                                SetLedForFields(new[] { currentBestMove }, _extendedConfiguration.RGBPossibleMovesGood);
                            }
                        }

                        strings = ledsParameter.ProbingMoves.Where(p => p.Score <= -1).Select(p => p.FieldName)
                            .ToArray();
                        if (strings.Length > 0)
                        {
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesBad);
                        }
                    }

                    return;
                }

                if (ledsParameter.FieldNames.Length == 2)
                {
                    if (_showMoveLine && !ledsParameter.IsError)
                    {
                        var moveLine =
                            MoveLineHelper.GetMoveLine(ledsParameter.FieldNames[0], ledsParameter.FieldNames[1]);
                        fieldNamesList.AddRange(moveLine);
                    }
                    else
                    {
                        fieldNamesList.AddRange(ledsParameter.FieldNames);
                    }
                }
                else
                {
                    fieldNamesList.AddRange(ledsParameter.FieldNames);

                }

                allFieldNamesList.AddRange(fieldNamesList);
                allFieldNamesList.AddRange(ledsParameter.InvalidFieldNames);
                allFieldNamesList.AddRange(ledsParameter.BookFieldNames);
                if (allFieldNamesList.Count == 0)
                {
                    return;
                }

                if (ledsParameter.IsThinking)
                {
                    _flashFields.TryDequeue(out _);
                    ResetLastSendBytes();
                    if (ledsParameter.RepeatLastMove)
                    {
                        
                        _logger.LogDebug($"EB: Set thinking LEDs for {ledsParameter}");
                        SetLedForFields(fieldNamesList.ToArray(), rgbHelp);
                    }
                    else
                    {
                        _logger.LogDebug($"EB: Set thinking LEDs for {ledsParameter}");
                        SetLedForFields(fieldNamesList.ToArray(), rgbHelp);
                    }
                    SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove);
                    return;
                }

                if (ledsParameter.IsMove)
                {
                    //_logger.LogDebug($"EB: Set move LEDs for {ledsParameter}");
                    
                    if (_extendedConfiguration.SplitMoveFromMoveTo() && fieldNamesList.Count == 2)
                    {
                        var f = fieldNamesList.ToArray();
                        SetLedForFields(new[] { f[0] }, rgbMoveFrom);
                        SetLedForFields(new[] { f[1] }, rgbMoveTo);
                    }
                    else
                    {
                        SetLedForFields(fieldNamesList.ToArray(), rgbMoveFrom);
                    }

                    SetLedForFields(ledsParameter.InvalidFieldNames, rgbInvalid);
                    SetLedForFields(ledsParameter.HintFieldNames, rgbHelp);
                    SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove);
                    _flashFields.TryDequeue(out _);
                    _flashFields.Enqueue(new string[] {fieldNamesList[0],fieldNamesList[1]});
                    return;
                }

                if (ledsParameter.IsTakeBack)
                {
                    _flashFields.TryDequeue(out _);
                    _logger.LogDebug($"EB: Set take back LEDs for {ledsParameter}");
                    SetLedForFields(fieldNamesList.ToArray(), rgbTakeBack);
                    return;
                }

                if (ledsParameter.IsError)
                {
                    _flashFields.TryDequeue(out _);
                    _logger.LogDebug($"EB: Set error LEDs for {ledsParameter}");
                    SetLedForFields(fieldNamesList.ToArray(), rgbMoveFrom);
                    SetLedForFields(ledsParameter.InvalidFieldNames, rgbInvalid);


                    return;
                }

                if (ledsParameter.BookFieldNames.Length > 0)
                {
                    SetAllLedsOff(true);
                    SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove);
                }

                _logger.LogError($"EB: Request without valid indicator set LEDs for {ledsParameter}");
            }
        }

        public override void SetAllLedsOff(bool forceOff)
        {
            if (!EnsureConnection())
            {
                return;
            }
            _flashFields.TryDequeue(out _);
            lock (_locker)
            {
                _logger?.LogDebug("B: Send all off");
                _serialCommunication.ClearToBoard();
                Array.Copy(FieldArray, _lastSendBytes, FieldArray.Length);
                if (forceOff)
                {
                    _serialCommunication.Send(AllOff, true);
                }
                else
                {
                    ShowCurrentColorInfos();
                }
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
           //
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
            _calibrateStorage.DeleteCalibrationData();
            _currentColor = Fields.COLOR_WHITE;
            _prevColor = Fields.COLOR_BLACK;
            _logger?.LogDebug("B: start calibrate ");
            SetLedForFields(new SetLEDsParameter()
            {
                FieldNames = new[] { "A1", "B1", "C1", "D1", "E1", "F1", "G1", "H1", "A2", "B2", "C2", "D2", "E2", "F2", "G2", "H2", "A8", "B8", "C8", "D8", "E8", "F8", "G8", "H8", "A7", "B7", "C7", "D7", "E7", "F7", "G7", "H7", "D3", "D6" },
                Promote = string.Empty,
                IsThinking = false,
                IsMove = false,
                IsError = true,
                DisplayString = string.Empty
            });
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

        public override void AdditionalInformation(string information)
        {
            var strings = information.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (strings.Length == 0)
            {
                return;
            }
            _logger?.LogDebug($"Additional information: {information}");

            if (strings[0].StartsWith("R:", StringComparison.InvariantCultureIgnoreCase))
            {
                var array = strings[0].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                var red = array[1];

                array = strings[1].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                var green = array[1];

                array = strings[2].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                var blue = array[1];

                var fieldNames = new List<string>();
                for (var i = 3; i < strings.Length; i++)
                {
                    fieldNames.Add(strings[i]);
                }
                SetLedForFields(fieldNames.ToArray(), red, green, blue);
            }
            if (strings[0].StartsWith("SCORE:", StringComparison.InvariantCultureIgnoreCase))
            {
                _currentEvalColor = int.Parse(strings[1]);
                _currentEval = strings[2];
            }
        }

        private void SetLedForFields(string[] fieldNames, string rgbCodes)
        {
            if (rgbCodes.Length < 9)
            {
                return;
            }
            SetLedForFields(fieldNames, rgbCodes.Substring(0,3),rgbCodes.Substring(3,3),rgbCodes.Substring(6,3));
        }

        private void SetLedForFields(string[] fieldNames, string red, string green, string blue)
        {
            var result = Enumerable.Repeat<byte>(0, 243).ToArray();
            Array.Copy(_lastSendBytes, result, _lastSendBytes.Length);
            foreach (var fieldName in fieldNames)
            {
                if (fieldName == "CC")
                {
                    result[_whiteOnMoveFieldByte * 3 - 1] = byte.Parse(blue); 
                    result[_whiteOnMoveFieldByte * 3 - 2] = byte.Parse(green); 
                    result[_whiteOnMoveFieldByte * 3 - 3] = byte.Parse(red); 
                    break;
                }
                if (fieldName == "AD")
                {
                    foreach (var whiteAdvantageFieldByte in _whiteAdvantageFieldBytes)
                    {
                        result[whiteAdvantageFieldByte * 3 - 1] = byte.Parse(blue);
                        result[whiteAdvantageFieldByte * 3 - 2] = byte.Parse(green);
                        result[whiteAdvantageFieldByte * 3 - 3] = byte.Parse(red);
                     
                    }
                    break;
                }
                if (fieldName == "DA")
                {
                    foreach (var x1 in _whiteAdvantageFieldBytes.OrderByDescending(f => f).ToArray())
                    {
                        result[x1 * 3 - 1] = byte.Parse(blue);
                        result[x1 * 3 - 2] = byte.Parse(green);
                        result[x1 * 3 - 3] = byte.Parse(red);
                    }

                    break;
                }
                result = UpdateLEDsForField(fieldName, result, byte.Parse(red), byte.Parse(green), byte.Parse(blue));
            }
            Array.Copy(result, 0, SendArray, 2, result.Length);
            Array.Copy(result, _lastSendBytes, result.Length);
            _serialCommunication.Send(SendArray);
        }

        private void SetLedForField(string fieldName, string red, string green, string blue)
        {
            var result = Enumerable.Repeat<byte>(0, 243).ToArray();
            Array.Copy(_lastSendBytes, result, _lastSendBytes.Length);
            
            {
               
                result = UpdateLEDsForField(fieldName, result, byte.Parse(red), byte.Parse(green), byte.Parse(blue));
            }
            Array.Copy(result, 0, SendArray, 2, result.Length);
            Array.Copy(result, _lastSendBytes, result.Length);
            _serialCommunication.Send(SendArray);
        }

        private void SetLedForFlash(string fieldName, string red, string green, string blue)
        {
            var result = Enumerable.Repeat<byte>(0, 243).ToArray();
            

            {

                result = UpdateLEDsForField(fieldName, result, byte.Parse(red), byte.Parse(green), byte.Parse(blue));
            }
            Array.Copy(result, 0, SendArray, 2, result.Length);
          
            _serialCommunication.Send(SendArray);
        }

        public override void RequestDump()
        {
            //
        }

        private string GetPiecesFen(string[] dataArray)
        {
            var codes = new string[40];
            var fenLine = string.Empty;
            if (_playWithWhite)
            {
                Array.Copy(dataArray, 0, codes, 0, 40);
                fenLine = GetFenLine(codes, out _);

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
                fenLine += GetFenLine(codes, out _).Replace("/", string.Empty);

            }
            else
            {
                try
                {
                    Array.Copy(dataArray, 280, codes, 0, 40);
                    fenLine = GetFenLine(codes, out _);

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
                    fenLine += GetFenLine(codes, out _).Replace("/", string.Empty);

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
                try
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
                    var allData = new List<string>();
                    allData.AddRange(boardData.FromBoard.Replace('\0', ' ').Trim()
                        .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                    //var dataArray = boardData.FromBoard.Replace('\0', ' ').Trim()
                    //    .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var retries = 0;
                    while (retries < 10)
                    {
                        if (allData.Count < 320)
                        {
                            boardData = _serialCommunication.GetFromBoard();
                            allData.AddRange(boardData.FromBoard.Replace('\0', ' ').Trim()
                                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                            retries++;
                            continue;
                        }

                        break;
                    }

                    var  dataArray = allData.ToArray();
                    if (dataArray.Length < 320)
                    {
                        return new DataFromBoard(UnknownPieceCode, boardData.Repeated);
                    }

                    var codes = new string[40];
                    var fenLine = string.Empty;
                    if (string.Join(" ", dataArray).Contains(
                            "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0"))
                    {
                        bool playWithWhite = _playWithWhite;
                        _playWithWhite = true;
                        var piecesFen = GetPiecesFen(dataArray);
                        if (FenCodes.BasePosition.StartsWith(piecesFen))
                        {

                            return new DataFromBoard(
                                piecesFen.Contains(UnknownPieceCode) ? string.Empty : piecesFen,
                                boardData.Repeated);
                        }

                        _playWithWhite = false;
                        piecesFen = GetPiecesFen(dataArray);
                        if (FenCodes.BasePosition.StartsWith(piecesFen))
                        {

                            return new DataFromBoard(
                                piecesFen.Contains(UnknownPieceCode) ? string.Empty : piecesFen,
                                boardData.Repeated);
                        }

                        _playWithWhite = playWithWhite;
                    }

                    if (_playWithWhite)
                    {
                        Array.Copy(dataArray, 0, codes, 0, 40);
                        fenLine = GetFenLine(codes, out _);
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
                        fenLine += GetFenLine(codes, out _).Replace("/", string.Empty);
                    }
                    else
                    {
                        Array.Copy(dataArray, 280, codes, 0, 40);
                        fenLine = GetFenLine(codes, out _);

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
                        fenLine += GetFenLine(codes, out _).Replace("/", string.Empty);
                    }

                    return new DataFromBoard(fenLine.Contains(UnknownPieceCode) ? string.Empty : fenLine,
                        boardData.Repeated);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"B: GetPiecesFen: {ex.Message} ");
                }
                return new DataFromBoard(string.Empty);
            }
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return GetPiecesFen();
        }

        protected override void SetToNewGame()
        {
            _currentEval = string.Empty;
            SetAllLedsOff(true);
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
            _currentColor = currentColor;
            ShowCurrentColorInfos();
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

        #region private

        private void FlashLEDs()
        {
            bool switchSide = false;
            while (!_release)
            {
                if (_flashFields.TryPeek(out string[] fields))
                {
                    if (_extendedConfiguration.FlashMoveFrom || _extendedConfiguration.FlashMoveTo)
                    {
                        if (switchSide)
                        {

                            SetLedForFlash(fields[1], _extendedConfiguration.RGBMoveTo.Substring(0, 3),
                                _extendedConfiguration.RGBMoveTo.Substring(3, 3),
                                _extendedConfiguration.RGBMoveTo.Substring(6, 3));
                        }
                        else
                        {
                            SetLedForFlash(fields[0], _extendedConfiguration.RGBMoveFrom.Substring(0, 3),
                                _extendedConfiguration.RGBMoveFrom.Substring(3, 3),
                                _extendedConfiguration.RGBMoveFrom.Substring(6, 3));
                        }

                        switchSide = !switchSide;
                    }

                    Thread.Sleep(1200);
                    continue;

                }

                Thread.Sleep(10);
            }
        }
        private void ResetLastSendBytes()
        {
            Array.Copy(FieldArray, _lastSendBytes, FieldArray.Length);
        }

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

        private byte[] UpdateLEDsForField(string fieldName, byte[] current, byte red, byte green, byte blue)
        {
            // Exact two letters expected, e.g. "E2"
            if (string.IsNullOrWhiteSpace(fieldName) || fieldName.Length != 2)
            {
                return current;
            }
            
            var fieldNumber = Fields.GetFieldNumber(fieldName);
            if (fieldNumber == Fields.COLOR_EMPTY)
            {
                return current;
            }
            var result = Enumerable.Repeat<byte>(0, 243).ToArray();
            Array.Copy(current, result, current.Length);
            if (_playWithWhite)
            {
                foreach (var x1 in _whiteFieldNamesToLedArray[fieldNumber])
                {
                    result[x1 * 3 - 1] = blue;
                    result[x1 * 3 - 2] = green;
                    result[x1 * 3 - 3] = red;
                }
            }
            else
            {
                foreach (var x1 in _blackFieldNamesToLedArray[fieldNumber])
                {
                    result[x1 * 3 - 1] = blue;
                    result[x1 * 3 - 2] = green;
                    result[x1 * 3 - 3] = red;
                }
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

            return _boardCodesToChessPiece.TryGetValue(code, out var fromCode) ? fromCode : UnknownPieceCode;
        }

        private void ShowCurrentColorInfos()
        {
            _logger?.LogDebug($"B: Show current color infos {_currentColor}");
            _prevColor = _currentColor;
            var red = _showCurrentColor ? _extendedConfiguration.RGBCurrentColor.Substring(0,3) : "000";
            var green = _showCurrentColor ? _extendedConfiguration.RGBCurrentColor.Substring(3, 3) : "000";
            var blue = _showCurrentColor ? _extendedConfiguration.RGBCurrentColor.Substring(6, 3) : "000";
            var result = Enumerable.Repeat<byte>(0, 243).ToArray();
            Array.Copy(FieldArray, result, FieldArray.Length);
            if (_showCurrentColor)
            {
                _logger?.LogDebug("B: Add LED for current color indication");
                if (_currentColor==Fields.COLOR_WHITE)
                {
                    if (_playWithWhite)
                    {
                        result[_whiteOnMoveFieldByte * 3 - 1] = byte.Parse(blue);
                        result[_whiteOnMoveFieldByte * 3 - 2] = byte.Parse(green);
                        result[_whiteOnMoveFieldByte * 3 - 3] = byte.Parse(red);
                    }
                    else
                    {
                        result[_blackOnMoveFieldByte * 3 - 1] = byte.Parse(blue);
                        result[_blackOnMoveFieldByte * 3 - 2] = byte.Parse(green);
                        result[_blackOnMoveFieldByte * 3 - 3] = byte.Parse(red);
                    }
                }
                else
                {
                    if (_playWithWhite)
                    {
                        result[_blackOnMoveFieldByte * 3 - 1] = byte.Parse(blue);
                        result[_blackOnMoveFieldByte * 3 - 2] = byte.Parse(green);
                        result[_blackOnMoveFieldByte * 3 - 3] = byte.Parse(red);
                    }
                    else
                    {
                        result[_whiteOnMoveFieldByte * 3 - 1] = byte.Parse(blue);
                        result[_whiteOnMoveFieldByte * 3 - 2] = byte.Parse(green);
                        result[_whiteOnMoveFieldByte * 3 - 3] = byte.Parse(red);
                    }
                }
            }

            if (!_showEvaluationValue)
            {
                Array.Copy(result, 0, SendArray, 2, result.Length);
                _serialCommunication.Send(SendArray);
                return;
            }
            if (!_currentEvalColor.Equals(_currentColor))
            {
                Array.Copy(result, 0, SendArray, 2, result.Length);
                _serialCommunication.Send(SendArray);
                return;
            }
            if (string.IsNullOrEmpty(_currentEval))
            {
                _currentEval = "0";
            }
            if (decimal.TryParse(_currentEval.Replace(".", ","), out var eval))
            {
                if (eval > 0)
                {
                    red = _extendedConfiguration.RGBEvalAdvantage.Substring(0, 3);
                    green = _extendedConfiguration.RGBEvalAdvantage.Substring(3, 3);
                    blue = _extendedConfiguration.RGBEvalAdvantage.Substring(6, 3);
                }
                else
                {
                    red = _extendedConfiguration.RGBEvalDisAdvantage.Substring(0, 3);
                    green = _extendedConfiguration.RGBEvalDisAdvantage.Substring(3, 3);
                    blue = _extendedConfiguration.RGBEvalDisAdvantage.Substring(6, 3);
                }
               
                var number = GetNumberForEval(eval);
                if (_currentColor == Fields.COLOR_WHITE)
                {
                    for (var i = 0; i < number; i++)
                    {
                        if (_playWithWhite)
                        {
                            result[_whiteAdvantageFieldBytes[i] * 3 - 1] = byte.Parse(blue);
                            result[_whiteAdvantageFieldBytes[i] * 3 - 2] = byte.Parse(green);
                            result[_whiteAdvantageFieldBytes[i] * 3 - 3] = byte.Parse(red);
                        }
                        else
                        {
                            result[_blackAdvantageFieldBytes[i] * 3 - 1] = byte.Parse(blue);
                            result[_blackAdvantageFieldBytes[i] * 3 - 2] = byte.Parse(green);
                            result[_blackAdvantageFieldBytes[i] * 3 - 3] = byte.Parse(red);
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < number; i++)
                    {
                        if (_playWithWhite)
                        {
                            result[_blackAdvantageFieldBytes[i] * 3 - 1] = byte.Parse(blue);
                            result[_blackAdvantageFieldBytes[i] * 3 - 2] = byte.Parse(green);
                            result[_blackAdvantageFieldBytes[i] * 3 - 3] = byte.Parse(red);
                        }
                        else
                        {
                            result[_whiteAdvantageFieldBytes[i] * 3 - 1] = byte.Parse(blue);
                            result[_whiteAdvantageFieldBytes[i] * 3 - 2] = byte.Parse(green);
                            result[_whiteAdvantageFieldBytes[i] * 3 - 3] = byte.Parse(red);
                        }
                    }
                }
                _logger?.LogDebug($"B: Add LEDs for evaluation score {eval}");
                Array.Copy(result, 0, SendArray, 2, result.Length);
                _serialCommunication.Send(SendArray);
            }
        }
        private int GetNumberForEval(decimal eval)
        {
            int number = 0;
            if (eval < 0)
            {
                if (eval <= -1)
                {
                    number = 1;
                }

                if (eval <= -2)
                {
                    number = 2;
                }

                if (eval <= -3)
                {
                    number = 3;
                }

                if (eval <= -4)
                {
                    number = 4;
                }

                if (eval <= -5)
                {
                    number = 5;
                }

                if (eval <= -6)
                {
                    number = 6;
                }

                if (eval <= -7)
                {
                    number = 7;
                }
            }
            else
            {
                if (eval >= 1)
                {
                    number = 1;
                }

                if (eval >= 2)
                {
                    number = 2;
                }

                if (eval >= 3)
                {
                    number = 3;
                }

                if (eval >= 4)
                {
                    number = 4;
                }

                if (eval >= 5)
                {
                    number = 5;
                }

                if (eval >= 6)
                {
                    number = 6;
                }

                if (eval >= 7)
                {
                    number = 7;
                }
            }
            return number;
        }

        #endregion

    }
}
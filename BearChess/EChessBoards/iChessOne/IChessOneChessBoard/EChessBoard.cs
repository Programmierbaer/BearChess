using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.IChessOneChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly string _basePath;
        private static readonly object _lock = new object();
        private readonly bool _useBluetooth;
        private bool _release = false;
        private readonly string[] _allLEDSOn  = { "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF" };
        private readonly string[] _allLEDSOff = { "00", "00", "00", "00", "00", "00", "00", "00", "00", "00" };
        private readonly string[] _LEDSForFlash = { "00", "80", "00", "00", "00", "00", "00", "00", "01", "00"};
        private readonly string[] _LEDSForBlackOnMove = { "FF", "00", "00", "00", "00", "00", "00", "00", "00", "00" };
        private readonly string _startReadingRQCmd = "CPIRQ";
        private readonly string _requestHardwareVersion = "RH";
        private readonly string _requestBattery = "RB";
        private readonly string _requestPosition = "RP";
        private readonly string _startReadingPMCmd = "CPM";
        private readonly string _stopReadingCmd = "CPOFF";
        private readonly string _statusOffCmd = "CSOFF";
        private readonly string _advancedColor = "EL";


        public override void SetEngineColor(int color)
        {
            //
        }

        public override event EventHandler BasePositionEvent;
        public override event EventHandler<string> DataEvent;
        public override event EventHandler HelpRequestedEvent;

        private readonly Dictionary<string, string> _codeToFen = new Dictionary<string, string>()
                                                                 {
                                                                     { "0", "." },
                                                                     { "1", "P" },
                                                                     { "2", "N" },
                                                                     { "3", "B" },
                                                                     { "4", "R" },
                                                                     { "5", "Q" },
                                                                     { "6", "K" },
                                                                     { "7", "p" },
                                                                     { "8", "n" },
                                                                     { "9", "b" },
                                                                     { "A", "r" },
                                                                     { "B", "q" },
                                                                     { "C", "k" }
                                                                 };

        private readonly Dictionary<Fields.Lines, int> _linesToColWhite = new Dictionary<Fields.Lines, int>()
                                                                     {
                                                                         { Fields.Lines.A, 1 },
                                                                         { Fields.Lines.B, 2 },
                                                                         { Fields.Lines.C, 4 },
                                                                         { Fields.Lines.D, 8 },
                                                                         { Fields.Lines.E, 16 },
                                                                         { Fields.Lines.F, 32 },
                                                                         { Fields.Lines.G, 64 },
                                                                         { Fields.Lines.H, 128 },
                                                                     };

        private string _lastData = string.Empty;
        private string _lastResult = string.Empty;
        private readonly string _dimLevel = string.Empty;
        private readonly string[] _allData = new string[32];
        private int _allIndex = 0;
        private bool _readingPosition = false;
        private bool _readingCmdStart = false;
        private int _currentColor;
        private int _prevColor;
        private readonly bool _showMoveLine;
        private readonly bool _showEvaluationValue;
        private readonly bool _showCurrentColor;
        private string _currentEval = "0";
        private readonly ExtendedEChessBoardConfiguration _extendedConfiguration;
        HashSet<string> _lastSend = new HashSet<string>();
        private SetLEDsParameter _lastSendMoveParameters = new SetLEDsParameter();
        private SetLEDsParameter _lastSendThinkingParameters = new SetLEDsParameter();
        private int _currentEvalColor;

        public EChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _basePath = basePath;
            _useBluetooth = configuration.UseBluetooth;
            _extendedConfiguration = configuration.ExtendedConfig.First(e => e.IsCurrent);
            _showEvaluationValue = _extendedConfiguration.ShowEvaluationValue;
            _showMoveLine = _extendedConfiguration.ShowMoveLine;
            _showCurrentColor = _extendedConfiguration.ShowCurrentColor;
            _dimLevel =  _extendedConfiguration.DimLevel.ToString("X");
            _currentColor = Fields.COLOR_WHITE;
            _prevColor = Fields.COLOR_BLACK;
            _logger = logger;
            
            MultiColorLEDs = true;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            lock (_lock)
            {
                _serialCommunication = new SerialCommunication(logger, configuration.PortName, _useBluetooth);
            }

            Information = "iChessOne";
        }

      

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            Information = "iChessOne";
            MultiColorLEDs = true;
        }

        public override void SetCurrentColor(int currentColor)
        {
            _currentColor = currentColor;
            ShowCurrentColorInfos();
        }

        
        private byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public override void Reset()
        {
          //
        }

        public override void Release()
        {
            _release = true;
            lock (_lock)
            {
                _serialCommunication.Send(ConvertToArray(_statusOffCmd));
            }
        }

        
        public override void SetLedForFields(SetLEDsParameter ledsParameter)
        {
            lock (_lock)
            {

            //    _logger.LogDebug($"EB: Request set LEDs for {ledsParameter}");
                var fieldNamesList = new List<string>();
                var allFieldNamesList = new List<string>();
                var rgbMoveFrom = _extendedConfiguration.RGBMoveFrom;
                var dimMoveFrom = _extendedConfiguration.DimMoveFrom;
                var rgbMoveTo = _extendedConfiguration.RGBMoveTo;
                var dimMoveTo = _extendedConfiguration.DimMoveTo;
                var flashMoveFrom = _extendedConfiguration.FlashMoveFrom;
                var flashMoveTo = _extendedConfiguration.FlashMoveTo;
                var rgbInvalid = _extendedConfiguration.RGBInvalid;
                var rgbHelp = _extendedConfiguration.RGBHelp;
                var rgbTakeBack = _extendedConfiguration.RGBTakeBack;
                if (ledsParameter.IsProbing && _extendedConfiguration.ShowPossibleMoves)
                {

                    if (ledsParameter.HintFieldNames.All(f => _lastSend.Contains(f)))
                    {
                       // return;
                    }

                    string currentBestMove = string.Empty;
                    decimal currentBestSore = decimal.Zero;
                    if (ledsParameter.ProbingMoves.Length < 1 || !_extendedConfiguration.ShowPossibleMovesEval)
                    {
                        SetLedForFields(ledsParameter.FieldNames, rgbMoveFrom, _extendedConfiguration.FlashMoveFrom,
                            _extendedConfiguration.DimMoveFrom.ToString("X"), false,"Move from");
                        //if (ledsParameter.HintFieldNames.Length > 1)
                        {
                            SetLedForFields(ledsParameter.HintFieldNames, _extendedConfiguration.RGBPossibleMoves,
                                _extendedConfiguration.FlashPossibleMoves,
                                _extendedConfiguration.DimPossibleMoves.ToString("X"), true, "Possible moves");
                        }

                        _lastSend.Clear();
                        _lastSend = new HashSet<string>(ledsParameter.HintFieldNames);
                    }
                    else
                    {
                        SetLedForFields(ledsParameter.FieldNames, rgbMoveFrom, _extendedConfiguration.FlashMoveFrom,
                            _extendedConfiguration.DimMoveFrom.ToString("X"), false,"Move from");
                        string[] strings = ledsParameter.ProbingMoves.Where(p => p.Score <= -1).Select(p => p.FieldName).ToArray();
                        if (strings.Length > 0)
                        {
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesBad,
                                _extendedConfiguration.FlashPossibleMovesBad,
                                _extendedConfiguration.DimPossibleMovesBad.ToString("X"), true,"Bad moves");
                        }
                        strings = ledsParameter.ProbingMoves.Where(p => p.Score <= 1 && p.Score>=-1).OrderByDescending(p => p.Score).Select(p => p.FieldName).ToArray();
                        if (strings.Length > 0)
                        {
                            currentBestMove = strings[0];
                            currentBestSore = ledsParameter.ProbingMoves.First(p => p.FieldName.Equals(currentBestMove)).Score;
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesPlayable,
                                _extendedConfiguration.FlashPossibleMovesPlayable,
                                _extendedConfiguration.DimPossibleMovesPlayable.ToString("X"), true, "Playable moves");
                        }
                        strings = ledsParameter.ProbingMoves.Where(p => p.Score > 1).OrderByDescending(p => p.Score).Select(p => p.FieldName).ToArray();
                        if (strings.Length > 0)
                        {
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesGood,
                                _extendedConfiguration.FlashPossibleMovesGood,
                                _extendedConfiguration.DimPossibleMovesGood.ToString("X"), true, "Good moves");
                        }
                        else
                        {
                            if (currentBestSore > 0)
                            {
                                SetLedForFields(new[] {currentBestMove}, _extendedConfiguration.RGBPossibleMovesGood,
                                    _extendedConfiguration.FlashPossibleMovesGood,
                                    _extendedConfiguration.DimPossibleMovesGood.ToString("X"), true, "Good moves");
                            }
                        }

                        _lastSend.Clear();
                        _lastSend = new HashSet<string>(ledsParameter.HintFieldNames);
                    }
                    return;
                }

                if (ledsParameter.FieldNames.Length == 2)
                {
                    if (_showMoveLine && !ledsParameter.IsError)
                    {
                        var moveLine = MoveLineHelper.GetMoveLine(ledsParameter.FieldNames[0], ledsParameter.FieldNames[1]);
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
                if (allFieldNamesList.Count == 0)
                {
                    return;
                }
                if (!ledsParameter.ForceShow && allFieldNamesList.Count>0 && allFieldNamesList.All(f => _lastSend.Contains(f)))
                {
  //                  _logger.LogDebug($"EB: Request set LEDs ignored: equal to last send");
                    return;
                }

                _lastSend.Clear();
                _lastSend = new HashSet<string>(fieldNamesList.Concat(ledsParameter.InvalidFieldNames));
               

                if (ledsParameter.IsThinking)
                {
                    if (!ledsParameter.ForceShow && _lastSendThinkingParameters.FieldNames.Length==2 && _lastSendThinkingParameters.FieldNames[0].Equals(ledsParameter.FieldNames[0]) &&
                        _lastSendThinkingParameters.FieldNames[1].Equals(ledsParameter.FieldNames[1]))
                    {
                        return;
                    }
                    if (ledsParameter.RepeatLastMove)
                    {
                        SetLedForFields(_lastSendMoveParameters);
                        _logger.LogDebug($"EB: Set thinking LEDs for {ledsParameter}");
                        SetLedForFields(fieldNamesList.ToArray(), rgbHelp, _extendedConfiguration.FlashHelp, _extendedConfiguration.DimHelp.ToString("X"),
                            true,"Thinking fields");
                    }
                    else
                    {
                        _logger.LogDebug($"EB: Set thinking LEDs for {ledsParameter}");
                        SetLedForFields(fieldNamesList.ToArray(), rgbHelp, _extendedConfiguration.FlashHelp,
                            _extendedConfiguration.DimHelp.ToString("X"),
                            false,"Thinking fields");
                    }

                    _lastSendThinkingParameters = ledsParameter;
                    return;
                }

                if (ledsParameter.IsMove)
                {
                    //_logger.LogDebug($"EB: Set move LEDs for {ledsParameter}");
                    _lastSendMoveParameters = ledsParameter;
                    if (_extendedConfiguration.SplitMoveFromMoveTo() && fieldNamesList.Count == 2)
                    {
                        string[] f = fieldNamesList.ToArray();
                        SetLedForFields(new[] { f[0] }, rgbMoveFrom, flashMoveFrom, dimMoveFrom.ToString("X"), false,"Move from");
                        SetLedForFields(new[] { f[1] }, rgbMoveTo, flashMoveTo, dimMoveTo.ToString("X"), true, "Move to");

                    }
                    else
                    {
                        SetLedForFields(fieldNamesList.ToArray(), rgbMoveFrom, flashMoveFrom,dimMoveFrom.ToString("X"), false, "Move from/to");
                    }

                    SetLedForFields(ledsParameter.InvalidFieldNames, rgbInvalid, _extendedConfiguration.FlashInvalid,
                        _extendedConfiguration.DimInvalid.ToString("X"), fieldNamesList.ToArray().Length > 0,"Invalid fields");
                    SetLedForFields(ledsParameter.HintFieldNames, rgbHelp, _extendedConfiguration.FlashHelp,
                        _extendedConfiguration.DimHelp.ToString("X"), fieldNamesList.ToArray().Length > 0 || ledsParameter.InvalidFieldNames.ToArray().Length > 0,"Hint fields") ;

                    return;
                }

                if (ledsParameter.IsTakeBack)
                {
                    _logger.LogDebug($"EB: Set take back LEDs for {ledsParameter}");
                    SetLedForFields(fieldNamesList.ToArray(), rgbTakeBack, _extendedConfiguration.FlashTakeBack,
                                        _extendedConfiguration.DimTakeBack.ToString("X"), false,"Take back");
                    
                    return;
                }

                if (ledsParameter.IsError)
                {
                    _logger.LogDebug($"EB: Set error LEDs for {ledsParameter}");
                    SetLedForFields(fieldNamesList.ToArray(), rgbMoveFrom, _extendedConfiguration.FlashCurrentColor,
                                        _extendedConfiguration.DimMoveFrom.ToString("X"), false,"Invalid fields: Move from");
                    SetLedForFields(ledsParameter.InvalidFieldNames, rgbInvalid, _extendedConfiguration.FlashInvalid,
                        _extendedConfiguration.DimInvalid.ToString("X"), fieldNamesList.ToArray().Length > 0,"Invalid fields: Invalid fields");


                    return;
                }
                _logger.LogError($"EB: Request without valid indicator set LEDs for {ledsParameter}");
            }
        }

        public override void SetAllLedsOff(bool forceOff)
        {
            _lastSend.Clear();
            if (!forceOff && (_showCurrentColor || _showEvaluationValue) && (_currentColor==_prevColor))
            {
                _logger.LogDebug("EB: Send for all off via show current color");
                ShowCurrentColorInfos();
                return;
            }

            byte[] colorFieldsMove = StringToByteArray(string.Join("", "00", "00"));
            lock (_lock)
            {
                var allLEDsOff = new string[_allLEDSOff.Length];
                _allLEDSOff.CopyTo(allLEDsOff, 0);

                var startCode = ConvertToArray(_advancedColor);
                var fieldLEDS = StringToByteArray(string.Join("", allLEDsOff));
                var flashFields = StringToByteArray(string.Join("", "01", "FF"));
                _logger.LogDebug("EB: Send for all off");
                _serialCommunication.Send(startCode.Concat(colorFieldsMove).Concat(fieldLEDS).Concat(flashFields)
                                                   .ToArray(),"Set all LED OFF");
            }
        }

        public override void SetAllLedsOn()
        {
            byte[] startCode = ConvertToArray(_advancedColor);
            byte[] fieldLEDS = StringToByteArray(string.Join("", _allLEDSOn));
            byte[] colorFields = StringToByteArray(string.Join("", $"{_dimLevel}F", "00"));
            byte[] flashFields = StringToByteArray(string.Join("", "E1", "FF"));
            lock (_lock)
            {
                _serialCommunication.Send(startCode.Concat(colorFields).Concat(fieldLEDS).Concat(flashFields)
                    .ToArray(),"Set all LED ON 1/3");
            }
            
            colorFields = StringToByteArray(string.Join("", $"{_dimLevel}0", "F0"));
            
            lock (_lock)
            {
                Thread.Sleep(500);
                _serialCommunication.Send(startCode.Concat(colorFields).Concat(fieldLEDS).Concat(flashFields)
                                                   .ToArray(), "Set all LED ON 2/3");
            }
            
            colorFields = StringToByteArray(string.Join("", $"{_dimLevel}0", "0F"));

            lock (_lock)
            {
                Thread.Sleep(500);
                _serialCommunication.Send(startCode.Concat(colorFields).Concat(fieldLEDS).Concat(flashFields)
                                                   .ToArray(),"Set all LED ON 3/3"); 
            }
        }
        private byte[] ConvertToArray(string param)
        {
            return Encoding.ASCII.GetBytes(param);
        }

      

        public override void Calibrate()
        {
            if (!EnsureConnection())
            {
                return;
            }
            _currentColor = Fields.COLOR_WHITE;
            _prevColor = Fields.COLOR_BLACK;
            lock (_lock)
            {
                if (_extendedConfiguration.InterruptMode)
                {
                    _serialCommunication.Send(ConvertToArray(_startReadingRQCmd),"Reading with interrupt mode");
                }
                else
                {
                    _serialCommunication.Send(ConvertToArray($"{_startReadingPMCmd}{_extendedConfiguration.ScanIntervall}"),$"Reading with interval mode every {_extendedConfiguration.ScanIntervall} ms");
                }

                _serialCommunication.Send(ConvertToArray(_requestPosition),"Request current position");
                _serialCommunication.Send(ConvertToArray(_requestHardwareVersion),"Request hardware version");
            }
        }

        public override void SendInformation(string message)
        {

        }

        public override void AdditionalInformation(string information)
        {
            var strings = information.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (strings.Length == 0)
            {
                return;
            }
            _logger?.LogDebug($"Additional information: {information}");
            if (strings[0].StartsWith("45", StringComparison.InvariantCultureIgnoreCase))
            {
                byte[] startCode = StringToByteArray(string.Join("", strings));
                _serialCommunication.Send(startCode,$"Additional information: {information}");
                return;
            }
       

            if (strings[0].StartsWith("DIM:", StringComparison.InvariantCultureIgnoreCase))
            {
                var array = strings[1].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string dim = int.Parse(array[0]).ToString("X");
                array = strings[2].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string red = int.Parse(array[1]).ToString("X");

                array = strings[3].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string green = int.Parse(array[1]).ToString("X");

                array = strings[4].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string blue = int.Parse(array[1]).ToString("X");

                array = strings[5].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                bool flash = array[1].ToUpper().StartsWith("T");

                array = strings[6].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string clear = array[1];
                List<string> fieldNames = new List<string>();
                for (int i = 7; i < strings.Length; i++)
                {
                    fieldNames.Add(strings[i]);
                }
                lock (_lock)
                {
                    SetLedForFields(fieldNames.ToArray(), $"{red}{green}{blue}",flash, dim, clear.Equals("0"),$"Additional information: {information}");
                }
            }

            if (strings[0].StartsWith("SCORE:", StringComparison.InvariantCultureIgnoreCase))
            {
                _currentEvalColor = int.Parse(strings[1]);
                _currentEval = strings[2];
            }
        }

        private string ConvertFromRead(string data)
        {
            var i = Convert.ToInt32(data, 16);
            return Encoding.ASCII.GetString(new byte[] { (byte)i });
        }

     
        public override DataFromBoard GetPiecesFen()
        {
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }

            var dataFromBoard = _serialCommunication.GetFromBoard();
            if (dataFromBoard.FromBoard.Equals(_lastData))
            {
               return new DataFromBoard(_lastResult, dataFromBoard.Repeated);
            }

            _lastData = dataFromBoard.FromBoard;
            var allData = dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < allData.Length; i++)
            {
                if (allData[i].Equals("3D") )
                {
                    _readingCmdStart = true;
                    continue;
                }
                if (allData[i].Equals("70") && _readingCmdStart)
                {
                    _readingPosition = true;
                    _readingCmdStart = false;
                    continue;
                }
                if (allData[i].Equals("68") && _readingCmdStart)
                {
                    _readingCmdStart = false;
                    string pLine = "";
                    for (int j = i+1; j < allData.Length; j++)
                    {
                        pLine += ConvertFromRead(allData[j]);
                    }

                    Information += " " + pLine;
                    return new DataFromBoard(_lastResult, dataFromBoard.Repeated);
                    
                }
                if (_allIndex < _allData.Length)
                {
                    _allData[_allIndex] = allData[i];
                }

                _allIndex++;
            }

            if (_readingPosition && _allIndex >= _allData.Length)
            {
                _readingPosition = false;
                _allIndex = 0;

                string pLine = "";
                for (int i = 0; i < _allData.Length; i++)
                {
                    var pCode = _allData[i];
                    if (_codeToFen.ContainsKey(pCode[0].ToString()) &&
                        _codeToFen.ContainsKey(pCode[1].ToString()))
                    {
                        pLine += _codeToFen[pCode[0].ToString()] + _codeToFen[pCode[1].ToString()];
                    }
                    else
                    {
                        _logger?.LogError($"EB: Unknown code {pCode}");
                    }
                }

                pLine = "s" + pLine;
                _logger?.LogDebug($"EB: Read: {pLine}");
                if (pLine.StartsWith("sRNBKQBNRPPPPPPPP................................pppppppprnbkqbnr"))
                {
                    _playWithWhite = false;

                }

                if (pLine.StartsWith("srnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR"))
                {
                    _playWithWhite = true;
                }

                if (pLine.Equals("s"))
                {
                    return new DataFromBoard(string.Empty);
                }

                _lastResult = GetPiecesFen(pLine, _playWithWhite);
            }

            return new DataFromBoard(_lastResult, dataFromBoard.Repeated);
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return GetPiecesFen();
        }

        protected override void SetToNewGame()
        {
            lock (_lock)
            {
                _serialCommunication.Send(ConvertToArray("RP"),"New game");
            }
            _currentColor = Fields.COLOR_WHITE;
            _prevColor = Fields.COLOR_WHITE;
        }

     

        private static string GetPiecesFen(string boardFen, bool playWithWhite)
        {
            string result = string.Empty;

            try
            {
                if (playWithWhite)
                {
                    for (int i = 1; i < 58; i += 8)
                    {
                        var substring = boardFen.Substring(i, 8);
                        result += GetFenLine(string.Concat(substring.Reverse()));
                    }
                }
                else
                {
                    for (int i = 57; i > 0; i -= 8)
                    {
                        var substring = boardFen.Substring(i, 8);
                        result += GetFenLine(substring);
                    }
                }

                return result.Substring(0, result.Length - 1);
            }
            catch
            {
                return string.Empty;
            }

        }

        private static string GetFenLine(string substring)
        {
            var result = string.Empty;
            var noFigureCounter = 0;
            for (var i = substring.Length - 1; i >= 0; i--)
            {
                var piecesFromCode = substring[i];
                if (piecesFromCode == '.')
                {
                    noFigureCounter++;
                    continue;
                }

                if (noFigureCounter > 0)
                {
                    result += noFigureCounter;
                }

                noFigureCounter = 0;
                result += piecesFromCode;
            }

            if (noFigureCounter > 0)
            {
                result += noFigureCounter;
            }

            return result + "/";
        }

        private void ShowCurrentColorInfos()
        {
            _prevColor = _currentColor;
            string red = _showCurrentColor ? _extendedConfiguration.RGBCurrentColor[0].ToString() : "0";
            string green = _showCurrentColor ? _extendedConfiguration.RGBCurrentColor[1].ToString() : "0";
            string blue = _showCurrentColor ? _extendedConfiguration.RGBCurrentColor[2].ToString() : "0";
            string flash = _showCurrentColor && _extendedConfiguration.FlashCurrentColor ? "F" : "0";
            string[] allLEDsOff = new string[_allLEDSOff.Length];
            _allLEDSOff.CopyTo(allLEDsOff, 0);
            if (_showCurrentColor)
            {
                if (_playWithWhite)
                {
                    allLEDsOff[9] = _currentColor == Fields.COLOR_WHITE ? $"80" : $"00";
                    allLEDsOff[0] = _currentColor == Fields.COLOR_WHITE ? $"00" : $"80";
                }
                else
                {
                    allLEDsOff[0] = _currentColor == Fields.COLOR_WHITE ? $"80" : $"00";
                    allLEDsOff[9] = _currentColor == Fields.COLOR_WHITE ? $"00" : $"80";
                }
            }

            byte[] startCode = ConvertToArray(_advancedColor);
            byte[] fieldLEDS = StringToByteArray(string.Join("", allLEDsOff));
            byte[] colorFieldsMove = StringToByteArray(string.Join("", $"{_extendedConfiguration.DimCurrentColor:X}{red}", $"{green}{blue}"));
            byte[] flashFields = StringToByteArray(string.Join("", $"{flash}1", "FF"));

            if (_showCurrentColor)
            {
                lock (_lock)
                {
                    _logger.LogDebug($"EB: Send infos for current color {Fields.ColorToName[_currentColor]}");
                    _serialCommunication.Send(startCode.Concat(colorFieldsMove).Concat(fieldLEDS).Concat(flashFields)
                                                       .ToArray(), $"Infos for current color {Fields.ColorToName[_currentColor]}");
                }
            }

            if (!_showEvaluationValue)
            {
                return;
            }
            if (!_currentEvalColor.Equals(_currentColor))
            {
                return;
            }
            Thread.Sleep(100);
            if (decimal.TryParse(_currentEval.Replace(".", ","), out decimal eval))
            {
                string dimLevelEval = _extendedConfiguration.DimEvalAdvantage.ToString("X");
                if (eval > 0)
                {
                    red = _extendedConfiguration.RGBEvalAdvantage[0].ToString();
                    green = _extendedConfiguration.RGBEvalAdvantage[1].ToString();
                    blue = _extendedConfiguration.RGBEvalAdvantage[2].ToString();
                    flash = _showCurrentColor && _extendedConfiguration.FlashEvalAdvantage ? "F" : "0";
                }
                else
                {
                    red = _extendedConfiguration.RGBEvalDisAdvantage[0].ToString();
                    green = _extendedConfiguration.RGBEvalDisAdvantage[1].ToString();
                    blue = _extendedConfiguration.RGBEvalDisAdvantage[2].ToString();
                    flash = _showCurrentColor && _extendedConfiguration.FlashEvalDisAdvantage ? "F" : "0";
                    dimLevelEval = _extendedConfiguration.DimEvalDisAdvantage.ToString("X");
                }
                byte[] colorFieldsOff = StringToByteArray(string.Join("", $"{dimLevelEval}{red}", $"{green}{blue}"));
                int number =  GetNumberForEval(eval);
              
                if (_currentColor == Fields.COLOR_WHITE)
                {
                    if (_playWithWhite)
                    {
                        allLEDsOff[9] = number.ToString("X2");
                    }
                    else
                    {
                        allLEDsOff[0] = number.ToString("X2");
                    }
                }
                else
                {
                    if (_playWithWhite)
                    {
                        allLEDsOff[0] = number.ToString("X2");
                    }
                    else
                    {
                        allLEDsOff[9] = number.ToString("X2");
                    }
                }
                fieldLEDS = StringToByteArray(string.Join("", allLEDsOff));
                byte[] flashFieldsAdd = StringToByteArray(string.Join("", $"{flash}0", "FF"));
                lock (_lock)
                {
                    _logger.LogDebug($"EB: Send for current evaluation {_currentEval} as {number}");
                    _serialCommunication.Send(startCode.Concat(colorFieldsOff).Concat(fieldLEDS).Concat(flashFieldsAdd)
                        .ToArray(), $"Current evaluation {_currentEval} as {number}");
                }
            }
        }

        private int GetNumberForEval(decimal eval)
        {
            int number = 0;
            if (eval < 0)
            {
                if (eval <= -1)
                {
                    number += 64;
                }

                if (eval <= -2)
                {
                    number += 32;
                }

                if (eval <= -3)
                {
                    number += 16;
                }

                if (eval <= -4)
                {
                    number += 8;
                }

                if (eval <= -5)
                {
                    number += 4;
                }

                if (eval <= -6)
                {
                    number += 2;
                }

                if (eval <= -7)
                {
                    number += 1;
                }
            }
            else
            {
                if (eval >= 1)
                {
                    number += 1;
                }

                if (eval >= 2)
                {
                    number += 2;
                }

                if (eval >= 3)
                {
                    number += 4;
                }

                if (eval >= 4)
                {
                    number += 8;
                }

                if (eval >= 5)
                {
                    number += 16;
                }

                if (eval >= 6)
                {
                    number += 32;
                }

                if (eval >= 7)
                {
                    number += 64;
                }
            }
            return number;
        }



        private void SetLedForFields(string[] fieldNames, string rgbCode, bool flash, string dimLevel, bool addLEDs, string info)
        {
            if (fieldNames.Length == 0)
            {
                return;
            }
            _logger?.LogDebug($"SetLedForFields: {string.Join(" ",fieldNames)}  RGB: {rgbCode}  Flash: {flash}  Dim: {dimLevel}  Add: {addLEDs}  Info: {info}");
            string[] allLEDsOff = new string[_allLEDSOff.Length];
            _allLEDSOff.CopyTo(allLEDsOff, 0);
            int[] sumCols = { 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var fieldName in fieldNames)
            {
                if (fieldName == "CC")
                {
                    allLEDsOff[9] = "80";
                    break;
                }
                if (fieldName == "AD")
                {
                    allLEDsOff[9] = "3F";
                    break;
                }
                if (fieldName == "DA")
                {
                    allLEDsOff[9] = "7E";
                    break;
                }
                var fieldNumber = Fields.GetFieldNumber(Fields.GetAdaptedFieldName(fieldName,_playWithWhite));
                var row = Fields.GetRow(fieldNumber);
                Fields.Lines line = Fields.GetLine(fieldNumber);
                //sumCols[row - 1] += _playWithWhite ?  _linesToColWhite[line] : _linesToColBlack[line];
                sumCols[row - 1] +=  _linesToColWhite[line];
            }
            
            allLEDsOff[8] = $"{sumCols[0]:X2}";
            allLEDsOff[7] = $"{sumCols[1]:X2}";
            allLEDsOff[6] = $"{sumCols[2]:X2}";
            allLEDsOff[5] = $"{sumCols[3]:X2}";
            allLEDsOff[4] = $"{sumCols[4]:X2}";
            allLEDsOff[3] = $"{sumCols[5]:X2}";
            allLEDsOff[2] = $"{sumCols[6]:X2}";
            allLEDsOff[1] = $"{sumCols[7]:X2}";
           
            byte[] startCode = ConvertToArray(_advancedColor);
            byte[] fieldLEDS = StringToByteArray(string.Join("", allLEDsOff));
            byte[] colorFields = StringToByteArray(string.Join("", $"{dimLevel}{rgbCode[0]}", $"{rgbCode[1]}{rgbCode[2]}"));
            string flashCode = flash ? "F" : "0";
            string clear = addLEDs ? "0" : "1";
            byte[] flashFields = StringToByteArray(string.Join("", $"{flashCode}{clear}", "FF"));
            _serialCommunication.Send(startCode.Concat(colorFields).Concat(fieldLEDS).Concat(flashFields).ToArray(), $"Fields: {string.Join(" ", fieldNames)}  RGB: {rgbCode}  Flash: {flash}  Dim: {dimLevel}  Add: {addLEDs}  Info: {info}");

        }

        #region Ignored
        public override void SetFen(string fen)
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
            return true;
        }

        public override void SpeedLeds(int level)
        {
            //
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

        public override void RequestDump()
        {
            //
        }

        #endregion
    }
}

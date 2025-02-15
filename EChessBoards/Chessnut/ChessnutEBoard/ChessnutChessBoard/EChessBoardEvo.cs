using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.ChessnutChessBoard
{
    public  class EChessBoardEvo : AbstractEBoard
    {
        public override event EventHandler BasePositionEvent;
        public override event EventHandler NewGamePositionEvent;
        public override event EventHandler HelpRequestedEvent;
        public override event EventHandler<string> DataEvent;

        private readonly EChessBoardConfiguration _boardConfiguration;
        
        private readonly bool _showMoveLine;
        private static readonly Dictionary<string, byte> _adaptedFieldName = new Dictionary<string, byte>()
                                                                            {
                                                                                { "A8", 0 },
                                                                                { "B8", 1 },
                                                                                { "C8", 2 },
                                                                                { "D8", 3 },
                                                                                { "E8", 4 },
                                                                                { "F8", 5 },
                                                                                { "G8", 6 },
                                                                                { "H8", 7 },
                                                                                { "A7", 8 },
                                                                                { "B7", 9 },
                                                                                { "C7", 10 },
                                                                                { "D7", 11 },
                                                                                { "E7", 12 },
                                                                                { "F7", 13 },
                                                                                { "G7", 14 },
                                                                                { "H7", 15 },
                                                                                { "A6", 16 },
                                                                                { "B6", 17 },
                                                                                { "C6", 18 },
                                                                                { "D6", 19 },
                                                                                { "E6", 20 },
                                                                                { "F6", 21 },
                                                                                { "G6", 22 },
                                                                                { "H6", 23 },
                                                                                { "A5", 24 },
                                                                                { "B5", 25 },
                                                                                { "C5", 26 },
                                                                                { "D5", 27 },
                                                                                { "E5", 28 },
                                                                                { "F5", 29 },
                                                                                { "G5", 30 },
                                                                                { "H5", 31 },
                                                                                { "A4", 32 },
                                                                                { "B4", 33 },
                                                                                { "C4", 34 },
                                                                                { "D4", 35 },
                                                                                { "E4", 36 },
                                                                                { "F4", 37 },
                                                                                { "G4", 38 },
                                                                                { "H4", 39 },
                                                                                { "A3", 40 },
                                                                                { "B3", 41 },
                                                                                { "C3", 42 },
                                                                                { "D3", 43 },
                                                                                { "E3", 44 },
                                                                                { "F3", 45 },
                                                                                { "G3", 46 },
                                                                                { "H3", 47 },
                                                                                { "A2", 48 },
                                                                                { "B2", 49 },
                                                                                { "C2", 50 },
                                                                                { "D2", 51 },
                                                                                { "E2", 52 },
                                                                                { "F2", 53 },
                                                                                { "G2", 54 },
                                                                                { "H2", 55 },
                                                                                { "A1", 56 },
                                                                                { "B1", 57 },
                                                                                { "C1", 58 },
                                                                                { "D1", 59 },
                                                                                { "E1", 60 },
                                                                                { "F1", 61 },
                                                                                { "G1", 62 },
                                                                                { "H1", 63 },
                                                                            };

        private static readonly object _lock = new object();
        private readonly ExtendedEChessBoardConfiguration _extendedConfiguration;
        private SetLEDsParameter _lastSendMoveParameters = new SetLEDsParameter();
        private SetLEDsParameter _lastSendThinkingParameters = new SetLEDsParameter();
        HashSet<string> _lastSend = new HashSet<string>();

        public EChessBoardEvo(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _boardConfiguration = configuration;
            _showMoveLine = configuration.ShowMoveLine;
            _logger = logger;
            MultiColorLEDs = true;
            PieceRecognition = true;
            ValidForAnalyse = true;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            _extendedConfiguration = configuration.ExtendedConfig.First(e => e.IsCurrent);
            //_serialCommunication = new WebCommunication(logger, configuration.WebSocketAddr, Constants.ChessnutEvo);
            _serialCommunication = new WebCommunication(logger, configuration.WebSocketAddr, Constants.ChessnutEvo);
            Information = Constants.ChessnutEvo;
            //var thread = new Thread(FlashLeds) { IsBackground = true };
            //thread.Start();
            //var probingThread = new Thread(ShowProbingMoves) { IsBackground = true };
            //probingThread.Start();
            _acceptProbingMoves = true;
        }
        public EChessBoardEvo(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            Information = Constants.ChessnutEvo;
            MultiColorLEDs = true;
            PieceRecognition = true;
            ValidForAnalyse = true;
        }

        public override void Reset()
        {
           //
        }

        public override void Release()
        {
            //
        }

        public override void SetFen(string fen)
        {
            var strings = fen.Split(" ".ToCharArray());
            _serialCommunication.Send(strings[0]);
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

        private void SetLedForFields(string[] fieldNames, string rgbCode, bool flash, string dimLevel, bool addLEDs,
            string info)
        {
            if (!addLEDs)
            {
                SetAllLedsOff(true);
            }

            if (int.TryParse(dimLevel, out int dimValue)) 
            {
                int red = rgbCode[0]=='0' ? 0 : Convert.ToInt32(dimValue.ToString("X")+ rgbCode[0],16);
                int green = rgbCode[1] == '0' ? 0 : Convert.ToInt32(dimValue.ToString("X") + rgbCode[1],16);
                int blue = rgbCode[2] == '0' ? 0 : Convert.ToInt32(dimValue.ToString("X") + rgbCode[2],16);
                foreach (var fieldName in fieldNames)
                {
                    _serialCommunication.Send($"led {_adaptedFieldName[fieldName.ToUpper()]} true {red} {green} {blue}");
                }
            }
        }

        public override void SetLedForFields(SetLEDsParameter ledsParameter)
        {
            if (!EnsureConnection())
            {
                return;
            }

            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(ledsParameter.FenString))
                {
                    var strings = ledsParameter.FenString.Split(" ".ToCharArray());
                    _serialCommunication.Send($"displayFen {strings[0]}");
                }

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
                var rgbBookMove = _extendedConfiguration.RGBBookMove;
                if (ledsParameter.IsProbing && _extendedConfiguration.ShowPossibleMoves)
                {
                    string currentBestMove = string.Empty;
                    decimal currentBestSore = decimal.Zero;
                    if (ledsParameter.ProbingMoves.Length < 1 || !_extendedConfiguration.ShowPossibleMovesEval)
                    {
                        SetLedForFields(ledsParameter.FieldNames, rgbMoveFrom, _extendedConfiguration.FlashMoveFrom,
                            _extendedConfiguration.DimMoveFrom.ToString(), false, "Move from");

                        SetLedForFields(ledsParameter.HintFieldNames, _extendedConfiguration.RGBPossibleMoves,
                            _extendedConfiguration.FlashPossibleMoves,
                            _extendedConfiguration.DimPossibleMoves.ToString(), true, "Possible moves");


                        SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove, _extendedConfiguration.FlashBookMove,
                            _extendedConfiguration.DimBook.ToString(), true, "Book fields");


                        _lastSend.Clear();
                        _lastSend = new HashSet<string>(ledsParameter.HintFieldNames);
                    }
                    else
                    {
                        SetLedForFields(ledsParameter.FieldNames, rgbMoveFrom, _extendedConfiguration.FlashMoveFrom,
                            _extendedConfiguration.DimMoveFrom.ToString(), false, "Move from");
                        string[] strings = ledsParameter.ProbingMoves.Where(p => p.Score <= -1).Select(p => p.FieldName)
                            .ToArray();
                        if (strings.Length > 0)
                        {
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesBad,
                                _extendedConfiguration.FlashPossibleMovesBad,
                                _extendedConfiguration.DimPossibleMovesBad.ToString(), true, "Bad moves");
                        }

                        strings = ledsParameter.ProbingMoves.Where(p => p.Score <= 1 && p.Score >= -1)
                            .OrderByDescending(p => p.Score).Select(p => p.FieldName).ToArray();
                        if (strings.Length > 0)
                        {
                            currentBestMove = strings[0];
                            currentBestSore = ledsParameter.ProbingMoves.First(p => p.FieldName.Equals(currentBestMove))
                                .Score;
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesPlayable,
                                _extendedConfiguration.FlashPossibleMovesPlayable,
                                _extendedConfiguration.DimPossibleMovesPlayable.ToString(), true, "Playable moves");
                        }

                        strings = ledsParameter.ProbingMoves.Where(p => p.Score > 1).OrderByDescending(p => p.Score)
                            .Select(p => p.FieldName).ToArray();
                        if (strings.Length > 0)
                        {
                            SetLedForFields(strings, _extendedConfiguration.RGBPossibleMovesGood,
                                _extendedConfiguration.FlashPossibleMovesGood,
                                _extendedConfiguration.DimPossibleMovesGood.ToString(), true, "Good moves");
                        }
                        else
                        {
                            if (currentBestSore > 0)
                            {
                                SetLedForFields(new[] { currentBestMove }, _extendedConfiguration.RGBPossibleMovesGood,
                                    _extendedConfiguration.FlashPossibleMovesGood,
                                    _extendedConfiguration.DimPossibleMovesGood.ToString(), true, "Good moves");
                            }
                        }

                        SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove, _extendedConfiguration.FlashBookMove,
                            _extendedConfiguration.DimBook.ToString(), true, "Book fields");
                        _lastSend.Clear();
                        _lastSend = new HashSet<string>(ledsParameter.HintFieldNames);
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

                if (!ledsParameter.ForceShow && allFieldNamesList.Count > 0 &&
                    allFieldNamesList.All(f => _lastSend.Contains(f)))
                {
                    //                  _logger.LogDebug($"EB: Request set LEDs ignored: equal to last send");
                    return;
                }

                _lastSend.Clear();
                _lastSend = new HashSet<string>(fieldNamesList.Concat(ledsParameter.InvalidFieldNames));


                if (ledsParameter.IsThinking)
                {
                    if (!ledsParameter.ForceShow && _lastSendThinkingParameters.FieldNames.Length == 2 &&
                        _lastSendThinkingParameters.FieldNames[0].Equals(ledsParameter.FieldNames[0]) &&
                        _lastSendThinkingParameters.FieldNames[1].Equals(ledsParameter.FieldNames[1]))
                    {
                        return;
                    }

                    if (ledsParameter.RepeatLastMove)
                    {
                        SetLedForFields(_lastSendMoveParameters);
                        _logger.LogDebug($"EB: Set thinking LEDs for {ledsParameter}");
                        SetLedForFields(fieldNamesList.ToArray(), rgbHelp, _extendedConfiguration.FlashHelp,
                            _extendedConfiguration.DimHelp.ToString(),
                            true, "Thinking fields");
                    }
                    else
                    {
                        _logger.LogDebug($"EB: Set thinking LEDs for {ledsParameter}");
                        SetLedForFields(fieldNamesList.ToArray(), rgbHelp, _extendedConfiguration.FlashHelp,
                            _extendedConfiguration.DimHelp.ToString(),
                            false, "Thinking fields");
                    }

                    SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove, _extendedConfiguration.FlashBookMove,
                        _extendedConfiguration.DimBook.ToString(), true, "Book fields");
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
                        SetLedForFields(new[] { f[0] }, rgbMoveFrom, flashMoveFrom, dimMoveFrom.ToString(), false,
                            "Move from");
                        SetLedForFields(new[] { f[1] }, rgbMoveTo, flashMoveTo, dimMoveTo.ToString(), true,
                            "Move to");

                    }
                    else
                    {
                        SetLedForFields(fieldNamesList.ToArray(), rgbMoveFrom, flashMoveFrom, dimMoveFrom.ToString(),
                            false, "Move from/to");
                    }

                    SetLedForFields(ledsParameter.InvalidFieldNames, rgbInvalid, _extendedConfiguration.FlashInvalid,
                        _extendedConfiguration.DimInvalid.ToString(), fieldNamesList.ToArray().Length > 0,
                        "Invalid fields");
                    SetLedForFields(ledsParameter.HintFieldNames, rgbHelp, _extendedConfiguration.FlashHelp,
                        _extendedConfiguration.DimHelp.ToString(),
                        fieldNamesList.ToArray().Length > 0 || ledsParameter.InvalidFieldNames.ToArray().Length > 0,
                        "Hint fields");

                    SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove, _extendedConfiguration.FlashBookMove,
                        _extendedConfiguration.DimHelp.ToString(),
                        fieldNamesList.ToArray().Length > 0 || ledsParameter.InvalidFieldNames.ToArray().Length > 0
                                                            || ledsParameter.HintFieldNames.ToArray().Length > 0,
                        "Book fields");
                    return;
                }

                if (ledsParameter.IsTakeBack)
                {
                    _logger.LogDebug($"EB: Set take back LEDs for {ledsParameter}");
                    SetLedForFields(fieldNamesList.ToArray(), rgbTakeBack, _extendedConfiguration.FlashTakeBack,
                        _extendedConfiguration.DimTakeBack.ToString(), false, "Take back");

                    return;
                }

                if (ledsParameter.IsError)
                {
                    _logger.LogDebug($"EB: Set error LEDs for {ledsParameter}");
                    SetLedForFields(fieldNamesList.ToArray(), rgbMoveFrom, _extendedConfiguration.FlashCurrentColor,
                        _extendedConfiguration.DimMoveFrom.ToString(), false, "Invalid fields: Move from");
                    SetLedForFields(ledsParameter.InvalidFieldNames, rgbInvalid, _extendedConfiguration.FlashInvalid,
                        _extendedConfiguration.DimInvalid.ToString(), fieldNamesList.ToArray().Length > 0,
                        "Invalid fields: Invalid fields");


                    return;
                }


                SetLedForFields(ledsParameter.BookFieldNames, rgbBookMove, _extendedConfiguration.FlashBookMove,
                    _extendedConfiguration.DimHelp.ToString("X"), false, "Book fields");
                _logger.LogError($"EB: Request without valid indicator set LEDs for {ledsParameter}");

                _logger.LogError($"EB: Request without valid indicator set LEDs for {ledsParameter}");
            }

        }

        public override void SetAllLedsOff(bool forceOff)
        {
            if (!EnsureConnection())
            {
                return;
            }

            _serialCommunication.Send("led off");
        }

        public override void SetAllLedsOn()
        {
            if (!EnsureConnection())
            {
                return;
            }

            for (int i = 0; i < 64; i++)
            {
                _serialCommunication.Send($"led {i} true 255 255 255");
            }
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
            _serialCommunication.Send("fen");
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
         
            if (strings[0].StartsWith("DIM:", StringComparison.InvariantCultureIgnoreCase))
            {
                var array = strings[1].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string dim = int.Parse(array[0]).ToString();
                array = strings[2].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string red = int.Parse(array[1]).ToString("X");

                array = strings[3].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string green = int.Parse(array[1]).ToString("X");

                array = strings[4].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string blue = int.Parse(array[1]).ToString("X");

                array = strings[5].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
                string clear = array[1];
                List<string> fieldNames = new List<string>();
                for (int i = 6; i < strings.Length; i++)
                {
                    fieldNames.Add(strings[i]);
                }
                lock (_lock)
                {
                    SetLedForFields(fieldNames.ToArray(), $"{red}{green}{blue}", false, dim, clear.Equals("0"), $"Additional information: {information}");
                }
            }

           
        }

        public override void RequestDump()
        {
            //
        }

        public override DataFromBoard GetPiecesFen()
        {
            var dataFromBoard = _serialCommunication.GetFromBoard();
            return dataFromBoard;
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return GetPiecesFen();
        }

        protected override void SetToNewGame()
        {
            SetAllLedsOff(true);
            SetLedForFields(new SetLEDsParameter()
            {
                FenString = FenCodes.BasePosition
            });
        }

        public override void SpeedLeds(int level)
        {
            //
        }
    }
}

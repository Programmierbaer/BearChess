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

namespace www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private class LedCorner
        {
            public bool UpperLeft = true;
            public bool UpperRight = true;
            public bool LowerLeft = true;
            public bool LowerRight = true;
        }

        private readonly bool _useChesstimation;
        private readonly bool _useElfacun;

        private readonly string[] _ledToField =
        {
            "h1", "h1h2", "h2h3", "h3h4", "h4h5", "h5h6", "h6h7", "h7h8", "h8",
            "h1g1", "h1g1h2g2", "h2g2h3g3", "h3g3h4g4", "h4g4h5g5", "h5g5h6g6",
            "h6g6h7g7", "h7g7h8g8", "h8g8",
            "g1f1", "g1f1g2f2", "g2f2g3f3", "g3f3g4f4", "g4f4g5f5", "g5f5g6f6",
            "g6f6g7f7", "g7f7g8f8", "g8f8",
            "f1e1", "f1e1f2e2", "f2e2f3e3", "f3e3f4e4", "f4e4f5e5", "f5e5f6e6",
            "f6e6f7e7", "f7e7f8e8", "f8e8",
            "e1d1", "e1d1e2d2", "e2d2e3d3", "e3d3e4d4", "e4d4e5e5", "e5d5e6d6",
            "e6d6e7d7", "e7d7e8d8", "e8d8",
            "d1c1", "d1c1d2c2", "d2c2d3c3", "d3c3d4c4", "d4c4d5c5", "d5c5d6c6",
            "d6c6d7c7", "d7c7d8c8", "d8c8",
            "c1b1", "c1b1c2b2", "c2b2c3b3", "c3b3c4b4", "c4b4c5b5", "c5b5c6b6",
            "c6b6c7b7", "c7b7c8b8", "c8b8",
            "b1a1", "b1a1b2a2", "b2a2b3a3", "b3a3b4a4", "b4a4b5a5", "b5a5b6a6",
            "b6a6b7a7", "b7a7b8a8", "b8a8",
            "a1", "a1a2", "a2a3", "a3a4", "a4a5", "a5a6", "a6a7", "a7a8", "a8"
        };

        private readonly string[] _ledUpperLeftToField =
        {
            "", "", "", "", "", "", "", "", "",
            "", "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8",
            "", "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8",
            "", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8",
            "", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8",
            "", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8",
            "", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8",
            "", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8",
            "", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"
        };

        private readonly string[] _ledUpperLeftToFieldInvert =
        {

            "", "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1",
            "", "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1",
            "", "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1",
            "", "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1",
            "", "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1",
            "", "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1",
            "", "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1",
            "", "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1",
            "", "", "", "", "", "", "", "", ""
        };

        private readonly string[] _ledUpperRightToField =
        {
            "", "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8",
            "", "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8",
            "", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8",
            "", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8",
            "", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8",
            "", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8",
            "", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8",
            "", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8",
            "", "", "", "", "", "", "", "", ""
        };

        private readonly string[] _ledUpperRightToFieldInvert =
        {
            "", "", "", "", "", "", "", "", "",
            "", "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1",
            "", "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1",
            "", "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1",
            "", "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1",
            "", "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1",
            "", "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1",
            "", "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1",
            "", "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1",
        };

        private readonly string[] _ledLowerRightToField =
        {
            "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8", "",
            "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8", "",
            "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "",
            "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "",
            "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "",
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "",
            "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "",
            "", "", "", "", "", "", "", "", ""
        };

        private readonly string[] _ledLowerRightToFieldInvert =
        {
            "", "", "", "", "", "", "", "", "",
            "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1", "",
            "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1", "",
            "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1", "",
            "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1", "",
            "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1", "",
            "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1", "",
            "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1", "",
            "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1", ""
        };

        private readonly string[] _ledLowerLeftToField =
        {
            "", "", "", "", "", "", "", "", "",
            "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8", "",
            "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8", "",
            "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "",
            "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "",
            "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "",
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "",
            "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "",
            "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", ""
        };

        private readonly string[] _ledLowerLeftToFieldInvert =
        {
            "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1", "",
            "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1", "",
            "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1", "",
            "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1", "",
            "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1", "",
            "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1", "",
            "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1", "",
            "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1", "",
            "", "", "", "", "", "", "", "", ""
        };

     


        private bool _flashSync = false;
        private string _lastSendLeds;
        private EnumFlashMode _flashMode = EnumFlashMode.FlashAsync;
        private readonly bool _showMoveLine;
        private readonly ConcurrentQueue<ProbingMove[]> _probingFields = new ConcurrentQueue<ProbingMove[]>();
        private bool _release = false;
        private string _eprom { get; set; }
        private readonly EChessBoardConfiguration _boardConfiguration;
        private int _currentEvalColor;
        private string _currentEval = "0";
        private readonly int _currentColor;
        private LedCorner _ledCorner;


        public string Version { get; private set; }
        
        public EChessBoard(ILogging logger, EChessBoardConfiguration configuration) 
        {
            _useChesstimation = configuration.UseChesstimation;
            _useElfacun = configuration.UseElfacun;
            _showMoveLine = configuration.ShowMoveLine;
            _boardConfiguration = configuration;
            _logger = logger;
            _serialCommunication = new SerialCommunication(logger, configuration.PortName);
            _serialCommunication.UseChesstimation = _useChesstimation;
            _serialCommunication.UseElfacun = _useElfacun;
            Version = string.Empty;
            _eprom = string.Empty;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            IsConnected = EnsureConnection();
            if (_useChesstimation || _useElfacun)
            {
                Information = _serialCommunication.BoardInformation;
            }
            else
            {
                Information = "Millennium " + _serialCommunication.BoardInformation;
            }
            PieceRecognition = _serialCommunication.BoardInformation != Constants.MeOne && !_useElfacun && !_useChesstimation;
            ValidForAnalyse = PieceRecognition;
            SelfControlled = false;
            MultiColorLEDs = PieceRecognition || _useElfacun || _useChesstimation;
            var probingThread = new Thread(ShowProbingMoves) { IsBackground = true };
            probingThread.Start();
            _acceptProbingMoves = true;
            _currentColor = Fields.COLOR_WHITE;
            _ledCorner = new LedCorner() { UpperLeft = true, LowerLeft = true, LowerRight = true, UpperRight = true };
        }


        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "Full";
        }

        private void ShowProbingMoves()
        {
            
            List<string> showFields = new List<string>();
            string ledForFields = string.Empty;
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
                    showFields.Clear();
                    var probingMove = fields.OrderByDescending(f => f.Score).First();
                    {
                        showFields.Add(probingMove.FieldName);
                        foreach (var field in fields)
                        {
                            if (_boardConfiguration.ShowPossibleMoves)
                            {
                                showFields.Add(field.FieldName);
                            }
                        }
                        
                    }
                    if (showFields.Count > 0)
                    {
                        ledForFields = GetLedForProbingFields(showFields.ToArray(), _boardConfiguration.ShowPossibleMovesEval ? probingMove.FieldName: string.Empty,true);
                        _serialCommunication.Send(_lastSendLeds = $"L22{ledForFields}");
                    }

                    Thread.Sleep(100);
                    continue;
                }


                Thread.Sleep(10);
            }
        }

        public override void Reset()
        {
            //
        }

        public override bool CheckComPort(string portName)
        {
            lock (_locker)
            {
                try
                {
                    _logger?.LogDebug($"CheckComPort {portName}");
                    _serialCommunication = new SerialCommunication(_logger, portName);
                    _serialCommunication.UseChesstimation = _useChesstimation;
                    _serialCommunication.UseElfacun = _useElfacun;
                    if (_serialCommunication.CheckConnect(portName))
                    {
                        _logger?.LogDebug("CheckComPort successful. Send ROM initialize ");
                        _serialCommunication.SendRawToBoard("W0000");
                        Thread.Sleep(10);
                        _serialCommunication.SendRawToBoard("W011E");
                        Thread.Sleep(10);
                        _serialCommunication.SendRawToBoard("W0203");
                        Thread.Sleep(10);
                        _serialCommunication.SendRawToBoard("W030A");
                        Thread.Sleep(10);
                        var readLine = _serialCommunication.GetRawFromBoard("V");

                        if (readLine.Length > 0 && readLine.StartsWith("v"))
                        {
                            Version = readLine;
                        }
                        else
                        {
                            return false;
                        }
                        readLine = _serialCommunication.GetRawFromBoard("R00");

                        if (readLine.Length > 0 && readLine.StartsWith("r"))
                        {
                            _eprom += readLine + " ";
                        }

                        readLine = _serialCommunication.GetRawFromBoard("R01");


                        if (readLine.Length > 0 && readLine.StartsWith("r"))
                        {
                            _eprom += readLine + " ";
                        }

                        readLine = _serialCommunication.GetRawFromBoard("R02");


                        if (readLine.Length > 0 && readLine.StartsWith("r"))
                        {
                            _eprom += readLine + " ";
                        }

                        readLine = _serialCommunication.GetRawFromBoard("R03");


                        if (readLine.Length > 0 && readLine.StartsWith("r"))
                        {
                            _eprom += readLine + " ";
                        }

                        readLine = _serialCommunication.GetRawFromBoard("R04");


                        if (readLine.Length > 0 && readLine.StartsWith("r"))
                        {
                            _eprom += readLine + " ";
                        }

                        readLine = _serialCommunication.GetRawFromBoard("R04");


                        if (readLine.Length > 0 && readLine.StartsWith("r"))
                        {
                            _eprom += readLine + " ";
                        }

                        _serialCommunication.DisConnectFromCheck();

                    }

                    _logger?.LogDebug($"C: Version: {Version}  Eprom: {_eprom}");
                    return !string.IsNullOrWhiteSpace(Version) && !string.IsNullOrWhiteSpace(_eprom);
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug($"C: Error on check: {ex.Message}");
                    return false;
                }

            }
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
            if (ledsParameter.FieldNames.Length == 0)
            {
                ledsParameter.FieldNames = ledsParameter.InvalidFieldNames.ToArray();
            }
            if (ledsParameter.FieldNames.Length==0)
            {
                ledsParameter.FieldNames = ledsParameter.BookFieldNames.ToArray();
                if (ledsParameter.FieldNames.Length == 0)
                {
                    return;
                }
            }
            if (ledsParameter.IsProbing && (_boardConfiguration.ShowPossibleMoves || _boardConfiguration.ShowPossibleMovesEval))
            {
                _logger?.LogDebug($"B: set LEDs for probing {ledsParameter}");
                _probingFields.TryDequeue(out _);
                _probingFields.Enqueue(ledsParameter.ProbingMoves);
                return;
            }   

            string ledForFields;
            _probingFields.TryDequeue(out _);
            if ((ledsParameter.FieldNames.Length == 2) && _showMoveLine)
            {
                string[] moveLine = MoveLineHelper.GetMoveLine(ledsParameter.FieldNames[0], ledsParameter.FieldNames[1]);
                _logger.LogDebug($"Extend move line: {string.Join(" ", moveLine)} ");
                ledForFields = GetLedForFields(moveLine, ledsParameter.IsThinking, _ledCorner);
            }
            else
            {
                ledForFields = GetLedForFields(ledsParameter.FieldNames, ledsParameter.IsThinking, _ledCorner);
            }

            if (!string.IsNullOrWhiteSpace(_lastSendLeds) && _lastSendLeds.Equals($"L22{ledForFields}"))
            {
                return;
            }
            _lastSendLeds = $"L22{ledForFields}";
            _logger?.LogDebug($"SendFields : {string.Join(" ", ledsParameter.FieldNames)}");
            //   lock (_locker)
            {
                _serialCommunication.Send(_lastSendLeds);
                if (ledsParameter.FieldNames.Length == 1 && (_useChesstimation || _useElfacun))
                {
                    _serialCommunication.Send("S");
                }
            }
        }


        public override void SetAllLedsOff(bool forceOff)
        {
            if (!EnsureConnection())
            {
                return;
            }
            _probingFields.TryDequeue(out _);
            // lock (_locker)
            {           
                _serialCommunication.Send("X");
            }
            if (!forceOff && _boardConfiguration.ExtendedConfig[0].ShowEvaluationValue)
            {
                ShowCurrentColorInfos();
            }
        }

        public override void SetAllLedsOn()
        {
            if (_useChesstimation || _useElfacun || !EnsureConnection())
            {
                return;
            }
            _probingFields.TryDequeue(out _);
            //  lock (_locker)
            {
                if (!_useChesstimation && !_useElfacun)
                {
                    var ledForFields = GetLedForAllFieldsOn();
                    _serialCommunication.Send($"L22{ledForFields}");
                }
            }
        }

        public override void DimLeds(bool dimLeds)
        {
            lock (_locker)
            {
                _serialCommunication?.Send(dimLeds ? "W0400" : "W041D");
            }
        }

        public override void DimLeds(int level)
        {
            lock (_locker)
            {
                if (level > 14)
                {
                    level = 14;
                }
                string hexValue = $"{level:x2}";
                _serialCommunication?.Send($"W04{hexValue.ToUpper()}");
            }
        }

        public override void SetScanTime(int scanTime)
        {
            lock (_locker)
            {
                string hexValue = $"{scanTime:x2}";
                _serialCommunication?.Send($"W01{hexValue.ToUpper()}");
            }
        }

        public override void SetDebounce(int debounce)
        {
            lock (_locker)
            {
                debounce = 3 + debounce;
               _serialCommunication?.Send($"W020{debounce}");
            }
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        public override void FlashMode(EnumFlashMode flashMode)
        {
             _flashSync = flashMode == EnumFlashMode.FlashSync;
             _flashMode = flashMode;
        }


        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            _ledCorner = new LedCorner() { UpperLeft = upperLeft, LowerLeft = lowerLeft, LowerRight = lowerRight, UpperRight = upperRight };
        }

        public override void Calibrate()
        {
            lock (_locker)
            {
                _serialCommunication.Send("S");
                IsCalibrated = true;
                SetAllLedsOn();
                Thread.Sleep(1500);
                SetAllLedsOff(true);
            }
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override void AdditionalInformation(string information)
        {
            if (_useChesstimation || _useElfacun)
            {
                return;
            }
            var strings = information.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (strings.Length == 0)
            {
                return;
            }
            _logger?.LogDebug($"Additional information: {information}");
            if (strings[0].StartsWith("SCORE:", StringComparison.InvariantCultureIgnoreCase))
            {
                _currentEvalColor = int.Parse(strings[1]);
                _currentEval = strings[2];
            }
        }

        public override void RequestDump()
        {
       //     lock (_locker)
            {
                if (!PieceRecognition)
                {
                    _serialCommunication.Send("G");
                }
            }
        }

        

        public override DataFromBoard GetPiecesFen()
        {
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }

            bool isDump = false;
            var result = string.Empty;

            var dataFromBoard = _serialCommunication.GetFromBoard();
            if (dataFromBoard.FromBoard.StartsWith("s") && dataFromBoard.FromBoard.Length == 67)
            {
                if (dataFromBoard.FromBoard.StartsWith(
                    "srnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR"))
                {
                    _playWithWhite = false;
                    
                }

                if (dataFromBoard.FromBoard.StartsWith(
                    "sRNBKQBNRPPPPPPPP................................pppppppprnbkqbnr"))
                {
                    _playWithWhite = true;
                }

                result = FenConversions.GetPiecesFen(dataFromBoard.FromBoard, true, _playWithWhite);
               
            }

            if (dataFromBoard.FromBoard.StartsWith("g") && dataFromBoard.FromBoard.Length == 67)
            {
                isDump = true;
               
                result = FenConversions.GetPiecesFen(dataFromBoard.FromBoard, false, true);
               
            }


            if (string.IsNullOrWhiteSpace(result))
            {
                return new DataFromBoard(UnknownPieceCode, dataFromBoard.Repeated);
            }

            return new DataFromBoard(result, dataFromBoard.Repeated) {IsFieldDump = isDump};

        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return GetPiecesFen();
        }

        protected override void SetToNewGame()
        {
            lock (_locker)
            {
                _probingFields.TryDequeue(out _);
                _currentEval = "0";
                if (!PieceRecognition)
                {
                    _serialCommunication.Send("S");
                }
            }
        }



        public override void Release()
        {
            _release = true;
            _probingFields.TryDequeue(out _);
        }

        public override void SetFen(string fen)
        {
          //  lock (_locker)
            {
                if (!PieceRecognition)
                {
                    var chessLinkFen = FenConversions.GetChessLinkFen(fen);
                    _serialCommunication.Send($"G{chessLinkFen}");
                    _currentEval = "0";
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

        private string GetFenLine(string substring)
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

        private string GetLedForAllFieldsOn()
        {
            var codes = string.Empty;
            return codes.PadLeft(162, 'C');
        }

        private string GetLedForAllFieldsOff()
        {
            var codes = string.Empty;
            return codes.PadLeft(162, '0');
        }

        private string GetLedForProbingFields(string[] fieldNames, string probingField, bool thinking)
        {
            var codes = string.Empty;
            var toCode = "CC";
            var fromCode = "FF";

            for (var i = 0; i < 81; i++)
            {
                var code = "00";
                for (var j = 0; j < fieldNames.Length; j++)
                {
                    if (_playWithWhite)
                    {
                        if (_ledCorner.UpperRight && _ledUpperRightToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }

                        if (_ledCorner.UpperLeft && _ledUpperLeftToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }

                        if (_ledCorner.LowerRight && _ledLowerRightToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }

                        if (_ledCorner.LowerLeft && _ledLowerLeftToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }
                    }
                    else
                    {
                        if (_ledCorner.UpperRight && _ledUpperRightToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }


                        if (_ledCorner.UpperLeft && _ledUpperLeftToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }

                        if (_ledCorner.LowerRight && _ledLowerRightToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }

                        if (_ledCorner.LowerLeft && _ledLowerLeftToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = !fieldNames[j].Equals(probingField) ? fromCode : toCode;
                            break;
                        }
                    }
                }
                codes += code;
            }

            return _useChesstimation || _useElfacun ? codes.Replace("FFFFFFFF", "FFF00FFF") : codes;
        }

        private string GetLedForFields(string[] fieldNames, bool thinking, LedCorner ledCorner)
        {
            var codes = string.Empty;
            var toCode = string.Empty;
            var fromCode = "CC";
            if (_useChesstimation || _useElfacun)
            {
                fromCode = "FF";
                toCode = "FF";
            }
            else
            {
                if (_flashMode == EnumFlashMode.NoFlash)
                {
                    if (thinking)
                    {
                        toCode = "33";
                    }
                    else
                    {
                        toCode = "FF";
                        fromCode = "FF";
                    }
                }
                else
                {
                    toCode = _flashMode == EnumFlashMode.FlashSync ? "CC" : "33";
                    if (thinking)
                    {
                        toCode = "FF";
                        fromCode = "FF";
                    }
                }
            }

            for (var i = 0; i < 81; i++)
            {
                var code = "00";
                for (var j = 0; j < fieldNames.Length; j++)
                {
                    if (_playWithWhite)
                    {
                        if (ledCorner.UpperRight && _ledUpperRightToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }

                        if (ledCorner.UpperLeft  && _ledUpperLeftToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }

                        if (ledCorner.LowerRight && _ledLowerRightToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }

                        if (ledCorner.LowerLeft  && _ledLowerLeftToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }
                    }
                    else
                    {
                        if (ledCorner.UpperRight && _ledUpperRightToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }


                        if (ledCorner.UpperLeft && _ledUpperLeftToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }

                        if (ledCorner.LowerRight && _ledLowerRightToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }

                        if (ledCorner.LowerLeft && _ledLowerLeftToFieldInvert[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;
                            break;
                        }
                    }
                }
                codes += code;
            }

            return _useChesstimation ? codes.Replace("FFFFFFFF", "FFF00FFF") : codes;
        }

        private string GetEvalFieldName(int index, decimal eval)
        {
            var dimEvalAdvantage = _boardConfiguration.ExtendedConfig[0].DimEvalAdvantage;
            
            if (index == 1)
            {
                if (dimEvalAdvantage == 0)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A1" : "A8";
                    return eval > 0 ? "A8" : "A1";
                }
                if (dimEvalAdvantage == 1)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "H1" : "A8";
                    return eval > 0 ? "A8" : "H1";
                }
                if (dimEvalAdvantage == 2)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A1" : "H1";
                    return eval > 0 ? "H1" : "A1";
                }
                if (dimEvalAdvantage == 3)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A1" : "H8";
                    return eval > 0 ? "H1" : "A1";
                }
            }
            if (index == 2)
            {
                if (dimEvalAdvantage == 0)
                {
                    if (_playWithWhite) 
                        return eval > 0 ? "A2" : "A7";
                    return eval > 0 ? "A7" : "A2";
                }
                if (dimEvalAdvantage == 1)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "H2" : "A7";
                    return eval > 0 ? "A7" : "H2";
                }
                if (dimEvalAdvantage == 2)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "B1" : "G1";
                    return eval > 0 ? "G1" : "B1";
                }
                if (dimEvalAdvantage == 3)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "B1" : "G8";
                    return eval > 0 ? "G8" : "B1";
                }
            }

            if (index == 3)
            {
                if (dimEvalAdvantage == 0)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A3" : "A6";
                    return eval > 0 ? "A6" : "A3";
                }
                if (dimEvalAdvantage == 1)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "H3" : "A6";
                    return eval > 0 ? "A6" : "H3";
                }
                if (dimEvalAdvantage == 2)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "C1" : "F1";
                    return eval > 0 ? "F1" : "C1";
                }
                if (dimEvalAdvantage == 3)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "C1" : "F8";
                    return eval > 0 ? "F8" : "C1";
                }
            }

            if (index == 4)
            {
                if (dimEvalAdvantage == 0)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A4" : "A5";
                    return eval > 0 ? "A5" : "A4";
                }
                if (dimEvalAdvantage == 1)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "H4" : "A5";
                    return eval > 0 ? "A5" : "H4";
                }
                if (dimEvalAdvantage == 2)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "D1" : "E1";
                    return eval > 0 ? "E1" : "D1";
                }
                if (dimEvalAdvantage == 3)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "D1" : "E8";
                    return eval > 0 ? "E8" : "D1";
                }
            }

            if (index == 5)
            {
                if (dimEvalAdvantage == 0)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A5" : "A4";
                    return eval > 0 ? "A4" : "A5";
                }
                if (dimEvalAdvantage == 1)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "H5" : "A4";
                    return eval > 0 ? "A4" : "H5";
                }
                if (dimEvalAdvantage == 2)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "E1" : "D1";
                    return eval > 0 ? "D1" : "E1";
                }
                if (dimEvalAdvantage == 3)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "E1" : "D8";
                    return eval > 0 ? "D8" : "E1";
                }
            }
            if (index == 6)
            {
                if (dimEvalAdvantage == 0)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A6" : "A3";
                    return eval > 0 ? "A3" : "A6";
                }
                if (dimEvalAdvantage == 1)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "H6" : "A3";
                    return eval > 0 ? "A3" : "H6";
                }
                if (dimEvalAdvantage == 2)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "F1" : "C1";
                    return eval > 0 ? "C1" : "F1";
                }
                if (dimEvalAdvantage == 3)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "F1" : "C8";
                    return eval > 0 ? "C8" : "F1";
                }
            }
            if (index == 7)
            {
                if (dimEvalAdvantage == 0)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "A7" : "A2";
                    return eval > 0 ? "A2" : "A7";
                }
                if (dimEvalAdvantage == 1)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "H7" : "A2";
                    return eval > 0 ? "A2" : "H7";
                }
                if (dimEvalAdvantage == 2)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "G1" : "B1";
                    return eval > 0 ? "B1" : "G1";
                }
                if (dimEvalAdvantage == 3)
                {
                    if (_playWithWhite)
                        return eval > 0 ? "G1" : "B8";
                    return eval > 0 ? "B8" : "G1";
                }
            }


            return "A1";
        }

        private LedCorner GetLedCorner(decimal eval)
        {
            var dimEvalAdvantage = _boardConfiguration.ExtendedConfig[0].DimEvalAdvantage;
            LedCorner ledCorner = new LedCorner()
            {
                LowerLeft = false,
                LowerRight = false,
                UpperRight = false,
                UpperLeft = false
            };
            if (eval > 0)
            {
                if (dimEvalAdvantage == 0)
                {
                    ledCorner.LowerLeft = true;
                }
                if (dimEvalAdvantage == 1)
                {
                    ledCorner.LowerRight = true;
                }
                if (dimEvalAdvantage == 2)
                {
                    ledCorner.LowerLeft = true;
                }
                if (dimEvalAdvantage == 0)
                {
                    ledCorner.LowerLeft = true;
                }
            }
            if (eval < 0)
            {
                if (dimEvalAdvantage == 0)
                {
                    ledCorner.UpperLeft = true;
                }
                if (dimEvalAdvantage == 1)
                {
                    ledCorner.UpperLeft = true;
                }
                if (dimEvalAdvantage == 2)
                {
                    ledCorner.LowerRight = true;
                }
                if (dimEvalAdvantage == 0)
                {
                    ledCorner.UpperLeft = true;
                }
            }

            return ledCorner;
        }

        private void ShowCurrentColorInfos()
        {
            if (string.IsNullOrEmpty(_currentEval))
            {
                _currentEval = "0";
            }
            if (decimal.TryParse(_currentEval.Replace(".", ","), out decimal eval))
            {
                string toField = string.Empty;
                string fromField = string.Empty;
                LedCorner ledCorner = null;
                if (_currentColor == Fields.COLOR_WHITE)
                {
                    if (eval > -1 && eval < 1)
                    {
                        return;
                    }

                    ledCorner = GetLedCorner(eval);

                    if (eval >= 1 || eval <= -1)
                    {
                        fromField = GetEvalFieldName(1,eval);
                        toField = GetEvalFieldName(1, eval);
                    }

                    if (eval >= 2 || eval <= -2)
                    {
                        toField = GetEvalFieldName(2, eval);
                    }

                    if (eval >= 3 || eval <= -3)
                    {
                        toField = GetEvalFieldName(3, eval);
                    }

                    if (eval >= 4 || eval <= -4)
                    {
                        toField = GetEvalFieldName(4, eval);
                    }

                    if (eval >= 5 || eval <= -5)
                    {
                        toField = GetEvalFieldName(5, eval);
                    }

                    if (eval >= 6 || eval <= -6)
                    {
                        toField = GetEvalFieldName(6, eval);
                    }

                    if (eval >= 7 || eval <= -7)
                    {
                        toField = GetEvalFieldName(7, eval);
                    }
                }

                if (!string.IsNullOrWhiteSpace(fromField))
                {
                    string[] moveLine = MoveLineHelper.GetMoveLine(fromField, toField);
                    _logger.LogDebug($"Extend move line: {string.Join(" ", moveLine)} ");
                    string ledForFields = GetLedForFields(moveLine, false, ledCorner);
                    _serialCommunication.Send($"L22{ledForFields}");
                }
            }
        }
    }
}
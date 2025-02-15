using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.DGTChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly bool _useClock;
        private readonly bool _clockUpperCase;
        private readonly bool _showOnlyMoves;
        private readonly bool _switchClockSide;
        private readonly bool _useBluetooth;

        private readonly byte[] _allLEDsOff = { 0x60, 0x04, 0x00, 0x40, 0x40, 0x00 };
        private readonly byte[] _resetBoard = { 64 }; // @
        private readonly byte[] _dumpBoard = { 66 };  // B
        private readonly byte[] _startReading = { 68 }; // D
        private readonly byte[] _serialNumber = { 69 }; // E
        private readonly byte[] _unknown144 = { 70 }; // F
        private readonly byte[] _requestTrademark = { 71 }; // G
        private readonly byte[] _hardwareVersion = { 72 }; // H
        private readonly byte[] _unknown143 = { 73 }; // I
        private readonly byte[] _startReadingNice = { 75 }; // I
        private readonly byte[] _batteryState = { 76 }; // L

        private readonly byte[] _requestVersion = { 77 }; // M
        private readonly byte[] _serialNumberLong = { 85 }; // U
        private readonly byte[] _unknown163 = { 86 }; // V
        private readonly byte[] _lockState = { 89 }; // Y
        private readonly byte[] _authorized = { 90 }; // Y
        private readonly byte[] _initialize = { 99, 7, 190, 245, 174, 221, 169, 95, 0 };
        private string _prevRead = string.Empty;
        private byte DGT_CLOCK_MESSAGE = 0x2b; // 43
        private byte DGT_CMD_CLOCK_ASCII = 0x0c; // 12
        private byte DGT_CMD_CLOCK_START_MESSAGE = 0x03; 
        private byte DGT_CMD_CLOCK_SETNRUN = 0x0a; // 10
        private byte DGT_CMD_CLOCK_END = 0x03; 
        private byte DGT_CMD_CLOCK_BEEP = 0x0b; // 11
        private byte DGT_CMD_CLOCK_VERSION = 0x09;
        private byte DGT_CMD_CLOCK_END_MESSAGE = 0x00;
        private byte DGT_CLOCK_3000 = 0x0c; // 12




        private readonly Dictionary<byte, int> _pieceByte2PieceCode = new Dictionary<byte, int>()
                                                                      {
                                                                          { 0, FigureId.NO_PIECE },
                                                                          { 1, FigureId.WHITE_PAWN },
                                                                          { 2, FigureId.WHITE_ROOK },
                                                                          { 3, FigureId.WHITE_KNIGHT },
                                                                          { 4, FigureId.WHITE_BISHOP },
                                                                          { 5, FigureId.WHITE_KING },
                                                                          { 6, FigureId.WHITE_QUEEN },
                                                                          { 7, FigureId.BLACK_PAWN },
                                                                          { 8, FigureId.BLACK_ROOK },
                                                                          { 9, FigureId.BLACK_KNIGHT },
                                                                          { 10, FigureId.BLACK_BISHOP },
                                                                          { 11, FigureId.BLACK_KING },
                                                                          { 12, FigureId.BLACK_QUEEN }
                                                                      };
        private static readonly Dictionary<string, string> _pieceByte2FenCode = new Dictionary<string, string>()
                                                                                {
                                                                                    { "0", "." },
                                                                                    { "1", FenCodes.WhitePawn },
                                                                                    { "2", FenCodes.WhiteRook },
                                                                                    { "3", FenCodes.WhiteKnight },
                                                                                    { "4", FenCodes.WhiteBishop },
                                                                                    { "5", FenCodes.WhiteKing },
                                                                                    { "6", FenCodes.WhiteQueen },
                                                                                    { "7", FenCodes.BlackPawn },
                                                                                    { "8", FenCodes.BlackRook },
                                                                                    { "9", FenCodes.BlackKnight },
                                                                                    { "10", FenCodes.BlackBishop },
                                                                                    { "11", FenCodes.BlackKing },
                                                                                    { "12", FenCodes.BlackQueen }
                                                                                };
        private readonly Dictionary<string, byte> _fieldName2FieldByte = new Dictionary<string, byte>()
                                                                         {   { "A8",  0 }, { "B8",  1 }, { "C8",  2 }, { "D8",  3 }, { "E8",  4 }, { "F8",  5 }, { "G8",  6 }, { "H8",  7 },
                                                                             { "A7",  8 }, { "B7",  9 }, { "C7", 10 }, { "D7", 11 }, { "E7", 12 }, { "F7", 13 }, { "G7", 14 }, { "H7", 15 },
                                                                             { "A6", 16 }, { "B6", 17 }, { "C6", 18 }, { "D6", 19 }, { "E6", 20 }, { "F6", 21 }, { "G6", 22 }, { "H6", 23 },
                                                                             { "A5", 24 }, { "B5", 25 }, { "C5", 26 }, { "D5", 27 }, { "E5", 28 }, { "F5", 29 }, { "G5", 30 }, { "H5", 31 },
                                                                             { "A4", 32 }, { "B4", 33 }, { "C4", 34 }, { "D4", 35 }, { "E4", 36 }, { "F4", 37 }, { "G4", 38 }, { "H4", 39 },
                                                                             { "A3", 40 }, { "B3", 41 }, { "C3", 42 }, { "D3", 43 }, { "E3", 44 }, { "F3", 45 }, { "G3", 46 }, { "H3", 47 },
                                                                             { "A2", 48 }, { "B2", 49 }, { "C2", 50 }, { "D2", 51 }, { "E2", 52 }, { "F2", 53 }, { "G2", 54 }, { "H2", 55 },
                                                                             { "A1", 56 }, { "B1", 57 }, { "C1", 58 }, { "D1", 59 }, { "E1", 60 }, { "F1", 61 }, { "G1", 62 }, { "H1", 63 }
                                                                         };


        private string _currentFen;

        private class BoardMove
        {
            private readonly List<string> _moveBytes = new List<string>();

            public bool Complete => _moveBytes.Count == 5;

            public void AddMoveByte(string moveByte)
            {
                _moveBytes.Add(moveByte);
            }
        }

        private class BoardDump
        {
            private readonly bool _playWithWhite;
            private readonly List<string> _boardBytes = new List<string>();
            private string fenCode = "s";
            public bool Complete => _boardBytes.Count == 67;

            public string GetFenCode => FenConversions.GetPiecesFen(fenCode, false, true);
            public string GetFenBytes => fenCode;

            public BoardDump(bool playWithWhite)
            {
                _playWithWhite = playWithWhite;
            }

            public void AddBoardByte(string moveByte)
            {
                _boardBytes.Add(moveByte);
                if (_boardBytes.Count > 2)
                {

                    if (_pieceByte2FenCode.ContainsKey(moveByte))
                    {
                        fenCode += _pieceByte2FenCode[moveByte];
                    }

                }
            }

        }

        private readonly List<BoardMove> _boardMoves = new List<BoardMove>();
        private readonly List<BoardDump> _boardDumps = new List<BoardDump>();
        private string _lastSendFields = string.Empty;
        private readonly List<byte> _currentClockBytes = new List<byte>();
        private volatile bool _waitingForClockMessage = false;
        private readonly ConcurrentQueue<List<byte>> _clockQueue = new ConcurrentQueue<List<byte>>();
        private DateTime _currentWhiteTime = DateTime.MinValue;
        private DateTime _currentBlackTime= DateTime.MinValue;
        private int? _clockHH;
        private int? _clockMM;
        private int? _clockSS;
        private bool _readingClock = false;
        private bool _readingClockTime = false;
        private bool _readingBlackTime = false;
        private int _readingClockIndex = 0;
        private int _currentColor;
        private readonly EChessBoardConfiguration _boardConfiguration;
        private string _lastDisplayString;


        public EChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _boardConfiguration = configuration;
            _useBluetooth = _boardConfiguration.UseBluetooth;
            _useClock = _boardConfiguration.UseClock;
            _clockUpperCase = _boardConfiguration.ClockUpperCase;
            _showOnlyMoves = _boardConfiguration.ClockShowOnlyMoves;
            _switchClockSide = _boardConfiguration.ClockSwitchSide;
            _serialCommunication = new SerialCommunication(logger, _boardConfiguration.PortName, _useBluetooth);
            _logger = logger;
            MultiColorLEDs = true;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = true;
            ValidForAnalyse = true;
            SelfControlled = false;
            Information = string.Empty;
            IsConnected = EnsureConnection();
            _serialCommunication.Send(_startReadingNice);
            _serialCommunication.Send(_dumpBoard);
            Information = Constants.DGT;
            _lastDisplayString = string.Empty;
            var clockThread = new Thread(HandlingClockMessages) { IsBackground = true };
            clockThread.Start();
        }


        private void HandlingClockMessages()
        {
            int counter = 0;
            while (true)
            {
                if (!_useClock)
                {
                    while (_clockQueue.TryDequeue(out _))
                    {

                    }
                    Thread.Sleep(100);
                    continue;

                }
                if (!_clockQueue.IsEmpty && ( !_waitingForClockMessage || counter>10))
                {
                    _waitingForClockMessage = true;
                    counter = 0;
                    if (_clockQueue.TryDequeue(out List<byte> sendList))
                    {
                        _logger?.LogDebug($"DGT: Handle clock: {ConvertFromRead(sendList.ToArray())}");
                        try
                        {
                            _serialCommunication.Send(sendList.ToArray());
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError($"DGT: Handle clock: {ex.Message}");
                        }
                    }
                }
                Thread.Sleep(100);
                if (!_clockQueue.IsEmpty)
                {
                    counter++;
                }
            }
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = true;
            ValidForAnalyse = true;
            SelfControlled = false;
            Information = Constants.DGT;
        }

        public override void SetEngineColor(int color)
        {
            //
        }

        public override void Reset()
        {
            SendClockToNormal();
            SendDisplayToClock("Bear");
        }

        public override bool CheckComPort(string portName)
        {
            var serialCommunication = new SerialCommunication(_logger, portName, _useBluetooth);
            if (serialCommunication.CheckConnect(portName))
            {
                var readLine = serialCommunication.GetRawFromBoard(string.Empty);
                _logger?.LogDebug($"Read from raw: {readLine}");
                serialCommunication.DisConnectFromCheck();
                return readLine.Length > 0;
            }

            return false;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return CheckComPort(portName);
        }

        public override void SetLedForFields(SetLEDsParameter ledsParameter)
        {

            if (ledsParameter.FieldNames == null || ledsParameter.FieldNames.Length == 0)
            {
                return;
            }
            List<byte> allBytes = new List<byte>();
            string sendFields = string.Join(" ", ledsParameter.FieldNames);
            if (sendFields.Equals(_lastSendFields))
            {
                return;
            }

            _lastSendFields = sendFields;
            _logger?.LogDebug($"DGT: Set LED for fields: {_lastSendFields} IsThinking: {ledsParameter.IsThinking}");

            //if (thinking && fieldNamesLength > 1)
            //{
            //    SetLedForFields(new string[] { fieldNames[0], fieldNames[0] }, thinking, isMove, displayString);
            //    SetLedForFields(new string[] { fieldNames[1], fieldNames[1] }, thinking, isMove, displayString);
            //    return;
            //}
            allBytes.Add(0x60);
            allBytes.Add(0x04);
            allBytes.Add(0x01);
            string fieldName = ledsParameter.FieldNames[0];
            if (_fieldName2FieldByte.ContainsKey(fieldName.ToUpper()))
            {
                allBytes.Add(_fieldName2FieldByte[fieldName.ToUpper()]);
            }

            if (ledsParameter.FieldNames.Length > 1)
            {
                fieldName = ledsParameter.FieldNames[1];
            }
            if (_fieldName2FieldByte.ContainsKey(fieldName.ToUpper()))
            {
                allBytes.Add(_fieldName2FieldByte[fieldName.ToUpper()]);
            }
            allBytes.Add(0);
            _serialCommunication.Send(allBytes.ToArray());
            if ((ledsParameter.IsMove || ledsParameter.IsThinking) && ledsParameter.FieldNames.Length == 2)
            {
                if (!_lastDisplayString.Equals(ledsParameter.DisplayString))
                {
                    SendDisplayToClock(_clockUpperCase
                        ? ledsParameter.DisplayString.ToUpper()
                        : ledsParameter.DisplayString);
                    _lastDisplayString = ledsParameter.DisplayString;
                }

                if (ledsParameter.IsEngineMove)
                {
                    SendClockBeep();
                }
                
            }
            if (_useBluetooth)
            {
                _serialCommunication.Send(_batteryState);
            }
        }



        public override void SetAllLedsOff(bool forceOff)
        {
            _serialCommunication.Send(_allLEDsOff);
            SendClockToNormal();
        }

        public override void SetAllLedsOn()
        {

            SetLedForFields(new SetLEDsParameter()
                            {
                                FieldNames = new string[] { "A1", "H8" },

                            });
            SetLedForFields(new SetLEDsParameter()
                            {
                                FieldNames = new string[] { "A8", "H1" },

                            });
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
            // ignore
        }

        public override void SetDebounce(int debounce)
        {
            // ignore
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
            SendDisplayToClock("* Bear *");
        }

        public override void SendInformation(string message)
        {
            SendDisplayToClock(message);
        }

        public override void AdditionalInformation(string information)
        {
            //
        }

        public override void RequestDump()
        {
            lock (_locker)
            {
                _serialCommunication.SendRawToBoard(_dumpBoard);
            }
        }

        public override DataFromBoard GetPiecesFen()
        {
            
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }


            DataFromBoard dataFromBoard = null;
            while (true)
            {
                dataFromBoard = _serialCommunication.GetFromBoard();
                if (dataFromBoard.FromBoard.Trim().Length > 1)
                {
                    break;

                }

                Thread.Sleep(5);
            }
            if (!string.IsNullOrWhiteSpace(dataFromBoard.FromBoard))
            {
                var strings = dataFromBoard.FromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strings.Length; i++)
                {
                    if (strings[i] == "142")
                    {
                        var boardMove = new BoardMove();
                        boardMove.AddMoveByte(strings[i]);
                        _boardMoves.Add(boardMove);
                        continue;
                    }

                    if (_boardMoves.Count > 0)
                    {
                        var boardMove = _boardMoves[_boardMoves.Count - 1];
                        boardMove.AddMoveByte(strings[i]);
                        if (boardMove.Complete)
                        {
                            _boardMoves.Clear();
                            RequestDump();
                        }

                        continue;
                    }


                    if (strings[i] == "134")
                    {
                        var boardDump = new BoardDump(_playWithWhite);
                        boardDump.AddBoardByte(strings[i]);
                        _boardDumps.Add(boardDump);
                        continue;
                    }

                    if (strings[i] == "141")
                    {
                        _waitingForClockMessage = false;
                        _readingClock = true;
                        _readingClockTime = false;
                        _clockHH = null;
                        _clockMM = null;
                        _clockSS = null;
                        _readingClockIndex = 1;
                        _readingBlackTime = true;
                        continue;
                    }

                    if (_readingClock)
                    {
                        _readingClockIndex++;

                        if (_readingClockIndex > 10)
                        {
                            _readingClock = false;
                            _readingClockIndex = 0;
                            _readingClockTime = false;
                            _clockHH = null;
                            _clockMM = null;
                            _clockSS = null;
                            continue;
                        }

                        if (strings[i] == "10" && !_readingClockTime )
                        {
                            _readingClockTime = true;
                            continue;
                        }
                        if (strings[i] == "10" && _readingClockTime && _readingClockIndex==4)
                        {
                            _readingClock = false;
                            _readingClockIndex = 0;
                            _readingClockTime = false;
                            _clockHH = null;
                            _clockMM = null;
                            _clockSS = null;
                            continue;
                        }

                        if (_readingClockTime)
                        {
                            if (_clockHH == null)
                            {
                                if (int.TryParse(strings[i], out int hh))
                                {
                                    if (int.TryParse($"{hh:X}", result: out int hh2))
                                    {
                                        _clockHH = hh2;
                                    }
                                }
                                continue;
                            }
                            if (_clockMM == null)
                            {
                                if (int.TryParse(strings[i], out int mm))
                                {
                                    if (int.TryParse($"{mm:X}", result: out int mm2))
                                    {
                                        _clockMM = mm2;
                                    }
                                    
                                }
                                continue;
                            }
                            if (_clockSS == null)
                            {
                                if (int.TryParse(strings[i], out int ss))
                                {
                                    if (int.TryParse($"{ss:X}", result: out int ss2))
                                    {
                                        _clockSS = ss2;
                                    }
                                }
                            }

                            if (_readingBlackTime)
                            {
                                try
                                {
                                    _currentBlackTime =
                                        new DateTime(2000, 1, 1, _clockHH.Value, _clockMM.Value, _clockSS.Value);
                                    _clockHH = null;
                                    _clockMM = null;
                                    _clockSS = null;
                                    _readingBlackTime = false;
                                }
                                catch
                                {
                                    _readingClock = false;
                                    _readingClockIndex = 0;
                                    _readingClockTime = false;
                                    _clockHH = null;
                                    _clockMM = null;
                                    _clockSS = null;
                                }

                                continue;
                            }

                            {
                                try
                                {
                                    _currentWhiteTime =
                                        new DateTime(2000, 1, 1, _clockHH.Value, _clockMM.Value, _clockSS.Value);
                                    _clockHH = null;
                                    _clockMM = null;
                                    _clockSS = null;
                                }
                                catch
                                {
                                    _readingClock = false;
                                    _readingClockIndex = 0;
                                    _readingClockTime = false;
                                    _clockHH = null;
                                    _clockMM = null;
                                    _clockSS = null;
                                }

                                continue;
                            }
                        }
                    }
                    if (_boardDumps.Count > 0)
                    {
                        var boardMove = _boardDumps[_boardDumps.Count - 1];
                        boardMove.AddBoardByte(strings[i]);
                        if (boardMove.Complete)
                        {
                            _boardMoves.Clear();
                            if (boardMove.GetFenBytes.StartsWith(
                                    "srnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR"))
                            {
                                _playWithWhite = true;

                            }

                            if (boardMove.GetFenBytes.StartsWith(
                                    "sRNBKQBNRPPPPPPPP................................pppppppprnbkqbnr"))
                            {
                                _playWithWhite = false;
                            }
                            
                            //result = FenConversions.GetPiecesFen(dataFromBoard.FromBoard, true, _playWithWhite);
                            try
                            {
                                _currentFen = FenConversions.GetPiecesFen(boardMove.GetFenBytes, false, _playWithWhite);
                                //_currentFen = boardMove.GetFenCode;
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError($"{ex.Message}: Fen: {boardMove.GetFenBytes}");
                                _currentFen = string.Empty;
                            }

                            
                            return new DataFromBoard(_currentFen, 3);
                        }

                    }

                }
            }
            return new DataFromBoard(_currentFen ?? string.Empty, 3);
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
            //
        }

        public override void SetFen(string fen)
        {
            //
        }

        public override void StopClock()
        {
            if (_currentClockBytes.Count == 0 || _showOnlyMoves)
            {
                return;
            }
            _logger?.LogDebug("DGT: Stop clock");
            List<byte> tmpSend = new List<byte>(new byte[]
                                                {
                                                    DGT_CLOCK_MESSAGE,
                                                    0x0a,
                                                    DGT_CMD_CLOCK_START_MESSAGE,
                                                    DGT_CMD_CLOCK_SETNRUN,
                                                    (byte)(_currentWhiteTime.Hour),
                                                    (byte)_currentWhiteTime.Minute,
                                                    (byte)_currentWhiteTime.Second,
                                                    (byte)(_currentBlackTime.Hour),
                                                    (byte)_currentBlackTime.Minute,
                                                    (byte)_currentBlackTime.Second
                                                });
            tmpSend.Add(0x04);
            tmpSend.Add(DGT_CMD_CLOCK_END_MESSAGE);
            _clockQueue.Enqueue(tmpSend);
        }

        public override void StartClock(bool white)
        {
            if (_currentClockBytes.Count == 0 || _showOnlyMoves)
            {
                return;
            }
            _logger?.LogDebug($"DGT: Start clock for white: {white} ");
            List<byte> tmpSend = new List<byte>(new byte[]
                                                {
                                                    DGT_CLOCK_MESSAGE,
                                                    0x0a,
                                                    DGT_CMD_CLOCK_START_MESSAGE,
                                                    DGT_CMD_CLOCK_SETNRUN,
                                                    (byte)_currentWhiteTime.Hour,
                                                    (byte)_currentWhiteTime.Minute,
                                                    (byte)_currentWhiteTime.Second,
                                                    (byte)_currentBlackTime.Hour,
                                                    (byte)_currentBlackTime.Minute,
                                                    (byte)_currentBlackTime.Second
                                                });
            if (_switchClockSide)
            {
                tmpSend.Add(white ? (byte)0x02: (byte)(0x01));
            }
            else
            {
                tmpSend.Add(white ? (byte)0x01 : (byte)(0x02));
            }

            tmpSend.Add(DGT_CMD_CLOCK_END_MESSAGE);
            _clockQueue.Enqueue(tmpSend);
        }

        public override void DisplayOnClock(string display)
        {
            SendDisplayToClock(display);
        }

        public override void SetCurrentColor(int currentColor)
        {
            _currentColor = currentColor;
        }

        public override event EventHandler BasePositionEvent;
        public override event EventHandler NewGamePositionEvent;
        public override event EventHandler<string> DataEvent;
        public override event EventHandler HelpRequestedEvent;

        public override void SetClock(int hourWhite, int minuteWhite, int secondWhite, int hourBlack, int minuteBlack, int secondBlack)
        {
            if (_showOnlyMoves)
            {
                return;
            }
            _logger?.LogDebug("DGT: Set clock");
            _currentClockBytes.Clear();
            _currentWhiteTime = _switchClockSide ? new DateTime(2000, 1, 1, hourBlack, minuteBlack, secondBlack) : new DateTime(2000, 1, 1, hourWhite, minuteWhite, secondWhite);
            _currentBlackTime = _switchClockSide ? new DateTime(2000, 1, 1, hourWhite, minuteWhite, secondWhite) : new DateTime(2000, 1, 1, hourBlack, minuteBlack, secondBlack);
            if (hourWhite + hourBlack + minuteWhite + minuteBlack + secondWhite + secondBlack == 0)
            {
               // return;
            }
            _currentClockBytes.AddRange(new byte[]
                                        {
                                            DGT_CLOCK_MESSAGE,
                                            0x0a,
                                            DGT_CMD_CLOCK_START_MESSAGE,
                                            DGT_CMD_CLOCK_SETNRUN,
                                            (byte)hourWhite,
                                            (byte)minuteWhite,
                                            (byte)secondWhite,
                                            (byte)hourBlack,
                                            (byte)minuteBlack,
                                            (byte)secondBlack
                                        });
            
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        private void SendClockToNormal()
        {
            _logger?.LogDebug("DGT: Clock to normal");
            List<byte> allBytes = new List<byte>
                                  {
                                      DGT_CLOCK_MESSAGE,
                                      0x03,
                                      DGT_CMD_CLOCK_START_MESSAGE,
                                      DGT_CMD_CLOCK_END,
                                      DGT_CMD_CLOCK_END_MESSAGE
                                  };
            _clockQueue.Enqueue(allBytes);
            
        }

        private void SendClockBeep()
        {
            if (_boardConfiguration.ClockBeep)
            {
                List<byte> allBytes = new List<byte>
                {
                    DGT_CLOCK_MESSAGE,
                    0x04,
                    DGT_CMD_CLOCK_START_MESSAGE,
                    DGT_CMD_CLOCK_BEEP,
                    (byte)_boardConfiguration.BeepDuration,
                    DGT_CMD_CLOCK_END_MESSAGE
                };
                _clockQueue.Enqueue(allBytes);
            }
        }

        private void SendClockRequestVersion()
        {
            List<byte> allBytes = new List<byte>
                                  {
                                      DGT_CLOCK_MESSAGE,
                                      0x03,
                                      DGT_CMD_CLOCK_START_MESSAGE,
                                      DGT_CMD_CLOCK_VERSION,
                                      DGT_CMD_CLOCK_END_MESSAGE
                                  };
            _clockQueue.Enqueue(allBytes);
        }

        private void SendDisplayToClock(string displayString)
        {
            if (string.IsNullOrWhiteSpace(displayString))
            {
                return;
            }

            if (displayString.Length > 8)
            {
                displayString = displayString.Substring(0, 8);
            }
            _logger?.LogDebug($"DGT: Display: {displayString}");
            List<byte> allBytes = new List<byte>
                                  {
                                      DGT_CLOCK_MESSAGE,
                                      DGT_CLOCK_3000,
                                      DGT_CMD_CLOCK_START_MESSAGE,
                                      DGT_CMD_CLOCK_ASCII
                                  };

            var bytes = Encoding.ASCII.GetBytes(displayString.PadRight(8));
            foreach (var b in bytes)
            {
                allBytes.Add(b);
            }

            allBytes.Add(0);
            allBytes.Add(DGT_CMD_CLOCK_END_MESSAGE);
            _clockQueue.Enqueue(allBytes);
        }

        private string ConvertFromRead(byte[] bArray)
        {
            string r = string.Empty;
            foreach (var b in bArray)
            {
                r = r + b + " ";
            }

            return r;
        }
    }
}

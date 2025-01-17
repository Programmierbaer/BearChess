using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.HoSChessBoard
{
    public class EChessBoard : AbstractEBoard
    {

        
        public override event EventHandler BasePositionEvent;
        public override event EventHandler NewGamePositionEvent;
        public override event EventHandler HelpRequestedEvent;
        public override event EventHandler<string> DataEvent;

        private bool _release = false;
        private readonly EChessBoardConfiguration _boardConfiguration;
        private readonly bool _useBluetooth;
        private string _lastData = string.Empty;
        private string _lastResult = string.Empty;
        private const string _basePositionString = "00432551346666666600000000000000000000000000000000CCCCCCCCA98BB79A";

        private static readonly Dictionary<string, byte> _adaptedFieldName = new Dictionary<string, byte>()
                                                                            {
                                                                                { "A1", 100 },
                                                                                { "B1", 101 },
                                                                                { "C1", 102 },
                                                                                { "D1", 103 },
                                                                                { "E1", 104 },
                                                                                { "F1", 105 },
                                                                                { "G1", 106 },
                                                                                { "H1", 107 },
                                                                                { "A2", 108 },
                                                                                { "B2", 109 },
                                                                                { "C2", 110 },
                                                                                { "D2", 111 },
                                                                                { "E2", 112 },
                                                                                { "F2", 113 },
                                                                                { "G2", 114 },
                                                                                { "H2", 115 },
                                                                                { "A3", 116 },
                                                                                { "B3", 117 },
                                                                                { "C3", 118 },
                                                                                { "D3", 119 },
                                                                                { "E3", 120 },
                                                                                { "F3", 121 },
                                                                                { "G3", 122 },
                                                                                { "H3", 123 },
                                                                                { "A4", 124 },
                                                                                { "B4", 125 },
                                                                                { "C4", 126 },
                                                                                { "D4", 127 },
                                                                                { "E4", 128 },
                                                                                { "F4", 129 },
                                                                                { "G4", 130 },
                                                                                { "H4", 131 },
                                                                                { "A5", 132 },
                                                                                { "B5", 133 },
                                                                                { "C5", 134 },
                                                                                { "D5", 135 },
                                                                                { "E5", 136 },
                                                                                { "F5", 137 },
                                                                                { "G5", 138 },
                                                                                { "H5", 139 },
                                                                                { "A6", 140 },
                                                                                { "B6", 141 },
                                                                                { "C6", 142 },
                                                                                { "D6", 143 },
                                                                                { "E6", 144 },
                                                                                { "F6", 145 },
                                                                                { "G6", 146 },
                                                                                { "H6", 147 },
                                                                                { "A7", 148 },
                                                                                { "B7", 149 },
                                                                                { "C7", 150 },
                                                                                { "D7", 151 },
                                                                                { "E7", 152 },
                                                                                { "F7", 153 },
                                                                                { "G7", 154 },
                                                                                { "H7", 155 },
                                                                                { "A8", 156 },
                                                                                { "B8", 157 },
                                                                                { "C8", 158 },
                                                                                { "D8", 159 },
                                                                                { "E8", 160 },
                                                                                { "F8", 161 },
                                                                                { "G8", 162 },
                                                                                { "H8", 163 },
                                                                            };
        private ConcurrentBag<byte> _ledBag = new ConcurrentBag<byte>();
        private static object _lock = new object();

        
        public EChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _boardConfiguration = configuration;
            _useBluetooth = configuration.UseBluetooth;            
            _logger = logger;
            MultiColorLEDs = true;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            _serialCommunication = new SerialCommunication(logger, configuration.PortName, _useBluetooth);
            Information = Constants.Zmartfun;
            _acceptProbingMoves = false;
            var handleLedThread = new Thread(LedThreadHandle) { IsBackground = true };
            handleLedThread.Start();
        }
        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            Information = Constants.Zmartfun;
        }

        private void LedThreadHandle()
        {
            byte[] array;
            while (true)
            {
                
                lock (_lock)
                {
                    array = _ledBag.ToArray();
                }

                if (array.Length == 0)
                {
                    Thread.Sleep(30);
                }

                foreach (var b in array)
                {
                    _serialCommunication.Send(new byte[] { 0, b });
                    Thread.Sleep(30);
                }
            }
        }

        public override void AdditionalInformation(string information)
        {
            // 
        }

        public override void Calibrate()
        {

            if (!EnsureConnection())
            {
                return;
            }
            
        }

        public override bool CheckComPort(string portName)
        {
            return true;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return true;
        }

        public override void DimLeds(bool dimLeds)
        {
            //
        }

        public override void DimLeds(int level)
        {
            //
        }

        public override void DisplayOnClock(string display)
        {
            //
        }

        public override void FlashMode(EnumFlashMode flashMode)
        {
            //
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return GetPiecesFen();
        }

        public override DataFromBoard GetPiecesFen()
        {
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }

            var dataFromBoard = _serialCommunication.GetFromBoard();

            if (dataFromBoard.FromBoard.Equals(_lastData) || dataFromBoard.FromBoard.StartsWith("FF") || dataFromBoard.FromBoard.Length<66)
            {
//                return new DataFromBoard(_lastResult, dataFromBoard.Repeated);
            }
            _lastData = dataFromBoard.FromBoard;
            if (string.IsNullOrWhiteSpace(_lastData))
            {
                return new DataFromBoard(_lastResult, dataFromBoard.Repeated);
            }
            return new DataFromBoard(ConvertToFen(_lastData), 3);

        }

        public override void Release()
        {
            _release = true;
        }

        public override void RequestDump()
        {
            //
        }

        public override void Reset()
        {
            //
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override void SetAllLedsOff(bool forceOff)
        {
            lock (_lock)
            {
                while (_ledBag.TryTake(out _))
                {

                }
            }

            _serialCommunication.ClearToBoard();
            _serialCommunication.Send(new[] { (byte)0, (byte)186 }, forceOff);
        }

        public override void SetAllLedsOn()
        {
            //SetLedForFields(new SetLEDsParameter() {FieldNames = new []{"E4","E5","D5","D4"}});
            //Thread.Sleep(1000);
            //SetAllLedsOff(true);
        }

        public override void SetClock(int hourWhite, int minuteWhite, int secWhite, int hourBlack, int minuteBlack, int secondBlack)
        {
            //
        }

        public override void SetCurrentColor(int currentColor)
        {
            //
        }

        public override void SetDebounce(int debounce)
        {
            //
        }

        public override void SetEngineColor(int color)
        {
            //
        }

        public override void SetFen(string fen)
        {
            //
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            //
        }

        public override void SetLedForFields(SetLEDsParameter ledsParameter)
        {


            if (ledsParameter.FieldNames.Length == 0 && ledsParameter.HintFieldNames.Length == 0 &&
                ledsParameter.InvalidFieldNames.Length == 0)
            {
                return;
                SetAllLedsOff(true);
            }
            else
            {
                lock (_lock)
                {
                    while (_ledBag.TryTake(out _))
                    {

                    }
                }

                _serialCommunication.ClearToBoard();
                
            }

            lock (_lock)
            {
                if (ledsParameter.IsProbing)
                {

                    var firstOrDefault = ledsParameter.ProbingMoves.OrderByDescending(p => p.Score).FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        if (_adaptedFieldName.ContainsKey(firstOrDefault.FieldName))
                        {
                            var fieldNumber = _adaptedFieldName[firstOrDefault.FieldName];
                            _ledBag.Add(fieldNumber);
                        }
                    }
                }
                else
                {
                    if (ledsParameter.FieldNames.Length > 0)
                    {
                        foreach (var parameterFieldName in ledsParameter.FieldNames)
                        {
                            if (_adaptedFieldName.ContainsKey(parameterFieldName.ToUpper()))
                            {
                                var fieldNumber = _adaptedFieldName[parameterFieldName.ToUpper()];
                                _ledBag.Add(fieldNumber);
                            }
                        }
                    }
                    else
                    {
                        foreach (var parameterFieldName in ledsParameter.InvalidFieldNames)
                        {
                            if (_adaptedFieldName.ContainsKey(parameterFieldName.ToUpper()))
                            {
                                var fieldNumber = _adaptedFieldName[parameterFieldName.ToUpper()];
                                _ledBag.Add(fieldNumber);
                            }
                        }

                        foreach (var parameterFieldName in ledsParameter.HintFieldNames)
                        {
                            if (_adaptedFieldName.ContainsKey(parameterFieldName.ToUpper()))
                            {
                                var fieldNumber = _adaptedFieldName[parameterFieldName.ToUpper()];
                                _ledBag.Add(fieldNumber);
                            }
                        }
                    }
                }
            }
        }

        public override void SetScanTime(int scanTime)
        {
            //
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        public override void StartClock(bool white)
        {
            //
        }

        public override void StopClock()
        {
            //
        }

        protected override void SetToNewGame()
        {
            SetAllLedsOff(true);
        }


        private string ConvertToFen(string fromBoard)
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            try
            {
                for (int i = 0; i < Fields.BoardFields.Length; i += 2)
                {
                    // Skip leading 00 and swap position
                    chessBoard.SetFigureOnPosition(ConvertPieceCode(fromBoard[i + 3]), Fields.BoardFields[i]);
                    chessBoard.SetFigureOnPosition(ConvertPieceCode(fromBoard[i + 2]), Fields.BoardFields[i + 1]);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Invalid position data received",ex);
            }

            return chessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
        }
        private int ConvertPieceCode(char boardCode)
        {
            switch (boardCode)
            {
                case '6': return FigureId.WHITE_PAWN;
                case '3': return FigureId.WHITE_ROOK;
                case '4': return FigureId.WHITE_KNIGHT;
                case '5': return FigureId.WHITE_BISHOP;
                case '2': return FigureId.WHITE_QUEEN;
                case '1': return FigureId.WHITE_KING;
                case 'C': return FigureId.BLACK_PAWN;
                case '9': return FigureId.BLACK_ROOK;
                case 'A': return FigureId.BLACK_KNIGHT;
                case 'B': return FigureId.BLACK_BISHOP;
                case '8': return FigureId.BLACK_QUEEN;
                case '7': return FigureId.BLACK_KING;
                default: return FigureId.NO_PIECE;
            }
        }
    }
}

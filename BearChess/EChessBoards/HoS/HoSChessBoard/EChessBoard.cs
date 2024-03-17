using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                                                                                { "A2", 101 },
                                                                                { "A3", 102 },
                                                                                { "A4", 103 },
                                                                                { "A5", 104 },
                                                                                { "A6", 105 },
                                                                                { "A7", 106 },
                                                                                { "A8", 107 },
                                                                                { "B1", 108 },
                                                                                { "B2", 109 },
                                                                                { "B3", 110 },
                                                                                { "B4", 111 },
                                                                                { "B5", 112 },
                                                                                { "B6", 113 },
                                                                                { "B7", 114 },
                                                                                { "B8", 115 },
                                                                                { "C1", 116 },
                                                                                { "C2", 117 },
                                                                                { "C3", 118 },
                                                                                { "C4", 119 },
                                                                                { "C5", 120 },
                                                                                { "C6", 121 },
                                                                                { "C7", 122 },
                                                                                { "C8", 123 },
                                                                                { "D1", 124 },
                                                                                { "D2", 125 },
                                                                                { "D3", 126 },
                                                                                { "D4", 127 },
                                                                                { "D5", 128 },
                                                                                { "D6", 129 },
                                                                                { "D7", 130 },
                                                                                { "D8", 131 },
                                                                                { "E1", 132 },
                                                                                { "E2", 133 },
                                                                                { "E3", 134 },
                                                                                { "E4", 135 },
                                                                                { "E5", 136 },
                                                                                { "E6", 137 },
                                                                                { "E7", 138 },
                                                                                { "E8", 139 },
                                                                                { "F1", 140 },
                                                                                { "F2", 141 },
                                                                                { "F3", 142 },
                                                                                { "F4", 143 },
                                                                                { "F5", 144 },
                                                                                { "F6", 145 },
                                                                                { "F7", 146 },
                                                                                { "F8", 147 },
                                                                                { "G1", 148 },
                                                                                { "G2", 149 },
                                                                                { "G3", 150 },
                                                                                { "G4", 151 },
                                                                                { "G5", 152 },
                                                                                { "G6", 153 },
                                                                                { "G7", 154 },
                                                                                { "G8", 155 },
                                                                                { "H1", 156 },
                                                                                { "H2", 157 },
                                                                                { "H3", 158 },
                                                                                { "H4", 159 },
                                                                                { "H5", 160 },
                                                                                { "H6", 161 },
                                                                                { "H7", 162 },
                                                                                { "H8", 163 },
                                                                            };





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
        }
        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "Full";
            Information = Constants.Zmartfun;
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
            Information = _serialCommunication.DeviceName;
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
                return new DataFromBoard(_lastResult, dataFromBoard.Repeated);
            }
            _lastData = dataFromBoard.FromBoard;
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
            _serialCommunication.Send(new []{(byte)186});
        }

        public override void SetAllLedsOn()
        {
            for (byte b = 100; b < 164; b++)
            {
                _serialCommunication.Send(new[] { b });
            }
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
            foreach (var ledsParameterFieldName in ledsParameter.FieldNames)
            {
                if (_adaptedFieldName.ContainsKey(ledsParameterFieldName.ToUpper())) 
                {
                    byte fieldNumber = _adaptedFieldName[ledsParameterFieldName.ToUpper()];
                    _serialCommunication.Send(new byte[] { fieldNumber });
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChess.ChessUpChessBoard
{
    public  class EChessBoard : AbstractEBoard
    {
        private readonly bool _useBluetooth;
        private readonly bool _showMoveLine;
        private bool _flashSync = false;
        private bool _release = false;
        private readonly byte[] _lastSendBytes = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private string _prevJoinedString = string.Empty;
        private string _lastFromBoardString = string.Empty;
        private int _prevLedField = 0;
        private byte _requestPosition = 0x67;
        private byte _resetGame = 0x64;
        private byte _gameSettings = 0xB9;
        private byte _sendMove = 0x99;
        private byte _gameType = 2;
        private byte _whiteType = 0;
        private byte _whiteLevel = 1;
        private byte _whiteButtonLock = 1;
        private byte _blackType = 0;
        private byte _blackLevel = 1;
        private byte _blackButtonLock = 1;
        private byte _hintLimit = 0;
        private byte _whiteRemote = 0;
        private byte _blackRemote = 1;
        private byte _deviceUserWhite = 0;
        private byte _deviceUserBlack = 1;
        private byte _confirmMove = 0x21;
        private byte _boardInformation = 0xB2;
        private byte _sendFen = 0x66;
        private byte _sendPromote = 0x97;
        private DataFromBoard _dumpDataFromBoard;
        private DataFromBoard _lastDataFromBoard;

        private const string _basePositionString =
            "103 1 2 3 4 5 3 2 1 0 0 0 0 0 0 0 0 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 8 8 8 8 8 8 8 8 9 10 11 12 13 11 10 9";


      

        public override event EventHandler BasePositionEvent;
        public override event EventHandler NewGamePositionEvent;
        public override event EventHandler HelpRequestedEvent;
        public override event EventHandler<string> DataEvent;
     

        public EChessBoard(string basePath, ILogging logger, EChessBoardConfiguration configuration)
        {
            _useBluetooth = configuration.UseBluetooth;
            _showMoveLine = configuration.ShowMoveLine;
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "\ud83d\udd0b";
            _serialCommunication = new SerialCommunication(logger, configuration.PortName, _useBluetooth);
            Information = "ChessUp";
            SelfControlled = true;
            PieceRecognition = false;
            ValidForAnalyse = false;
            IsConnected = EnsureConnection();


        }
        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth)
        {
            _useBluetooth = useBluetooth;
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "\ud83d\udd0b";
            _serialCommunication = new SerialCommunication(logger, portName, useBluetooth);
            Information = "ChessUp";
            SelfControlled = true;
            PieceRecognition = false;
            ValidForAnalyse = false;
            IsConnected = EnsureConnection();
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "---";
            BatteryStatus = "\ud83d\udd0b";
            Information = "ChessUp";
            SelfControlled = true;
            PieceRecognition = false;
            ValidForAnalyse = false;
            IsConnected = EnsureConnection();
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
            try
            {
                IsConnected = EnsureConnection();
                if (!IsConnected)
                {
                    return;
                }

                var split = fen.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                byte[] asciiBytes = Encoding.ASCII.GetBytes(fen);
                string hex = BitConverter.ToString(asciiBytes);
                string allBytes = "66";
                var strings = hex.Split("-".ToCharArray());
                allBytes += $" {Convert.ToString(strings.Length - 1,16)}";
                for (int i = 0; i < strings.Length-4; i++)
                {
                    allBytes += " " + strings[i];
                }

                string fullMove = int.Parse(split[split.Length - 1]).ToString("X2");
                string halfMove = int.Parse(split[split.Length - 2]).ToString("X2");
                allBytes += $" 00 {halfMove} {fullMove}";
                _serialCommunication.Send(allBytes.ToLower());
            }
            catch
            {
                //
            }
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

    
        public override bool CheckComPort(string portName)
        {
            return true;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return true;
        }
        public override void SetEngineColor(int color)
        {
            
          //  _serialCommunication.Send(new byte[] { _resetGame });
            if (color == Fields.COLOR_BLACK)
            {
                _serialCommunication.Send(new byte[]
                {
                    _gameSettings, _gameType, _whiteType, _whiteLevel, _whiteButtonLock, _blackType, _blackLevel,
                    _blackButtonLock, _hintLimit, _whiteRemote, _blackRemote, _deviceUserWhite
                });
            }
            if (color == Fields.COLOR_WHITE)
            {
                _serialCommunication.Send(new byte[]
                {
                    _gameSettings, _gameType, _whiteType, _whiteLevel, _whiteButtonLock, _blackType, _blackLevel,
                    _blackButtonLock, _hintLimit, _blackRemote, _whiteRemote, _deviceUserBlack
                });
            }
           
        }
        public override void SetLedForFields(SetLEDsParameter ledsParameter)
        {
            IsConnected = EnsureConnection();
            if (!IsConnected)
            {
                return;
            }
            if (ledsParameter.FieldNames.Length == 2 && !ledsParameter.IsThinking)
            {
                _serialCommunication.Send(new byte[] { _sendMove, ConvertFieldToCode(ledsParameter.FieldNames[0]), ConvertFieldToCode(ledsParameter.FieldNames[1])});
                if (!string.IsNullOrEmpty(ledsParameter.Promote))
                {
                    _serialCommunication.Send(new byte[] { _sendPromote, ConvertPromoteToBoardCode(ledsParameter.Promote.ToLower()) });
                }

            }
       

        }

        public override void SetAllLedsOff(bool forceOff)
        {
            //
        }

        public override void SetAllLedsOn()
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

        public override void Calibrate()
        {
            try
            {
                IsConnected = EnsureConnection();
                if (!IsConnected)
                {
                    return;
                }
                _serialCommunication.Send(new byte[] { _requestPosition });
                _serialCommunication.Send(new byte[] { _boardInformation });
            }
            catch
            {
                //
            }
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override void AdditionalInformation(string information)
        {
            //
        }

        public override void RequestDump()
        {
            try
            {
                IsConnected = EnsureConnection();
                if (!IsConnected)
                {
                    return;
                }

                _serialCommunication.Send(new byte[] { _requestPosition });
            }
            catch
            {
                //
            }
        }

        public override DataFromBoard GetPiecesFen()
        {
            IsConnected = EnsureConnection();
            if (!IsConnected)
            {
                return new DataFromBoard("", 3);
            }

            var dataFromBoard = _serialCommunication.GetFromBoard();
            BatteryLevel = _serialCommunication.BatteryLevel;

            if (!_lastFromBoardString.Equals(dataFromBoard.FromBoard))
            {

                _lastFromBoardString = dataFromBoard.FromBoard;

                if (dataFromBoard.FromBoard.StartsWith("103 "))
                {
                    string fromBoard = ConvertToFen(dataFromBoard.FromBoard);
                    _logger?.LogDebug($"Position information received: {fromBoard} ");

                    _dumpDataFromBoard = new DataFromBoard(fromBoard, 3)
                        { IsFieldDump = true, BasePosition = dataFromBoard.FromBoard.StartsWith(_basePositionString) };
                    _lastDataFromBoard = new DataFromBoard(fromBoard, 3)
                        { IsFieldDump = false, BasePosition = dataFromBoard.FromBoard.StartsWith(_basePositionString) };
                    if (_lastDataFromBoard.BasePosition)
                    {
                        BasePositionEvent?.Invoke(this, null);
                    }
                }


                if (dataFromBoard.FromBoard.StartsWith("163 53 "))
                {
                    _logger?.LogDebug($"Move received: {ConvertCodeToMove(dataFromBoard.FromBoard)}");
                    _logger?.LogDebug($"Pieces changed: {_lastFromBoardString} => Request new position");
                    _serialCommunication.Send(new byte[] { _requestPosition }, true);
                }

                if (dataFromBoard.FromBoard.StartsWith("49 "))
                {
                    BatteryStatus = dataFromBoard.FromBoard.Trim().EndsWith("1") ? "\ud83d\udd0c" : "\ud83d\udd0b";
                }
                if (dataFromBoard.FromBoard.StartsWith("36"))
                {
                    _logger?.LogDebug("Received settings ok");
                }
                if (dataFromBoard.FromBoard.StartsWith("100"))
                {
                    _logger?.LogDebug("Received reset");
                }
                // 178 0 1 9 0 1 0 0 13 239 54 208 1 4 0 1 
                if (dataFromBoard.FromBoard.StartsWith("178"))
                {
                    Information = BuildBoardInformation(dataFromBoard.FromBoard);

                }

                if (dataFromBoard.FromBoard.StartsWith("184 ") && dataFromBoard.FromBoard.Trim().EndsWith("5"))
                {
                    _logger?.LogDebug("SC: Help requested");
                    //
                }
            }

            return _lastDataFromBoard ?? new DataFromBoard("", 3);
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return _dumpDataFromBoard ?? new DataFromBoard("", 3);
        }

        protected override void SetToNewGame()
        {
            try
            {
                IsConnected = EnsureConnection();
                if (!IsConnected)
                {
                    return;
                }
                _serialCommunication.Send(new byte[] { _resetGame });
                //_serialCommunication.Send(new byte[] { _gameSettings,2,0,1,1,0,1,1,0,1,1,0 });
                _serialCommunication.Send(new byte[] { _gameSettings,_gameType,_whiteType,_whiteLevel,_whiteButtonLock,_blackType,_blackLevel,_blackButtonLock,_hintLimit,_whiteRemote,_blackRemote,_deviceUserWhite });
            }
            catch
            {
                //
            }
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        private string BuildBoardInformation(string information)
        {
            // 178 0 1 9 0 1 0 0 13 239 54 208 1 4 0 1 
            
            
                // 190 Firmware
                // 100 BLE Firmware
                // 140 Boottloader
                // 13 239 54 208
                // DEF36D0
                var strings = information.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length == 16)
                {
                    string firmware = $"{strings[2]}.{strings[3]}.{strings[4]}";
                    string BLEfirmware = $"{strings[5]}.{strings[6]}.{strings[7]}";
                    string bootLoader = $"{strings[12]}.{strings[13]}.{strings[14]}";
                    string version = Convert.ToInt32(strings[8] + strings[9] + strings[10] + strings[11],16).ToString();
                    return
                        $"ChessUp{Environment.NewLine}Firmware: {firmware}{Environment.NewLine}BLE-Firmware: {BLEfirmware}{Environment.NewLine}Bootloader: {bootLoader}{Environment.NewLine} Serial#: {version}";
                }

                return "ChessUp";
        }

        private byte ConvertFieldToCode(string fieldName)
        {
            var fieldNumber = Fields.GetFieldNumber(fieldName);
            return (byte)Array.IndexOf(Fields.BoardFields, fieldNumber);
        }
        private string ConvertCodeToMove(string codes)
        {

            string[] row = { "a", "b", "c", "d", "e", "f", "g", "h" };
            var strings = codes.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length==6)
            {
                try
                {
                    int fromRow = int.Parse(row[2]);
                    int toRow = int.Parse(row[4]);
                    return $"{fromRow}{strings[3]}{toRow}{strings[5]}";
                }
                catch
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private string ConvertToFen(string fromBoard)
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
           
            var strings = fromBoard.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Fields.BoardFields.Length; i++)
            {
                chessBoard.SetFigureOnPosition(ConvertPieceCode(strings[i + 1]), Fields.BoardFields[i]);
            }
            chessBoard.CurrentColor = strings[65].Equals("0") ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
            if (strings[66].Equals("1"))
            {
                chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Short);
            }
            if (strings[67].Equals("1"))
            {
                chessBoard.CanCastling(Fields.COLOR_WHITE, CastlingEnum.Long);
            }
            if (strings[68].Equals("1"))
            {
                chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Short);
            }
            if (strings[69].Equals("1"))
            {
                chessBoard.CanCastling(Fields.COLOR_BLACK, CastlingEnum.Long);
            }

            return chessBoard.GetFenPosition().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private int ConvertPieceCode(string boardCode)
        {
            switch (boardCode)
            {
                case "0": return FigureId.WHITE_PAWN;
                case "1": return FigureId.WHITE_ROOK;
                case "2": return FigureId.WHITE_KNIGHT;
                case "3": return FigureId.WHITE_BISHOP;
                case "4": return FigureId.WHITE_QUEEN;
                case "5": return FigureId.WHITE_KING;
                case "8": return FigureId.BLACK_PAWN;
                case "9": return FigureId.BLACK_ROOK;
                case "10": return FigureId.BLACK_KNIGHT;
                case "11": return FigureId.BLACK_BISHOP;
                case "12": return FigureId.BLACK_QUEEN;
                case "13": return FigureId.BLACK_KING;
                default: return FigureId.NO_PIECE;
            }
        }

        private byte ConvertPromoteToBoardCode(string promote)
        {
            switch (promote)
            {
                case "r": return 1;
                case "n": return 2;
                case "b": return 3;
                case "q": return 4;
                default: return 4;
            }
        }
    }
}

using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard
{
    public class EChessBoard : AbstractEBoard
    {

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


        private bool _flashSync = false;
        private string _lastSendLeds;
        private bool _upperLeft = true;
        private bool _upperRight = true;
        private bool _lowerLeft = true;
        private bool _lowerRight = true;

        public EChessBoard(ILogging logger, bool isFirstInstance, string portName)
        {
            _logger = logger;
            _serialCommunication = new SerialCommunication(isFirstInstance, logger, portName);
            _isFirstInstance = isFirstInstance;
            IsConnected = EnsureConnection();
        }

        public EChessBoard(ILogging logger)
        {
            _isFirstInstance = true;
            _logger = logger;
        }

        public override bool CheckComPort(string portName)
        {
            lock (_locker)
            {
                _serialCommunication = new SerialCommunication(true, _logger, portName);
                if (_serialCommunication.CheckConnect(portName))
                {
                    var readLine = _serialCommunication.GetRawFromBoard();
                    _serialCommunication.DisConnectFromCheck();
                    return readLine.Length > 0;
                }

                return false;
            }
        }

        public override void SetLedForFields(string[] fieldNames)
        {
            if (!EnsureConnection()) return;
            var ledForFields = GetLedForFields(fieldNames);
            _lastSendLeds = $"L22{ledForFields}";
            lock (_locker)
            {
                _serialCommunication.Send(_lastSendLeds);
            }
        }

        public override void SetLastLeds()
        {
            if (!EnsureConnection()) return;
            lock (_locker)
            {
                _serialCommunication.Send(_lastSendLeds);
            }
        }

        public override void SetAllLedsOff()
        {
            if (!EnsureConnection()) return;
            lock (_locker)
            {
                _serialCommunication.Send("X");
            }
        }

        public override void SetAllLedsOn()
        {
            if (!EnsureConnection()) return;
            lock (_locker)
            {
                var ledForFields = GetLedForAllFieldsOn();
                _serialCommunication.Send($"L22{ledForFields}");
            }
        }

        public override void DimLeds(bool dimLeds)
        {
            lock (_locker)
            {
                _serialCommunication?.Send(dimLeds ? "W0400" : "W041D");
            }
        }

        public override void FlashSync(bool flashSync)
        {
            _flashSync = flashSync;
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            _upperLeft = upperLeft;
            _upperRight = upperRight;
            _lowerLeft = lowerLeft;
            _lowerRight = lowerRight;
        }

        public override void Calibrate()
        {
            lock (_locker)
            {
                _serialCommunication.Send("S");
                IsCalibrated = true;
            }
        }

        public override DataFromBoard GetPiecesFen()
        {
            if (!EnsureConnection())
            {
                return new DataFromBoard(string.Empty);
            }

            var result = string.Empty;
            //lock (_locker)

            var dataFromBoard = _serialCommunication.GetFromBoard();
            if (dataFromBoard.FromBoard.StartsWith("s") && dataFromBoard.FromBoard.Length == 67)
            {
                for (int i = 57; i > 0; i -= 8)
                {
                    var substring = dataFromBoard.FromBoard.Substring(i, 8);
                    result += GetFenLine(substring);
                }
            }


            if (string.IsNullOrWhiteSpace(result))
            {
                return new DataFromBoard(UnknownPieceCode, dataFromBoard.Repeated);
            }

            return new DataFromBoard(result.Substring(0, result.Length - 1), dataFromBoard.Repeated);

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

        private string GetLedForFields(string[] fieldNames)
        {
            var codes = string.Empty;
            var toCode = _flashSync ? "CC" : "33";
            for (var i = 0; i < 81; i++)
            {
                var code = "00";
                for (var j = 0; j < fieldNames.Length; j++)
                {
                    if ( _upperRight && _ledUpperRightToField[i].Contains(fieldNames[j].ToLower()))
                    {
                        code = j == 0 ? "CC" : toCode;

                        break;
                    }

                    if (_upperLeft && _ledUpperLeftToField[i].Contains(fieldNames[j].ToLower()))
                    {
                        code = j == 0 ? "CC" : toCode;

                        break;
                    }

                    if (_lowerRight && _ledLowerRightToField[i].Contains(fieldNames[j].ToLower()))
                    {
                        code = j == 0 ? "CC" : toCode;

                        break;
                    }

                    if (_lowerLeft &&  _ledLowerLeftToField[i].Contains(fieldNames[j].ToLower()))
                    {
                        code = j == 0 ? "CC" : toCode;

                        break;
                    }
                }

                codes += code;
            }

            return codes;
        }
    }
}
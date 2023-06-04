using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.IChessOneChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly bool _useBluetooth;
        private bool _release = false;
        private ConcurrentQueue<string[]> _flashFields = new ConcurrentQueue<string[]>();
        private readonly string[] _allLEDSOn  = { "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF" };
        private readonly string[] _allLEDSOff = { "00", "00", "00", "00", "00", "00", "00", "00", "00", "00" };
        //private readonly string[] _LEDSForWhiteOnMove = { "0F", "FF", "FF", "00", "00", "00", "00", "00", "00", "00", "00", "00", "51", "00"};
        private readonly string[] _LEDSForWhiteOnMove = { "00", "00", "00", "00", "00", "00", "00", "00", "00", "FF"};
        private readonly string[] _LEDSForBlackOnMove = { "FF", "00", "00", "00", "00", "00", "00", "00", "00", "00" };
        private readonly string _startReadingCmd = "CPIRQ";
        //private readonly string _startReadingCmd = "CPM1000";
        private readonly string _stopReadingCmd = "CPOFF";
        private readonly string _statusOffCmd = "CSOFF";
        private readonly string _simpleColor = "ELS";
        private readonly string _advancedColor = "ELA";

      

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

        private readonly Dictionary<Fields.Lines, int> _linesToCol = new Dictionary<Fields.Lines, int>()
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
        private readonly string[] _allData = new string[32];
        private int _allIndex = 0;
        private bool _readingPosition = false;
        private int _currentColor;

        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth)
        {
            _useBluetooth = useBluetooth;
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            _serialCommunication = new SerialCommunication(logger, portName, useBluetooth);
            Information = "IChessOne";
            var thread = new Thread(FlashLeds) { IsBackground = true };
            thread.Start();
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            Information = "IChessOne";
        }

        public override void SetCurrentColor(int currentColor)
        {
            return;
            _currentColor = currentColor;
            byte[] convertToArray = ConvertToArray("ELS");
            byte[] stringToByteArray = StringToByteArray(string.Join("", currentColor==Fields.COLOR_WHITE ? _LEDSForWhiteOnMove : _LEDSForBlackOnMove));
            _serialCommunication.Send(convertToArray.Concat(stringToByteArray).ToArray());
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
                    List<byte> inits = new List<byte>() { 0x0A, 0x08 };
                    inits.AddRange(result);
                    _serialCommunication.Send(inits.ToArray());

                }

                Thread.Sleep(500);
            }
        }

        private byte[] UpdateLedsForField(string fieldName, byte[] current)
        {
            // Exact two letters expected, e.g. "E2"
            if (string.IsNullOrWhiteSpace(fieldName) || fieldName.Length != 2)
            {
                return current;
            }

            if (!int.TryParse(fieldName[1].ToString(), out int _))
            {
                return current;
            }
            // Don't manipulate parameters
            byte[] result = { 0, 0, 0, 0, 0, 0, 0, 0 };
            Array.Copy(current, result, current.Length);
            if (_playWithWhite)
            {
              //  result[8 - rowNumber] |= _colName2ColByte[colName];
            }
            else
            {
              //  result[rowNumber - 1] |= _flippedColName2ColByte[colName];
            }
            return result;
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
//            _serialCommunication.Send(ConvertToArray(_stopReadingCmd));
            _serialCommunication.Send(ConvertToArray(_statusOffCmd));
        }

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
            return true;
        }

        public override bool CheckComPort(string portName, string baud)
        {
            return true;
        }

        public override void SetLedForFields(string[] fieldNames, string promote, bool thinking, bool isMove,
                                             string displayString)
        {
            string[] allLedsOff = new string[_allLEDSOff.Length];
            _allLEDSOff.CopyTo(allLedsOff, 0);
            int[] sumCols = { 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var fieldName in fieldNames)
            {
                var fieldNumber = Fields.GetFieldNumber(fieldName);
                var row = Fields.GetRow(fieldNumber);
                Fields.Lines line = Fields.GetLine(fieldNumber);
                sumCols[row - 1] += _linesToCol[line];
            }

            allLedsOff[9] = $"FF";
            allLedsOff[8] = $"{sumCols[0]:X2}";
            allLedsOff[7] = $"{sumCols[1]:X2}";
            allLedsOff[6] = $"{sumCols[2]:X2}";
            allLedsOff[5] = $"{sumCols[3]:X2}";
            allLedsOff[4] = $"{sumCols[4]:X2}";
            allLedsOff[3] = $"{sumCols[5]:X2}";
            allLedsOff[2] = $"{sumCols[6]:X2}";
            allLedsOff[1] = $"{sumCols[7]:X2}";
            byte[] startCode = ConvertToArray(_advancedColor);
        //    byte[] startCode = ConvertToArray(_simpleColor);
            byte[] fieldLEDS = StringToByteArray(string.Join("", allLedsOff));
            //byte[] colorFields = !isMove ? StringToByteArray(string.Join("", new string[] {"80","0F"})) : StringToByteArray(string.Join("", new string[] { "8F", "00" }));
            byte[] colorFields = StringToByteArray(string.Join("", new string[] {"80","F0"}));
            byte[] flashFields = StringToByteArray(string.Join("", new string[] { "51", "AA" }));
            
            {
                _serialCommunication.Send(startCode.Concat(colorFields).Concat(fieldLEDS).Concat(flashFields).ToArray());
             //   _serialCommunication.Send(startCode.Concat(fieldLEDS).ToArray());
            }
            
        }

        public override void SetLastLeds()
        {
            //
        }

        public override void SetAllLedsOff()
        {
            byte[] convertToArray = ConvertToArray(_simpleColor);
            byte[] stringToByteArray = StringToByteArray(string.Join("", _allLEDSOff));
            _serialCommunication.Send(convertToArray.Concat(stringToByteArray).ToArray());
        }

        public override void SetAllLedsOn()
        {
            byte[] convertToArray = ConvertToArray(_simpleColor);
            byte[] stringToByteArray = StringToByteArray(string.Join("", _allLEDSOn));
            _serialCommunication.Send(convertToArray.Concat(stringToByteArray).ToArray());
        }
        private byte[] ConvertToArray(string param)
        {

            return Encoding.ASCII.GetBytes(param);
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
            if (!EnsureConnection())
            {
                return;
            }
            _serialCommunication.Send(ConvertToArray("CLC001"));
            _serialCommunication.Send(ConvertToArray(_startReadingCmd));
            _serialCommunication.Send(ConvertToArray("RP"));
           
        }

        public override void SendInformation(string message)
        {
           //
        }

        public override void RequestDump()
        {
            //
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
                if (allData[i].Equals("3D") && _allIndex==0)
                {
                    _readingPosition = true;
                    continue;
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
                        _logger?.LogError($"Unknown code {pCode}");
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
            _serialCommunication.Send(ConvertToArray("RP"));
            //_serialCommunication.Send(ConvertToArray("RH"));
        }

        public override void SpeedLeds(int level)
        {
            //
        }

        private static string GetPiecesFen(string boardFen, bool playWithWhite)
        {
            string result = string.Empty;


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
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.IChessOneChessBoard
{
    public class EChessBoard : AbstractEBoard
    {
        private readonly bool _useBluetooth;
        private bool _release = false;
        private ConcurrentQueue<string[]> _flashFields = new ConcurrentQueue<string[]>();
        private readonly string[] _allLEDSOn = { "ELS", "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF", "FF" };
        private readonly string[] _allLEDSOff = { "ELS", "00", "00", "00", "00", "00", "00", "00", "00", "00", "00" };
        private readonly string _startReading = "CPIRQ";
        //private readonly string _startReading = "CPM1000";
        private readonly string _stopReading = "CPOFF";

        public override event EventHandler BasePositionEvent;
        public override event EventHandler<string> DataEvent;
        public override event EventHandler HelpRequestedEvent;

        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth)
        {
            _useBluetooth = useBluetooth;
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            _serialCommunication = new SerialCommunication(logger, portName, useBluetooth);
            Information = "IChess One";
            var thread = new Thread(FlashLeds) { IsBackground = true };
            thread.Start();
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "100";
            BatteryStatus = "Full";
            Information = "IChess One";
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

            var colName = fieldName[0].ToString().ToUpper();
            if (!int.TryParse(fieldName[1].ToString(), out int rowNumber))
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

        public override void Reset()
        {
          //
        }

        public override void Release()
        {
            _release = true;
            _serialCommunication.Send(_stopReading);
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

        public override void SetLedForFields(string[] fieldNames, string promote, bool thinking, bool isMove, string displayString)
        {
          //
        }

        public override void SetLastLeds()
        {
            //
        }

        public override void SetAllLedsOff()
        {
           //_serialCommunication.Send(string.Join("",_allLEDSOff));
        }

        public override void SetAllLedsOn()
        {
           // _serialCommunication.Send(string.Join("", _allLEDSOn));
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
            _serialCommunication.Send(_startReading);
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
            return new DataFromBoard(string.Empty, 3);
        }

        public override DataFromBoard GetDumpPiecesFen()
        {
            return new DataFromBoard(string.Empty, 3);
        }

        protected override void SetToNewGame()
        {
            //
        }

        public override void SpeedLeds(int level)
        {
            //
        }
    }
}

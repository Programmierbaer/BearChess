using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.UCBChessBoard
{
    public class EChessBoard : AbstractEBoard
    {

        public EChessBoard(string basePath, ILogging logger, string portName, bool useBluetooth, string boardName)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            Information = boardName;
        }

        public EChessBoard(ILogging logger)
        {
            _logger = logger;
            BatteryLevel = "--";
            BatteryStatus = "";
            PieceRecognition = false;
            Information = string.Empty;
        }

        public override void Reset()
        {
            //
        }

        public override bool CheckComPort(string portName)
        {
            return true;
        }

        public override void SetLedForFields(string[] fieldNames, bool thinking, bool isMove, string displayString)
        {
            lock (_locker)
            {
                if (fieldNames.Length > 1)
                {
                    _serialCommunication.Send($"M{fieldNames[0]}-{fieldNames[1]}");
                }
            }
        }

        public override void SetLastLeds()
        {
            //
        }

        public override void SetAllLedsOff()
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

        public override void FlashSync(bool flashSync)
        {
           //
        }

        public override void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            //
        }

        public override void Calibrate()
        {
            //
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

            bool isDump = false;
            var result = string.Empty;

            var dataFromBoard = _serialCommunication.GetFromBoard();
          


            if (string.IsNullOrWhiteSpace(result))
            {
                return new DataFromBoard(UnknownPieceCode, dataFromBoard.Repeated);
            }

            return new DataFromBoard(result, dataFromBoard.Repeated) { IsFieldDump = isDump };
        }

        protected override void SetToNewGame()
        {
            lock (_locker)
            {
                _serialCommunication.Send("New Game");
            }
        }

        protected override void Release()
        {
            //
        }

        public override void SetFen(string fen)
        {
            //
        }

        public override void SetClock(int hourWhite, int minuteWhite, int minuteSec, int hourBlack, int minuteBlack, int secondBlack)
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

        public override void SpeedLeds(int level)
        {
            //
        }
    }
}

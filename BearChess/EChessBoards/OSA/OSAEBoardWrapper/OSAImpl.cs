using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.OSAEBoardWrapper
{
    public class OSAImpl : AbstractEBoardWrapper
    {
        public OSAImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public OSAImpl(string name, string basePath, string comPortName, string baud, bool useBluetooth, EChessBoardConfiguration configuration) : base(
            name, basePath, comPortName, baud, useBluetooth, true, false, false, false,configuration)
        {
        }

       

        public override bool Calibrate()
        {
            _stop = true;
            SetAllLedsOn();
            Thread.Sleep(1000);
            _board.Calibrate();
            SetAllLedsOff(false);
            _stop = false;
            return true;
        }

        public override void SendInformation(string message)
        {
            _board?.SendInformation(message);
        }

        public override void DimLEDs(bool dimLeds)
        {
            //
        }

        public override void DimLEDs(int level)
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
            _board.FlashMode(flashMode);
        }

    
        protected override IEBoard GetEBoard()
        {
            return new OSAChessBoard.EChessBoard(_fileLogger, _comPortName, _baud, _useBluetooth, Name);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new OSAChessBoard.EChessBoard(_fileLogger);
        }
    }
}

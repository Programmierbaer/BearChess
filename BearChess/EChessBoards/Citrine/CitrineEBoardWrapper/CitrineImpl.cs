using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;


namespace www.SoLaNoSoft.com.BearChess.CitrineEBoardWrapper
{
    public class CitrineImpl : AbstractEBoardWrapper
    {
        public CitrineImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public CitrineImpl(string name, string basePath, string comPortName, bool useBluetooth) : base(
            name, basePath, comPortName, useBluetooth, true, false, false)
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
            return new CitrineChessBoard.EChessBoard(basePath: _basePath, logger: _fileLogger, portName: _comPortName, _useBluetooth, Name);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new CitrineChessBoard.EChessBoard(logger: _fileLogger);
        }
    }
}

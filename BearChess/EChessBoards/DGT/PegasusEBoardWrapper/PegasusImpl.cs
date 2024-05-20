using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.PegasusEBoardWrapper
{
    public class PegasusImpl : AbstractEBoardWrapper
    {


        public PegasusImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public PegasusImpl(string name, string basePath, string comPortName, bool useBluetooth) : base(
            name, basePath,  comPortName, useBluetooth, true, false, false)
        {

        }

        public PegasusImpl(string name, string basePath, EChessBoardConfiguration configuration) : base(
            name, basePath, configuration)
        {

        }


        public override void SetScanTime(int scanTime)
        {
            //
        }

        public override void SetDebounce(int debounce)
        {
            _board.SetDebounce(debounce);
        }

        public override void FlashMode(EnumFlashMode flashMode)
        {
            _board.FlashMode(flashMode);
        }

      

        protected override IEBoard GetEBoard()
        {
            return new PegasusChessBoard.PegasusEChessBoard(_basePath, _fileLogger, _configuration);
            //return new PegasusChessBoard.EChessBoard(_basePath, _fileLogger, _configuration);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            
            return new PegasusChessBoard.PegasusEChessBoard(logger: _fileLogger);
            // return new PegasusChessBoard.EChessBoard(logger: _fileLogger);
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
            //
        }

        public override void DimLEDs(bool _)
        {
            // Ignore
        }

        public override void DimLEDs(int level)
        {
            _board.DimLeds(level);
        }
    }
}

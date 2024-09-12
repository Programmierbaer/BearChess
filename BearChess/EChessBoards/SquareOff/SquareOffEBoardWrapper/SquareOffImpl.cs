using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.SquareOffEBoardWrapper
{
    public class SquareOffImpl : AbstractEBoardWrapper
    {


        public SquareOffImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public SquareOffImpl(string name, string basePath,string comPortName, bool useBluetooth) : base(
            name, basePath, comPortName, useBluetooth, true, false, false)
        {

        }

        public SquareOffImpl(string name, string basePath, EChessBoardConfiguration configuration) :
            base(name, basePath, configuration)
        {

        }

        public override void SetScanTime(int scanTime)
        {
            _board?.SetScanTime(scanTime);
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
            return new SquareOffChessBoard.EChessBoard(basePath: _basePath, logger: _fileLogger,  portName: _comPortName, _useBluetooth,Name);
        }

        protected override IEBoard GetEBoard(bool check)
        {

            return new SquareOffChessBoard.EChessBoard(logger: _fileLogger);
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

        public override string RequestInformation(string message)
        {
            return string.Empty;
        }

        public override void DimLEDs(bool _)
        {
            // Ignore
        }

        public override void DimLEDs(int _)
        {
            // Ignore
        }
    }
}
using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.SquareOffEBoardWrapper
{
    public class SquareOffImpl : AbstractEBoardWrapper
    {


        public SquareOffImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public SquareOffImpl(string name, string basePath, bool isFirstInstance, string comPortName, bool useBluetooth) : base(
            name, basePath, isFirstInstance, comPortName, useBluetooth, true, false, false)
        {

        }

        public override void FlashInSync(bool flashInSync)
        {
            _board.FlashSync(flashInSync);
        }

        protected override IEBoard GetEBoard()
        {
            return new SquareOffChessBoard.EChessBoard(basePath: _basePath, logger: _fileLogger, isFirstInstance: _isFirstInstance, portName: _comPortName, _useBluetooth,Name);
        }

        protected override IEBoard GetEBoard(bool check)
        {

            return new SquareOffChessBoard.EChessBoard(logger: _fileLogger);
        }

        public override void Calibrate()
        {
            _stop = true;
            SetAllLedsOn();
            Thread.Sleep(1000);
            _board.Calibrate();
            SetAllLedsOff();
            _stop = false;
        }

        public override void SendInformation(string message)
        {
            //
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
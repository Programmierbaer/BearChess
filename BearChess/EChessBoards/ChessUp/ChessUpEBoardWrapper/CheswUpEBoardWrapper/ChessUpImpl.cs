using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.ChessUpEBoardWrapper
{
    public class ChessUpImpl : AbstractEBoardWrapper
    {

        public ChessUpImpl(string name, string basePath) : base(name, basePath)
        {

        }

        public ChessUpImpl(string name, string basePath, string comPortName, bool useBluetooth) : base(
            name, basePath, comPortName, useBluetooth, false, false, false)
        {

        }

        public ChessUpImpl(string name, string basePath, EChessBoardConfiguration configuration) : base(
            name, basePath, configuration)
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
            // throw new System.NotImplementedException();
        }

        public override void DimLEDs(bool dimLEDs)
        {
            //throw new System.NotImplementedException();
        }

        public override void DimLEDs(int level)
        {
            //throw new System.NotImplementedException();
        }

        public override void SetScanTime(int scanTime)
        {
            // ignore
        }

        public override void SetDebounce(int debounce)
        {
            // ignore
        }

        public override void FlashMode(EnumFlashMode flashMode)
        {
            //throw new System.NotImplementedException();
        }


        protected override IEBoard GetEBoard()
        {
            return new ChessUpChessBoard.EChessBoard(_basePath, _fileLogger, _configuration);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new ChessUpChessBoard.EChessBoard(_fileLogger);
        }
    }
}

using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.EBoardWrapper
{
    public class CernoImpl : AbstractEBoardWrapper
    {


        public CernoImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public CernoImpl(string name, string basePath, string comPortName, bool useBluetooth) : base(
            name, basePath, comPortName, useBluetooth, true, false, false)
        {

        }

        public CernoImpl(string name, string basePath, EChessBoardConfiguration configuration) :
            base(name, basePath, configuration)
        {

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
            _board.FlashMode(flashMode);
        }

       
        protected override IEBoard GetEBoard()
        {
            return new Cerno.ChessBoard.EChessBoard(_basePath, _fileLogger, _configuration);

        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new Cerno.ChessBoard.EChessBoard( logger: _fileLogger);
        }

        public override bool Calibrate()
        {
            _stop = true;
            SetAllLedsOn();
            Thread.Sleep(1000);
            _board.Calibrate();
            SetAllLedsOff(false);
            _stop = false;
            return _board.IsCalibrated;
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

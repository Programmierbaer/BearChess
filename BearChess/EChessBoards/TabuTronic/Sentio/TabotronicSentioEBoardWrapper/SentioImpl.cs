using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;


namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.EBoardWrapper
{
    public class SentioImpl : AbstractEBoardWrapper
    {


        public SentioImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public SentioImpl(string name, string basePath, string comPortName, bool useBluetooth) : base(
            name, basePath, comPortName, useBluetooth, true, false, false)
        {

        }

        public SentioImpl(string name, string basePath, EChessBoardConfiguration configuration) :
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
            return new ChessBoard.ESentioChessBoard(_basePath, _fileLogger, _configuration);
            //return new ChessBoard.ESentioChessBoard(_basePath, _fileLogger, _configuration);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new ChessBoard.ESentioChessBoard(_fileLogger);
            //return new ChessBoard.EChessBoard(_fileLogger);
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

        public override void DimLEDs(int _)
        {
            // Ignore
        }
    }
}

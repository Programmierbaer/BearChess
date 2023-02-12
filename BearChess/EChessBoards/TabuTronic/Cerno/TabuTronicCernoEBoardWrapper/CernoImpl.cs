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

        public override void SetScanTime(int scanTime)
        {
            // ignore
        }

        public override void SetDebounce(int debounce)
        {
            // ignore
        }

        public override void FlashInSync(bool flashInSync)
        {
            _board.FlashSync(flashInSync);
        }

        protected override IEBoard GetEBoard()
        {
            return new Cerno.ChessBoard.EChessBoard(basePath: _basePath,logger: _fileLogger, 
                                   portName: _comPortName, _useBluetooth);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new Cerno.ChessBoard.EChessBoard( logger: _fileLogger);
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

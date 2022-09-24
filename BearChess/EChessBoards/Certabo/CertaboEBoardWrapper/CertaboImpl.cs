using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.CertaboEBoardWrapper
{
    public class CertaboImpl : AbstractEBoardWrapper
    {


        public CertaboImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public CertaboImpl(string name, string basePath, string comPortName, bool useBluetooth) : base(
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
            return new CertaboChessBoard.EChessBoard(basePath: _basePath,logger: _fileLogger, 
                                   portName: _comPortName, _useBluetooth);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new CertaboChessBoard.EChessBoard( logger: _fileLogger);
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

using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.ChessnutEBoardWrapper
{
    public class ChessnutAirImpl : AbstractEBoardWrapper
    {
     


        public ChessnutAirImpl(string name, string basePath) : base(name, basePath)
        {
            
        }

        public ChessnutAirImpl(string name, string basePath, bool isFirstInstance, string comPortName, bool useBluetooth) : base(
            name, basePath, isFirstInstance, comPortName, useBluetooth, false, false, false)
        {

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

        public override void FlashInSync(bool flashSync)
        {
            //throw new System.NotImplementedException();
        }

        protected override IEBoard GetEBoard()
        {
            return new ChessnutChessBoard.EChessBoard(_basePath, _fileLogger, _isFirstInstance, _comPortName, _useBluetooth);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new ChessnutChessBoard.EChessBoard(_fileLogger);
        }
    }
}

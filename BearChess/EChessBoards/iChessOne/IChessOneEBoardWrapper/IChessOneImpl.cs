using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.IChessOneEBoardWrapper
{
    public class IChessOneImpl : AbstractEBoardWrapper
    {

        public IChessOneImpl(string name, string basePath) : base(name, basePath)
        {

        }

      

        public IChessOneImpl(string name, string basePath, EChessBoardConfiguration configuration) : base(
            name, basePath, configuration)
        {
        }

        public override bool Calibrate()
        {
            _stop = true;
            SetAllLedsOn();
            Thread.Sleep(1000);
            _board.Calibrate();
            SetAllLedsOff(true);
            _stop = false;
            return true;
        }

        public override void SendInformation(string message)
        {
            _board.SendInformation(message);
        }

        public override string RequestInformation(string message)
        {
            return string.Empty;
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

        public override void BuzzerOnConnected()
        {
            _board.BuzzerOnConnected();
        }


        public override void BuzzerOnMove()
        {
            _board.BuzzerOnMove();
        }
        public override void BuzzerOnCheck()
        {
            _board.BuzzerOnCheck();
        }

        public override void BuzzerOnDraw()
        {
            _board.BuzzerOnDraw();
        }
        public override void PlayBuzzer(string soundCode)
        {
            _board.PlayBuzzer(soundCode);
        }

        public override void BuzzerOnCheckMate()
        {
            _board.BuzzerOnCheckMate();
        }

        public override void BuzzerOnInvalid()
        {
            _board.BuzzerOnInvalid();
        }
        protected override IEBoard GetEBoard()
        {
            return new IChessOneChessBoard.EChessBoard(_basePath, _fileLogger, _configuration);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new IChessOneChessBoard.EChessBoard(_fileLogger);
        }
    }
}

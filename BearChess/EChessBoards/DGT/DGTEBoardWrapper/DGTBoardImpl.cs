using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.DGTEBoardWrapper
{
    public class DGTBoardImpl :AbstractEBoardWrapper
    {

        public DGTBoardImpl(string name, string basePath) : base(name, basePath)
        {
        }

     
       

        public DGTBoardImpl(string name, string basePath, EChessBoardConfiguration configuration) : base(
            name, basePath, configuration)
        {

        }



        public override bool Calibrate()
        {
            _board.Calibrate();
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

        public override void DimLEDs(bool dimLeds)
        {
           // throw new NotImplementedException();
        }

        public override void DimLEDs(int level)
        {
           //  throw new NotImplementedException();
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
            // throw new NotImplementedException();
        }

     

        protected override IEBoard GetEBoard()
        {
            return new DGTChessBoard.EChessBoard(_basePath, _fileLogger, _configuration);
          
        }

        protected override IEBoard GetEBoard(bool check)
        {

            return new DGTChessBoard.EChessBoard(logger: _fileLogger);
        }
    }
}

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
            return new DGTChessBoard.EChessBoard(basePath: _basePath, logger: _fileLogger,
                                                  portName: _comPortName,
                                                 _useBluetooth, _useClock, _showMovesOnly, _switchClockSide);
        }

        protected override IEBoard GetEBoard(bool check)
        {

            return new DGTChessBoard.EChessBoard(logger: _fileLogger);
        }
    }
}

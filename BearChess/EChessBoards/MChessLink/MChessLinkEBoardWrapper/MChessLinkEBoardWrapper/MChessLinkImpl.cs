using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkEBoardWrapper
{
    public class MChessLinkImpl : AbstractEBoardWrapper
    {
       

        public MChessLinkImpl(string name, string basePath) : base(name, basePath)
        {
        }

        /// <inheritdoc />
       
        public MChessLinkImpl(string name, string basePath, EChessBoardConfiguration configuration) :
            base(name, basePath, configuration)
        {

        }

        public override void SetScanTime(int scanTime)
        {
            _board?.SetScanTime(scanTime);
        }

        public override void SetDebounce(int debounce)
        {
            _board?.SetDebounce(debounce);
        }

        /// <inheritdoc />
        public override void FlashMode(EnumFlashMode flashMode)
        {
            _board?.FlashMode(flashMode);
        }

      

        /// <inheritdoc />
        protected override IEBoard GetEBoard()
        {
           // return new MChessLinkChessBoard.EChessBoard(_fileLogger, _comPortName, UseChesstimation);
            return new MChessLinkChessBoard.EChessBoard( _fileLogger, _configuration);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new MChessLinkChessBoard.EChessBoard(_fileLogger);
        }


        /// <inheritdoc />
        public override bool Calibrate()
        {
            return true;
        }

        public override void SendInformation(string message)
        {
            //
        }

        public override string RequestInformation(string message)
        {
            return string.Empty;
        }


        /// <inheritdoc />
        public override void DimLEDs(bool dimLeds)
        {
            _board?.DimLeds(dimLeds);
        }

        public override void DimLEDs(int level)
        {
            _board?.DimLeds(level);
        }
    }
}

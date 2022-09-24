using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkEBoardWrapper
{
    public class MChessLinkImpl : AbstractEBoardWrapper
    {
        public MChessLinkImpl(string name, string basePath) : base(name, basePath)
        {
        }

        /// <inheritdoc />
        public MChessLinkImpl(string name, string basePath, string comPortName) : base(
            name, basePath,  comPortName)
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
        public override void FlashInSync(bool flashSync)
        {
            _board?.FlashSync(flashSync);
        }


        /// <inheritdoc />
        protected override IEBoard GetEBoard()
        {
            return new MChessLinkChessBoard.EChessBoard(_fileLogger,  _comPortName);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new MChessLinkChessBoard.EChessBoard(_fileLogger);
        }


        /// <inheritdoc />
        public override void Calibrate()
        {
            // ignore
        }

        public override void SendInformation(string message)
        {
            //
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

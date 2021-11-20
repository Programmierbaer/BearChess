using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkEBoardWrapper
{
    public class MChessLinkImpl : AbstractEBoardWrapper
    {
        public MChessLinkImpl(string name, string basePath) : base(name, basePath)
        {
        }

        /// <inheritdoc />
        public MChessLinkImpl(string name, string basePath, bool isFirstInstance, string comPortName) : base(
            name, basePath, isFirstInstance, comPortName)
        {
        }


        /// <inheritdoc />
        public override void FlashInSync(bool flashSync)
        {
            _board?.FlashSync(flashSync);
        }


        /// <inheritdoc />
        protected override IEBoard GetEBoard()
        {
            return new MChessLinkChessBoard.EChessBoard(logger: _fileLogger, isFirstInstance: _isFirstInstance,
                                                        portName: _comPortName);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            return new MChessLinkChessBoard.EChessBoard(logger: _fileLogger);
        }


        /// <inheritdoc />
        public override void Calibrate()
        {
            // ignore
        }

        /// <inheritdoc />
        public override void DimLeds(bool dimLeds)
        {
            _board?.DimLeds(dimLeds);
        }

        public override void DimLeds(int level)
        {
            _board?.DimLeds(level);
        }
    }
}

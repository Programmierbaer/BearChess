using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkEBoardWrapper;


namespace www.SoLaNoSoft.com.BearChess.MChessLinkLoader
{
    public class MChessLinkLoader : AbstractLoader
    {

        public static string EBoardName = "MChessLink";

        public MChessLinkLoader() : base(EBoardName)
        {
        }

        public MChessLinkLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }

        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            var mChessLinkImpl = new MChessLinkImpl(name: Name, basePath: basePath, isFirstInstance: true, comPortName: configuration.PortName);
            mChessLinkImpl.FlashInSync(configuration.FlashInSync);
            mChessLinkImpl.DimLeds(configuration.DimLeds);
            return mChessLinkImpl;
        }
    }
}
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkEBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;


namespace www.SoLaNoSoft.com.BearChess.MChessLinkLoader
{
    public class MChessLinkLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.MChessLink;

        public MChessLinkLoader(bool check, string name) : base(check, name)
        {

        }

        public MChessLinkLoader() : base(EBoardName)
        {
        }

        public MChessLinkLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }

        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new MChessLinkImpl(name: Name, basePath: basePath);
            }
            var mChessLinkImpl = new MChessLinkImpl(name: Name, basePath: basePath,  comPortName: configuration.PortName);
            mChessLinkImpl.FlashInSync(configuration.FlashInSync);
            mChessLinkImpl.DimLEDs(configuration.DimLevel);
            mChessLinkImpl.SetScanTime(configuration.ScanTime);
            mChessLinkImpl.SetDebounce(configuration.Debounce);
            return mChessLinkImpl;
        }

   
    }
}
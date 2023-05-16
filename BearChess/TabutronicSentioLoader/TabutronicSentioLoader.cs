using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.EBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.Loader
{
    public class TabutronicSentioLoader : AbstractLoader
    {

        public static string EBoardName = Constants.TabutronicSentio;

        public TabutronicSentioLoader(bool check, string name) : base(check, name)
        {

        }

        public TabutronicSentioLoader() : base(EBoardName)
        {
        }

        public TabutronicSentioLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new SentioImpl(Name, basePath);
            }

            var eBoardWrapper = new SentioImpl(Name, basePath, configuration.PortName, configuration.UseBluetooth);
            //   eBoardWrapper.FlashInSync(true);
            return eBoardWrapper;
        }

      
    }
}

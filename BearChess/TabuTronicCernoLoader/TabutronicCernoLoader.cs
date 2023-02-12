using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.EBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.Loader
{
    public class TabutronicCernoLoader : AbstractLoader
    {

        public static string EBoardName = Constants.TabutronicCerno;

        public TabutronicCernoLoader(bool check, string name) : base(check, name)
        {

        }

        public TabutronicCernoLoader() : base(EBoardName)
        {
        }

        public TabutronicCernoLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new CernoImpl(Name, basePath);
            }

            var eBoardWrapper = new CernoImpl(Name, basePath, configuration.PortName, configuration.UseBluetooth);
            //   eBoardWrapper.FlashInSync(true);
            return eBoardWrapper;
        }
    }
}
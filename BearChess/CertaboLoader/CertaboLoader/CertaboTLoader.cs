using www.SoLaNoSoft.com.BearChess.CertaboEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.CertaboLoader
{
    public class CertaboTLoader : AbstractLoader
    {

        public static string EBoardName = Constants.Tabutronic;

        public CertaboTLoader(bool check, string name) : base(check, name)
        {

        }

        public CertaboTLoader() : base(EBoardName)
        {
        }

        public CertaboTLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new TabutronicImpl(Name, basePath);
            }

            var eBoardWrapper = new TabutronicImpl(Name, basePath, configuration.PortName, configuration.UseBluetooth);
            //   eBoardWrapper.FlashInSync(true);
            return eBoardWrapper;
        }
    }
}
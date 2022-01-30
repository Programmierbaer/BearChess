using www.SoLaNoSoft.com.BearChess.CertaboEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.CertaboLoader
{
    public class CertaboLoader : AbstractLoader
    {
     
        public static string EBoardName = Constants.Certabo;

        public CertaboLoader(bool check, string name) : base(check, name)
        {
         
        }

        public CertaboLoader() : base(EBoardName)
        {
        }

        public CertaboLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new CertaboImpl(Name, basePath);
            }

            var eBoardWrapper = new CertaboImpl(Name, basePath, true, configuration.PortName, configuration.UseBluetooth);
         //   eBoardWrapper.FlashInSync(true);
            return eBoardWrapper ;
        }
    }
}
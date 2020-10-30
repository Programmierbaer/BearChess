using System.Net.Http.Headers;
using www.SoLaNoSoft.com.BearChess.CertaboChessBoard;
using www.SoLaNoSoft.com.BearChess.CertaboEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.CertaboLoader
{
    public class CertaboLoader : AbstractLoader
    {
     
        public static string EBoardName = "Certabo";

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
                return new CertaboImpl(name: Name, basePath: basePath);
            }
            return new CertaboImpl(name: Name, basePath: basePath, isFirstInstance: true, comPortName: configuration.PortName);
        }
    }
}
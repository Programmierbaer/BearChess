using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.OSAEBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.OSALoader
{
    public class OSALoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.OSA;

        public OSALoader(bool check, string name) : base(check, name)
        {

        }

        public OSALoader() : base(EBoardName)
        {
        }

        public OSALoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new OSAImpl(Name, basePath);
            }

            var eBoardWrapper = new OSAImpl(Name, basePath, configuration.PortName, configuration.Baud, false);
            return eBoardWrapper;
        }

    }
}

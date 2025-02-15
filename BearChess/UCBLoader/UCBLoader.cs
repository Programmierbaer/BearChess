using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.UCBEBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.UCBLoader
{
    public class UCBLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.UCB;

        public UCBLoader(bool check, string name) : base(check, name)
        {

        }

        public UCBLoader() : base(EBoardName)
        {
        }

        public UCBLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new UCBImpl(Name, basePath);
            }

            var eBoardWrapper = new UCBImpl(Name, basePath, configuration);
            return eBoardWrapper;
        }

    }
}
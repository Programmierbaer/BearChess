using www.SoLaNoSoft.com.BearChess.DGTEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.DGTLoader
{
    public class DGTLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.DGT;

        public DGTLoader(bool check, string name) : base(check, name)
        {
        }

        public DGTLoader(string folderPath, string name) : base(folderPath, name)
        {
        }

        public DGTLoader(string folderPath)  : base(folderPath, EBoardName)
        {
        }

    protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new DGTBoardImpl(Name, basePath);
            }

            var eBoardWrapper = new DGTBoardImpl(Name, basePath, configuration);
            return eBoardWrapper;
        }
    }
}

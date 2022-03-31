using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.SquareOffEBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.SquareOffLoader
{
    public class SquareOffLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.SquareOff;

        public SquareOffLoader(bool check, string name) : base(check, name)
        {

        }

        public SquareOffLoader() : base(EBoardName)
        {
        }

        public SquareOffLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new SquareOffImpl(Name, basePath);
            }

            var eBoardWrapper = new SquareOffImpl(Name, basePath, true, configuration.PortName, true);

            return eBoardWrapper;
        }

    }
}
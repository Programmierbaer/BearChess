using www.SoLaNoSoft.com.BearChess.CitrineEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.CitrineLoader
{
    public class CitrineLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.Citrine;

        public CitrineLoader(bool check, string name) : base(check, name)
        {

        }

        public CitrineLoader() : base(EBoardName)
        {
        }

        public CitrineLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new CitrineImpl(Name, basePath);
            }

            var eBoardWrapper = new CitrineImpl(Name, basePath, configuration.PortName, false);

            return eBoardWrapper;
        }

    }
}

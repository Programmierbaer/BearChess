
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.PegasusEBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.PegasusLoader
{
    public class PegasusLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.Pegasus;

        public PegasusLoader(bool check, string name) : base(check, name)
        {

        }

        public PegasusLoader() : base(EBoardName)
        {
        }

        public PegasusLoader(string folderPath) : base(folderPath, EBoardName)
        {
            
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new PegasusImpl(Name, basePath);
            }

            configuration.PortName = "BTLE";
            var eBoardWrapper = new PegasusImpl(Name, basePath, configuration);

            eBoardWrapper.DimLEDs(configuration.DimLevel);
            eBoardWrapper.SetDebounce(configuration.Debounce);
            return eBoardWrapper;
        }
    }
}

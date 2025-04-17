using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.EBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.Loader
{
    public class TabutronicCernoSpectrumLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.TabutronicCernoSpectrum;

        public TabutronicCernoSpectrumLoader(bool check, string name) : base(check, name)
        {

        }

        public TabutronicCernoSpectrumLoader() : base(EBoardName)
        {
        }

        public TabutronicCernoSpectrumLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }


        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new CernoSpectrumImpl(Name, basePath);
            }

            var eBoardWrapper = new CernoSpectrumImpl(Name, basePath, configuration);
            return eBoardWrapper;
        }
    }
}
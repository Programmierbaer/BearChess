using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.TabuTronic.Tactum.EBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.TabuTronic.Tactum.Loader
{
    
    public class TabuTronicTactumLoader : AbstractLoader
    {
        public static string EBoardName = Constants.TabutronicTactum;
        

        public TabuTronicTactumLoader(bool check, string name) : base(check, name)
        {

        }

        public TabuTronicTactumLoader() : base(EBoardName)
        {
        }

        public TabuTronicTactumLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }

      
        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new TactumImpl(Name, basePath);
            }
            return new TactumImpl(Name, basePath, configuration);
        }
    }
}

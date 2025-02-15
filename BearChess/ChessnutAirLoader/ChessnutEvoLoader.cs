using System.IO;
using www.SoLaNoSoft.com.BearChess.ChessnutEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.ChessnutAirLoader
{
    public class ChessnutEvoLoader : AbstractLoader
    {
        public static readonly string EBoardName = Constants.ChessnutEvo;

        public ChessnutEvoLoader() :base (EBoardName)
        {
            
        }

        public ChessnutEvoLoader(bool check, string name) : base(check, name)
        {
        }

        public ChessnutEvoLoader(string folderPath, string name) : base(folderPath, name)
        {
        }

        public ChessnutEvoLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }
        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new ChessnutEvoImpl(Name, basePath);
            }

            var eBoardWrapper = new ChessnutEvoImpl(Name, basePath, configuration);
            return eBoardWrapper;
        }

        public static EChessBoardConfiguration Load(string basePath)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutEvo,
                $"{Constants.ChessnutEvo}Cfg.xml");
            return EChessBoardConfiguration.Load(fileName);

        }

        public static void Save(string basePath, bool useBluetooth, bool showMoveLine, bool showOwnMove)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutEvo,
                $"{Constants.ChessnutEvo}Cfg.xml");
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            eChessBoardConfiguration.UseBluetooth = useBluetooth;
            eChessBoardConfiguration.ShowMoveLine = showMoveLine;
            eChessBoardConfiguration.ShowOwnMoves = showOwnMove;
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }

        public static void Save(string basePath, EChessBoardConfiguration eChessBoardConfiguration)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutEvo,
                $"{Constants.ChessnutEvo}cfg.xml");
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }

    }
}
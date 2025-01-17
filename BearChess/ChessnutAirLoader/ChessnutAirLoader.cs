using System.IO;
using www.SoLaNoSoft.com.BearChess.ChessnutEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.ChessnutAirLoader
{
    public class ChessnutAirLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.ChessnutAir;

        public ChessnutAirLoader(bool check, string name) : base(check, name)
        {
        }

        public ChessnutAirLoader(string folderPath, string name) : base(folderPath, name)
        {
        }

        public ChessnutAirLoader(string folderPath) : base(folderPath,EBoardName)
        {
        }

        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new ChessnutAirImpl(Name, basePath);
            }

            var eBoardWrapper = new ChessnutAirImpl(Name, basePath, configuration);
            return eBoardWrapper;
        }

        public static EChessBoardConfiguration Load(string basePath)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutAir,
                $"{Constants.ChessnutAir}Cfg.xml");
            return EChessBoardConfiguration.Load(fileName);
            
        }

        public static void Save(string basePath, bool useBluetooth, bool showMoveLine, bool showOwnMove)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutAir,
                $"{Constants.ChessnutAir}Cfg.xml");
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            eChessBoardConfiguration.UseBluetooth = useBluetooth;
            eChessBoardConfiguration.ShowMoveLine = showMoveLine;
            eChessBoardConfiguration.ShowOwnMoves = showOwnMove;
            eChessBoardConfiguration.PortName = useBluetooth ? "BTLE" : "HID";
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }

        public static void Save(string basePath, EChessBoardConfiguration eChessBoardConfiguration)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutAir,
                $"{Constants.ChessnutAir}Cfg.xml");
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }
    }
}

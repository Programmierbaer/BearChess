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

            var eBoardWrapper = new ChessnutAirImpl(Name, basePath, configuration.PortName, configuration.UseBluetooth);
            return eBoardWrapper;
        }
        public static void Save(string basePath, bool useBluetooth)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutAir,
                $"{Constants.ChessnutAir}Cfg.xml");
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            eChessBoardConfiguration.UseBluetooth = useBluetooth;
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }
    }
}

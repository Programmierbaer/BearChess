using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.ChessnutEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.ChessnutAirLoader
{
    public class ChessnutGoLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.ChessnutGo;

        public ChessnutGoLoader(bool check, string name) : base(check, name)
        {
        }

        public ChessnutGoLoader(string folderPath, string name) : base(folderPath, name)
        {
        }

        public ChessnutGoLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }

        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new ChessnutGoImpl(Name, basePath);
            }

            var eBoardWrapper = new ChessnutGoImpl(Name, basePath,configuration);
            return eBoardWrapper;
        }

        public static EChessBoardConfiguration Load(string basePath)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutGo,
                $"{Constants.ChessnutGo}Cfg.xml");
            return EChessBoardConfiguration.Load(fileName);

        }

        public static void Save(string basePath, bool useBluetooth, bool showMoveLine, bool showOwnMove)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutGo,
                $"{Constants.ChessnutGo}Cfg.xml");
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            eChessBoardConfiguration.UseBluetooth = useBluetooth;
            eChessBoardConfiguration.ShowMoveLine = showMoveLine;
            eChessBoardConfiguration.ShowOwnMoves = showOwnMove;
            eChessBoardConfiguration.PortName = useBluetooth ? "BTLE" : "HDI";
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }

        public static void Save(string basePath, EChessBoardConfiguration eChessBoardConfiguration)
        {
            string fileName = Path.Combine(basePath, Constants.ChessnutGo,
                $"{Constants.ChessnutGo}Cfg.xml");
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }
    }
}

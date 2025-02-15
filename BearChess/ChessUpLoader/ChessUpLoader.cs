using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using www.SoLaNoSoft.com.BearChess.ChessUpEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.ChessUpLoader
{
    public class ChessUpLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.ChessUp;

        public ChessUpLoader(bool check, string name) : base(check, name)
        {
        }

        public ChessUpLoader(string folderPath, string name) : base(folderPath, name)
        {
        }

        public ChessUpLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }

        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new ChessUpImpl(Name, basePath);
            }

            var eBoardWrapper = new ChessUpImpl(Name, basePath, configuration);
            return eBoardWrapper;
        }

        public static void Save(string basePath, bool useBluetooth, bool showMoveLine, bool showOwnMove)
        {
            string fileName = Path.Combine(basePath, Constants.ChessUp,
                $"{Constants.ChessUp}Cfg.xml");
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            eChessBoardConfiguration.UseBluetooth = useBluetooth;
            eChessBoardConfiguration.ShowMoveLine = showMoveLine;
            eChessBoardConfiguration.ShowOwnMoves = showOwnMove;
            eChessBoardConfiguration.PortName = "BT";
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }

    }
}

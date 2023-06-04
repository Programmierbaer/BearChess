using System.IO;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.IChessOneEBoardWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.IChessOneLoader 
{
    public class IChessOneLoader : AbstractLoader
    {
        private const string EBoardName = Constants.IChessOne;

        public IChessOneLoader(bool check, string name) : base(check, name)
        {
        }

        public IChessOneLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }

        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new IChessOneImpl(Name, basePath);
            }

            var eBoardWrapper = new IChessOneImpl(Name, basePath, configuration.PortName, configuration.UseBluetooth);
            return eBoardWrapper;
        }

        public static void Save(string basePath, bool useBluetooth)
        {
            string fileName = Path.Combine(basePath, Constants.IChessOne,
                                           $"{Constants.IChessOne}Cfg.xml");
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            eChessBoardConfiguration.UseBluetooth = useBluetooth;
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }
    }
}

using System;
using System.IO;
using www.SoLaNoSoft.com.BearChess.HoSEBoardWrapper;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;


namespace www.SoLaNoSoft.com.BearChess.HoSLoader
{
    public class HoSLoader : AbstractLoader
    {

        public static readonly string EBoardName = Constants.Zmartfun;

        public HoSLoader(bool check, string name) : base(check, name)
        {
        }

        public HoSLoader(string folderPath, string name) : base(folderPath, name)
        {
        }

        public HoSLoader(string folderPath) : base(folderPath, EBoardName)
        {
        }

        protected override IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration)
        {
            if (Check)
            {
                return new HoSImpl(Name, basePath);
            }

            var eBoardWrapper = new HoSImpl(Name, basePath, configuration);
            eBoardWrapper.SetDebounce(configuration.Debounce);
            return eBoardWrapper;
        }

        public static EChessBoardConfiguration Load(string basePath)
        {
            var fileName = Path.Combine(basePath, Constants.Zmartfun,
                $"{Constants.Zmartfun}Cfg.xml");
            return EChessBoardConfiguration.Load(fileName);

        }

        public static void Save(string basePath, bool useBluetooth, bool showMoveLine, bool showOwnMove)
        {
            var fileName = Path.Combine(basePath, Constants.Zmartfun,
                $"{Constants.Zmartfun}Cfg.xml");
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            eChessBoardConfiguration.UseBluetooth = useBluetooth;
            eChessBoardConfiguration.ShowMoveLine = showMoveLine;
            eChessBoardConfiguration.ShowOwnMoves = showOwnMove;
            eChessBoardConfiguration.PortName = useBluetooth ? "BTLE" : "HID";
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }

        public static void Save(string basePath, EChessBoardConfiguration eChessBoardConfiguration)
        {
            var fileName = Path.Combine(basePath, Constants.Zmartfun,
                $"{Constants.Zmartfun}Cfg.xml");
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }

        public static void Save(string basePath)
        {
            var fileName = Path.Combine(basePath, Constants.Zmartfun,
                $"{Constants.Zmartfun}Cfg.xml");
            var exists = File.Exists(fileName);
            var eChessBoardConfiguration = EChessBoardConfiguration.Load(fileName);
            if (!exists)
            {
                eChessBoardConfiguration.ShowOwnMoves = false;
                eChessBoardConfiguration.ExtendedConfig[0].ShowOwnMoves = false;
            }
            EChessBoardConfiguration.Save(eChessBoardConfiguration, fileName);
        }
    }
}

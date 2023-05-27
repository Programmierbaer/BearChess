using System.IO;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public class EChessBoardConfiguration
    {
        public string PortName { get; set; }
        public string Baud { get; set; }
        public bool DimLeds { get; set; }
        public int DimLevel { get; set; }
        public bool FlashInSync { get; set; }
        public bool NoFlash { get; set; }
        public bool UseBluetooth { get; set; }
        public bool UseClock { get; set; }
        public bool ClockShowOnlyMoves { get; set; }
        public bool ClockSwitchSide { get; set; }
        public bool LongMoveFormat { get; set; }
        public int ScanTime { get; set; }
        public int Debounce { get; set; }
        public bool UseChesstimation { get; set; }

        public EChessBoardConfiguration()
        {
            DimLevel = -1;
            UseClock = true;
            ClockShowOnlyMoves = false;
            ClockSwitchSide = false;
            UseBluetooth = false;
            LongMoveFormat = true;
            ScanTime = 30; // Default for ChessLink
            Debounce = 0; // Default for ChessLink
            Baud = "1200";
            UseChesstimation = false;
        }

        public static EChessBoardConfiguration Load(string fileName)
        {
            var configuration = new EChessBoardConfiguration();
            try
            {
                if (File.Exists(fileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EChessBoardConfiguration));
                    TextReader textReader = new StreamReader(fileName);
                    var savedConfig = (EChessBoardConfiguration)serializer.Deserialize(textReader);
                    textReader.Close();
                    configuration.PortName = savedConfig.PortName;
                    configuration.Baud = savedConfig.Baud;
                    configuration.DimLeds = savedConfig.DimLeds;
                    configuration.DimLevel = savedConfig.DimLevel;
                    configuration.FlashInSync = savedConfig.FlashInSync;
                    configuration.NoFlash = savedConfig.NoFlash;
                    configuration.UseBluetooth = savedConfig.UseBluetooth;
                    configuration.ClockShowOnlyMoves = savedConfig.ClockShowOnlyMoves;
                    configuration.UseClock = savedConfig.UseClock;
                    configuration.ClockSwitchSide = savedConfig.ClockSwitchSide;
                    configuration.LongMoveFormat = savedConfig.LongMoveFormat;
                    configuration.ScanTime = savedConfig.ScanTime;
                    configuration.Debounce = savedConfig.Debounce;
                    configuration.UseChesstimation = savedConfig.UseChesstimation;
                    if (configuration.DimLevel < 0)
                    {
                        configuration.DimLevel = configuration.DimLeds ? 0 : 14;
                    }
                }
                else
                {
                    configuration.PortName = "<auto>";
                }
            }
            catch
            {
                configuration.PortName = "<auto>";
            }

            return configuration;
        }

        public static void Save(EChessBoardConfiguration configuration, string fileName)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EChessBoardConfiguration));
                TextWriter textWriter = new StreamWriter(fileName, false);
                serializer.Serialize(textWriter, configuration);
                textWriter.Close();
            }
            catch 
            {
                //   _fileLogger?.LogError("Error on save configuration", ex);
            }
        }

    }

}

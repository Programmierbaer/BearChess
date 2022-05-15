using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public class EChessBoardConfiguration
    {
        public string PortName { get; set; }
        public bool DimLeds { get; set; }
        public int DimLevel { get; set; }
        public bool FlashInSync { get; set; }
        public bool UseBluetooth { get; set; }
        public bool UseClock { get; set; }
        public bool ClockShowOnlyMoves { get; set; }
        public bool ClockSwitchSide { get; set; }

        public EChessBoardConfiguration()
        {
            DimLevel = -1;
            UseClock = true;
            ClockShowOnlyMoves = false;
            ClockSwitchSide = false;
        }

        public static EChessBoardConfiguration Load(string fileName)
        {
            EChessBoardConfiguration configuration = new EChessBoardConfiguration();
            try
            {
                if (File.Exists(fileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EChessBoardConfiguration));
                    TextReader textReader = new StreamReader(fileName);
                    EChessBoardConfiguration savedConfig = (EChessBoardConfiguration)serializer.Deserialize(textReader);
                    textReader.Close();
                    configuration.PortName = savedConfig.PortName;
                    configuration.DimLeds = savedConfig.DimLeds;
                    configuration.DimLevel = savedConfig.DimLevel;
                    configuration.FlashInSync = savedConfig.FlashInSync;
                    configuration.UseBluetooth = savedConfig.UseBluetooth;
                    configuration.ClockShowOnlyMoves = savedConfig.ClockShowOnlyMoves;
                    configuration.UseClock = savedConfig.UseClock;
                    configuration.ClockSwitchSide = savedConfig.ClockSwitchSide;
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

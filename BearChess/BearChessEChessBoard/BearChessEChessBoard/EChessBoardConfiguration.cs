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
        public bool FlashInSync { get; set; }

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
                    configuration.FlashInSync = savedConfig.FlashInSync;
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

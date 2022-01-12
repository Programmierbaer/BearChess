using System;
using System.Linq;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChessBase
{
    [Serializable]
    public class UciInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OriginName { get; set; }
        public string Author { get; set; }
        [XmlArray("Options")]
        public string[] Options { get; set; }
        public string FileName { get; set; }
        public string LogoFileName { get; set; }
        public bool Valid { get; set; }
        public string OpeningBook { get; set; }
        public string OpeningBookVariation { get; set; }
        [XmlArray("OptionValues")]
        public string[] OptionValues { get; set; }
        public bool AdjustStrength { get; set; }
        public string CommandParameter { get; set; }

        public bool WaitForStart { get; set; }
        public int WaitSeconds { get; set; }

        public UciInfo()
        {
            Options = Array.Empty<string>();
            OptionValues = Array.Empty<string>();
            OpeningBookVariation = "1";
            AdjustStrength = false;
            LogoFileName = string.Empty;
            WaitForStart = false;
            WaitSeconds = 0;
        }

        public UciInfo(string fileName) : this()
        {
            FileName = fileName;
        }

        public void AddOption(string option)
        {
            Options = Options.Append(option).ToArray();
        }

        public void ClearOptionValues()
        {
            OptionValues = Array.Empty<string>();
        }

        public void AddOptionValue(string optionValue)
        {
            OptionValues = OptionValues.Append(optionValue).ToArray();
        }

        public void SetPonderValue(string trueFalse)
        {
            OptionValues = OptionValues.Where(o => !o.Contains("Ponder")).Append($"setoption name Ponder value {trueFalse}").ToArray();
        }

        public override string ToString()
        {
            return $"{OriginName} from {Author}";
        }
    }
}

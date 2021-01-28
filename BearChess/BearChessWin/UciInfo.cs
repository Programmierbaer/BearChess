using System;
using System.Linq;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChessWin
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
        public bool Valid { get; set; }
        public string OpeningBook { get; set; }
        public string OpeningBookVariation { get; set; }
        [XmlArray("OptionValues")]
        public string[] OptionValues { get; set; }

        public UciInfo()
        {
            Options = new string[0];
            OptionValues = new string[0];
            OpeningBookVariation = "1";
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
            OptionValues = new string[0];
        }

        public void AddOptionValue(string optionValue)
        {
            OptionValues = OptionValues.Append(optionValue).ToArray();
        }

        public void SetPonderValue(string trueFalse)
        {
           OptionValues =  OptionValues.Where(o => !o.Contains("Ponder")).Append($"setoption name Ponder value {trueFalse}").ToArray();
        }

        public override string ToString()
        {
            return $"{OriginName} from {Author}";
        }
    }
}
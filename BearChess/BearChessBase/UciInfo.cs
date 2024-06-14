using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase
{

    

    [Serializable]
    public class UciInfo
    {
        private string _fileName;

        private int _playerElo;

        public string Id { get; set; }
        public string Name { get; set; }
        public string OriginName { get; set; }
        public string Author { get; set; }
        
        [XmlArray("Options")]
        public string[] Options { get; set; }

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                CheckIsValidForAnalysis();
                try
                {
                    if (IsMessChessEngine)
                    {
                        MessChessLevels  = Array.Empty<string>();
                        MessChessLevelInfo = string.Empty;
                        MessChessLevelsAreIncomplete = false;
                        MessChessLevelsAreManual = false;
                    }
                }
                catch
                {
                    //
                }
            }
        }

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
        public bool ValidForAnalysis { get; set; }
        public DateTime ChangeDateTime { get; set; }
        public bool IsPlayer { get; set; }
        public bool IsChessServer { get; set; }
        public bool IsChessComputer { get; set; }
        public bool IsActive { get; set; }
        public bool IsBuddy { get; set; }
        public bool IsProbing { get; set; }
        public bool IsInternalBearChessEngine { get; set; }
        public bool IsInternalChessEngine { get; set; }
        public bool IsMessChessEngine { get; set; }
        public bool IsMessChessNewEngine { get; set; }

        [XmlIgnore]
        public string[] MessChessLevels { get; set; }

        [XmlIgnore] 
        public string MessChessLevelInfo { get; set; }

        [XmlIgnore] 
        public bool MessChessLevelsAreIncomplete { get; private set; }

        [XmlIgnore]
        public bool MessChessLevelsAreManual { get; private set; }


        public UciInfo()
        {
            Options = Array.Empty<string>();
            OptionValues = Array.Empty<string>();
            OpeningBookVariation = "1";
            AdjustStrength = false;
            LogoFileName = string.Empty;
            WaitForStart = false;
            WaitSeconds = 0;
            ValidForAnalysis = true;
            _fileName = string.Empty;
            ChangeDateTime = DateTime.MinValue;
            IsPlayer = false;
            IsChessServer = false;
            IsActive = true;
            _playerElo = 0;
            IsBuddy = false;
            IsProbing = false;
            IsInternalBearChessEngine = false;
            IsMessChessEngine = false;
            IsMessChessNewEngine = false;
            MessChessLevels = Array.Empty<string>();
            MessChessLevelInfo = string.Empty;
            MessChessLevelsAreIncomplete = false;
            MessChessLevelsAreManual = false;
        }

        public UciInfo(string fileName) : this()
        {
            FileName = fileName;
        }

        public void ReadMessChessLevels(string bearChessFileName)
        {
            if (IsMessChessEngine)
            {
                FileInfo fi = new FileInfo(FileName);
                if (fi.Exists)
                {
                    string messChessLevelFileName = Path.Combine(fi.DirectoryName, "Level", "English.txt");
                    string messChessBearChessLevelFileName = Path.Combine(fi.DirectoryName, "BearChess", "MessChessLevels.txt");
                    try
                    {
                        var strings = CommandParameter.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string levelCode = string.Empty;
                        if (strings.Length > 1 && !strings[1].StartsWith("-"))
                        {
                            levelCode = strings[1];
                        }
                        if (File.Exists(messChessBearChessLevelFileName)) {
                            bearChessFileName = messChessBearChessLevelFileName;
                        }
                        var messChessLevelReader =
                            new MessChessLevelReader(bearChessFileName, messChessLevelFileName, strings[0], levelCode);
                        MessChessLevels = messChessLevelReader.GetLevels;
                        MessChessLevelInfo = messChessLevelReader.GetMessChessLevels;
                        MessChessLevelsAreIncomplete = messChessLevelReader.LevelsAreIncomplete;
                        MessChessLevelsAreManual = messChessLevelReader.LevelsAreManual;
                    }
                    catch
                    {
                        //
                    }
                }
            }
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

        public bool CanConfigureElo()
        {

            var uciElo = OptionValues.FirstOrDefault(f => f.StartsWith("setoption name UCI_Elo"));
            return uciElo != null;
        }


        public int GetConfiguredElo()
        {
            if (IsPlayer)
            {
                return _playerElo;
            }
            var uciElo = OptionValues.FirstOrDefault(f => f.StartsWith("setoption name UCI_Elo") ||
                                                          f.StartsWith("setoption name UCI Elo"));
            if (uciElo != null)
            {
                var uciEloLimit = OptionValues.FirstOrDefault(f => f.StartsWith("setoption name UCI_LimitStrength") ||
                                                                   f.StartsWith("setoption name UCI LimitStrength"));
                if (uciEloLimit != null)
                {
                    if (uciEloLimit.Contains("true"))
                    {
                        var strings = uciElo.Split(" ".ToCharArray());
                        if (int.TryParse(strings[strings.Length - 1], out int elo))
                        {
                            return elo;
                        }
                    }
                }
            }

            return -1;
        }

        public int GetMinimumElo()
        {
            int minValue = -1;
            if (CanConfigureElo())
            {
                var uciElo = Options.FirstOrDefault(f => f.StartsWith("option name UCI_Elo" )||
                                                         f.StartsWith("option name UCI Elo"));
                if (uciElo != null)
                {
                    var optionSplit = uciElo.Split(" ".ToCharArray());
                    for (int i = 0; i < optionSplit.Length; i++)
                    {
                        if (optionSplit[i].Equals("min"))
                        {
                            i++;
                            int.TryParse(optionSplit[i], out minValue);
                            break;
                        }
                    }
                }
            }
            return minValue;
        }

        public int GetMaximumElo()
        {
            int maxValue = -1;
            if (CanConfigureElo())
            {
                var uciElo = Options.FirstOrDefault(f => f.StartsWith("option name UCI_Elo") ||
                                                         f.StartsWith("option name UCI Elo"));
                if (uciElo != null)
                {
                    var optionSplit = uciElo.Split(" ".ToCharArray());
                    for (int i = 0; i < optionSplit.Length; i++)
                    {
                        if (optionSplit[i].Equals("max"))
                        {
                            i++;
                            int.TryParse(optionSplit[i], out maxValue);
                            break;
                        }
                    }
                }
            }
            return maxValue;
        }

        public void SetElo(int elo)
        {
            if (IsPlayer || IsChessServer)
            {
                _playerElo = elo;
                return;
            }
            List<string> newOptionValues = new List<string>();
            for (int i = 0; i < OptionValues.Length; i++)
            {
                var optionValue = OptionValues[i];
                if (optionValue.StartsWith("setoption name UCI_Elo"))
                {
                    optionValue = $"setoption name UCI_Elo value {elo}";
                }
                if (optionValue.StartsWith("setoption name UCI Elo"))
                {
                    optionValue = $"setoption name UCI Elo value {elo}";
                }

                if (optionValue.StartsWith("setoption name UCI_LimitStrength"))
                {
                    optionValue = "setoption name UCI_LimitStrength value true";
                }
                if (optionValue.StartsWith("setoption name UCI LimitStrength"))
                {
                    optionValue = "setoption name UCI LimitStrength value true";
                }
                newOptionValues.Add(optionValue);
            }
            OptionValues = newOptionValues.ToArray();
        }


        private void CheckIsValidForAnalysis()
        {
            ValidForAnalysis = true;
            IsMessChessEngine = false;

            if (FileName.EndsWith("MessChess.exe", StringComparison.OrdinalIgnoreCase))
            {
                ValidForAnalysis = false;
                IsMessChessEngine = true;
                return;
            }
            if (FileName.EndsWith("MessNew.exe", StringComparison.OrdinalIgnoreCase))
            {
                ValidForAnalysis = false;
                IsMessChessEngine = true;
                IsMessChessNewEngine = true;
                return;
            }

            if (FileName.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) || FileName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string allText = File.ReadAllText(FileName);
                    if (allText.IndexOf("MessChess", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ValidForAnalysis = false;
                        IsMessChessEngine = true;
                    }
                    if (allText.IndexOf("MessNew", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ValidForAnalysis = false;
                        IsMessChessEngine = true;
                        IsMessChessNewEngine = true;
                    }
                }
                catch
                {
                    //
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class MessChessLevelReader
    {

        private readonly List<string> _allLevels = new List<string>();

        private string _messChessLevel = string.Empty;

        public string[] GetLevels => _allLevels.ToArray();
        public string GetMessChessLevels => _messChessLevel;

        public bool LevelsAreIncomplete { get; private set; }
        public bool LevelsAreManual { get; private set; }

        public MessChessLevelReader(string bearChessFileName, string messChessFileName, string emulationCode)
        {
            if (File.Exists(bearChessFileName))
            {
                ReadBearChessChessLevel(bearChessFileName,emulationCode);
            }
            if (File.Exists(messChessFileName))
            {
                ReadMessChessLevel(messChessFileName, emulationCode);
            }
        }

        private void ReadMessChessLevel(string fileName, string emulationCode)
        {
            bool readingCode = false;
            bool readingLevel = false;
            var readAllLines = File.ReadAllLines(fileName,Encoding.Default);
            _messChessLevel = string.Empty;
            foreach (var line in readAllLines)
            {
                if (line.StartsWith("#") && line.Contains($"#{emulationCode}#"))
                {
                    readingCode = true;
                    continue;
                }

                if (readingCode && !line.StartsWith("#"))
                {
                    readingLevel = true;
                }

                if (readingLevel && line.StartsWith("#"))
                {
                    return;
                }

                if (!readingLevel)
                {
                    continue;
                }

                _messChessLevel += line + Environment.NewLine;
            }
        }

        private void ReadBearChessChessLevel(string fileName, string emulationCode)
        {
            bool readingCode = false;
            bool readingLevel = false;
            var readAllLines = File.ReadAllLines(fileName,Encoding.UTF8);
            _allLevels.Clear();
            foreach (var line in readAllLines)
            {
                if (line.StartsWith("#") && line.Contains($"#{emulationCode}#"))
                {
                    readingCode = true;
                    continue;
                }

                if (readingCode && line.StartsWith("Levels:"))
                {
                    readingLevel = true;
                    LevelsAreIncomplete = line.Contains("#");
                    LevelsAreManual = line.Contains("?");
                    if (LevelsAreManual)
                    {
                        return;

                    }
                    continue;
                }

                if (readingLevel && line.StartsWith("#"))
                {
                    return;
                }

                if (!readingLevel || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                
                var levels = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (levels.Length > 0)
                {
                    _allLevels.Add(levels[0].Trim().Replace("#"," "));
                }
                if (levels.Length > 1)
                {
                    string joined = string.Join(" ", levels, 1, levels.Length - 1);
                    _allLevels.Add(joined.RemoveSpaces());
                }
                else
                {
                    _allLevels.Add(" ");
                }
            }
        }

    }
}

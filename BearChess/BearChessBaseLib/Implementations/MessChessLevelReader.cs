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

        public MessChessLevelReader(string bearChessFileName, string messChessFileName, string emulationCode, string levelCode)
        {
            if (File.Exists(bearChessFileName))
            {
                ReadBearChessChessLevel(bearChessFileName,emulationCode, levelCode);
            }
            if (File.Exists(messChessFileName))
            {
                ReadMessChessLevel(messChessFileName, emulationCode, levelCode);
            }
        }

        private void ReadMessChessLevel(string fileName, string emulationCode, string levelCode)
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
                    if (string.IsNullOrWhiteSpace(levelCode))
                    {
                        readingLevel = true;
                        _messChessLevel += line + Environment.NewLine;
                        continue;
                    }

                    if (line.StartsWith("Levels:") && line.Contains(levelCode))
                    {
                        readingLevel = true;
                        _messChessLevel += line + Environment.NewLine;
                        continue;
                    }
                }

                if (readingLevel && line.StartsWith("#"))
                {
                    return;
                }

                if (readingLevel && line.StartsWith("Levels:"))
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

        private void ReadBearChessChessLevel(string fileName, string emulationCode, string levelCode)
        {
            bool readingCode = false;
            bool readingLevel = false;
            var readAllLines = File.ReadAllLines(fileName,Encoding.Default);
            _allLevels.Clear();
            foreach (var line in readAllLines)
            {
                if (line.StartsWith("#") && line.Contains($"#{emulationCode}#"))
                {
                    readingCode = true;
                    continue;
                }

                if (readingCode && !readingLevel && line.StartsWith("Levels:"))
                {
                    if (string.IsNullOrWhiteSpace(levelCode))
                    {
                        readingLevel = true;
                        LevelsAreIncomplete = line.Contains("#");
                        LevelsAreManual = line.Contains("?");
                        if (LevelsAreManual)
                        {
                            return;

                        }
                    }
                    else
                    {
                        if (line.StartsWith("Levels:") && line.Contains(levelCode))
                        {
                            readingLevel = true;
                            LevelsAreIncomplete = line.Contains("#");
                            LevelsAreManual = line.Contains("?");
                        }
                    }

                    continue;
                }

                if (readingLevel && line.StartsWith("#"))
                {
                    return;
                }
                if (readingLevel && line.StartsWith("Levels:"))
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

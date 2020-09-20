using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    [Serializable]
    public class EcoCode
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Moves { get; set; }
        public string FenCode { get; set; }

        public EcoCode()
        {
            
        }

        public EcoCode(string code, string name, string moves, string fenCode)
        {
            Code = code;
            Name = name;
            Moves = moves;
            FenCode = fenCode;
        }
    }

    public class EcoCodeReader
    {
        private string fileName { get; set; }

        public EcoCodeReader()
        {
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bearchess");
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch
                {
                    //
                }
            }
            fileName = Path.Combine(folderPath, "bearchess.eco");
        }

        public EcoCode[] Load()
        {
            if (File.Exists(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EcoCode[]));
                TextReader textReader = new StreamReader(fileName);
                EcoCode[] result = (EcoCode[])serializer.Deserialize(textReader);
                textReader.Close();
                return result;
            }
            return new EcoCode[0];
        }

        public void Save(EcoCode[] ecoCodes)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(EcoCode[]));
            TextWriter textWriter = new StreamWriter(fileName, false);
            serializer.Serialize(textWriter, ecoCodes);
            textWriter.Close();
        }


        public EcoCode[] LoadCsvFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new EcoCode[0];
            }
            List<EcoCode> result = new List<EcoCode>();
            var allLines = File.ReadAllLines(fileName);
            HashSet<string> allFenCodes = new HashSet<string>();
            foreach (var allLine in allLines)
            {
                var strings = allLine.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length > 2)
                {
                    var fenCode = GetFenCode(strings[strings.Length - 1]);
                    if (allFenCodes.Contains(fenCode))
                    {
                        continue;
                    }
                    allFenCodes.Add(fenCode);
                    string name = string.Empty;
                    for (int i=1; i<strings.Length-1; i++)
                    {
                        name += strings[i];
                    }
                    result.Add(new EcoCode(strings[0], name.Replace("\"",string.Empty).Replace(strings[0],string.Empty).Trim(), strings[strings.Length-1], fenCode));
                }
            }
            Save(result.ToArray());
            
            return result.ToArray();
        }

        private string GetFenCode(string moves)
        {

            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            var strings = moves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in strings)
            {
                if (s.Contains("."))
                {
                    chessBoard.MakeMove(s.Substring(s.IndexOf(".")+1));
                }
                else
                {
                    chessBoard.MakeMove(s);
                }
            }

            return chessBoard.GetFenPosition();
        }


        public EcoCode[] LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new EcoCode[0];
            }
            List<EcoCode> result = new List<EcoCode>();
            var allLines = File.ReadAllLines(fileName);
            string code = string.Empty;
            string openingName = string.Empty;
            string moves = string.Empty;
            HashSet<string> allFenCodes = new HashSet<string>();
            foreach (var line in allLines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                {
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        var fenCode = GetFenCode(moves);
                        if (!allFenCodes.Contains(fenCode))
                        {
                            allFenCodes.Add(fenCode);
                            result.Add(new EcoCode(code, openingName, moves, fenCode));
                        }
                    }

                    code = string.Empty;
                    openingName = string.Empty;
                    moves = string.Empty;
                    continue;
                }

                if (line.StartsWith("\""))
                {
                    openingName = line.Replace("\"", string.Empty);
                    code = openingName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    openingName = openingName.Replace(code, string.Empty).Trim();
                    continue;
                }

                moves += line + " ";
            }
            Save(result.ToArray());
            return result.ToArray();
        }

        public EcoCode[] LoadArenaFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new EcoCode[0];
            }
            List<EcoCode> result = new List<EcoCode>();
            var allLines = File.ReadAllLines(fileName);
            HashSet<string> allFenCodes = new HashSet<string>();
            foreach (var line in allLines)
            {
                if (!line.StartsWith("{") || !line.Contains("}"))
                {
                    continue;
                }

                var openingName = line.Substring(1, line.IndexOf("}") - 1);
                var code = openingName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                var moves = line.Substring(line.IndexOf("}")+1).Replace(". ",".");
                var fenCode = GetFenCode(moves);
                if (allFenCodes.Contains(fenCode))
                {
                    continue;
                }
                allFenCodes.Add(fenCode);
                result.Add(new EcoCode(code,openingName.Replace(code,string.Empty).Trim(),moves, fenCode));
            }
            Save(result.ToArray());
            return result.ToArray();
        }

    }
}

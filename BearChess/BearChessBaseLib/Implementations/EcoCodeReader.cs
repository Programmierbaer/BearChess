using System;
using System.Collections.Generic;
using System.IO;
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

        private string _fileName { get; }
        
        public EcoCodeReader(string[] pathNames)
        {
            _fileName = string.Empty;
            foreach (var pathName in pathNames)
            {
                var combine = Path.Combine(pathName, "bearchess.eco");
                if (File.Exists(combine))
                {
                    _fileName = combine;
                    return;
                }
            }
        }


        public EcoCode[] Load()
        {
            if (File.Exists(_fileName))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(EcoCode[]));
                    TextReader textReader = new StreamReader(_fileName);
                    var result = (EcoCode[])serializer.Deserialize(textReader);
                    textReader.Close();
                    return result;
                }
                catch
                {
                    //
                }
            }

            return Array.Empty<EcoCode>();
        }

        public void Save(EcoCode[] ecoCodes)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(EcoCode[]));
                TextWriter textWriter = new StreamWriter(_fileName, false);
                serializer.Serialize(textWriter, ecoCodes);
                textWriter.Close();
            }
            catch
            {
                //
            }
        }


        public EcoCode[] LoadCsvFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new EcoCode[0];
            }

            var result = new List<EcoCode>();
            try
            {
                var allLines = File.ReadAllLines(fileName);
                var allFenCodes = new HashSet<string>();
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
                        var name = string.Empty;
                        for (var i = 1; i < strings.Length - 1; i++) name += strings[i];
                        result.Add(new EcoCode(strings[0],
                            name.Replace("\"", string.Empty).Replace(strings[0], string.Empty).Trim(),
                            strings[strings.Length - 1], fenCode));
                    }
                }

                Save(result.ToArray());
            }
            catch
            {
                //
            }

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
                chessBoard.MakePgnMove(s.Contains(".") ? s.Substring(s.IndexOf(".") + 1) : s, string.Empty, string.Empty);
            }

            return chessBoard.GetFenPosition();
        }


        public EcoCode[] LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new EcoCode[0];
            }

            var result = new List<EcoCode>();
            var allLines = File.ReadAllLines(fileName);
            var code = string.Empty;
            var openingName = string.Empty;
            var moves = string.Empty;
            var allFenCodes = new HashSet<string>();
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

            var result = new List<EcoCode>();
            var allLines = File.ReadAllLines(fileName);
            var allFenCodes = new HashSet<string>();
            foreach (var line in allLines)
            {
                if (!line.StartsWith("{") || !line.Contains("}"))
                {
                    continue;
                }

                var openingName = line.Substring(1, line.IndexOf("}") - 1);
                var code = openingName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                var moves = line.Substring(line.IndexOf("}") + 1).Replace(". ", ".");
                var fenCode = GetFenCode(moves);
                if (allFenCodes.Contains(fenCode))
                {
                    continue;
                }

                allFenCodes.Add(fenCode);
                result.Add(new EcoCode(code, openingName.Replace(code, string.Empty).Trim(), moves, fenCode));
            }

            Save(result.ToArray());
            return result.ToArray();
        }
    }
}
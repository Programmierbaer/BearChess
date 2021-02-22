using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public class PgnLoader
    {
        private readonly Dictionary<Guid, PgnGame> pgnGames = new Dictionary<Guid, PgnGame>();

        public List<PgnGame> Games = new List<PgnGame>();

        public string Filename { get; private set; }
        //public PgnGame[] Games => pgnGames.Values.ToArray();

        public void Clear()
        {
            pgnGames.Clear();
            Games.Clear();
        }
        
        public IEnumerable<PgnGame> Load(string fileName)
        {
             Filename = fileName;
            PgnGame currentGame = null;
            int insideComment = 0;
            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string allLine;
                        while ((allLine = sr.ReadLine()) != null)
                        {
                            var line = allLine.Trim();
                            if (line.StartsWith("[Event ", StringComparison.OrdinalIgnoreCase))
                            {
                                if (currentGame != null)
                                {
                                    yield return currentGame;
                                }

                                currentGame = new PgnGame
                                {
                                    GameEvent = line
                                                              .Replace("[Event", string.Empty)
                                                              .Replace("]", string.Empty)
                                };
                                insideComment = 0;
                                continue;
                            }
                            if (insideComment == 0 && line.StartsWith("["))
                            {
                                line = line.Replace("[", string.Empty).Replace("]", string.Empty);
                                var indexOf = line.IndexOf(" ", StringComparison.Ordinal);
                                currentGame?.AddValue(line.Substring(0, indexOf), line.Substring(indexOf));
                                continue;
                            }

                            bool startsWithDollar = false;
                            string newLine = string.Empty;
                            for (int i = 0; i < line.Length; i++)
                            {
                                if (line[i] == '{' || line[i] == '(')
                                {
                                    insideComment++;
                                    continue;
                                }

                                if (line[i] == '}' || line[i] == ')')
                                {
                                    insideComment--;
                                    continue;
                                }

                                if (insideComment > 0)
                                {
                                    continue;
                                }

                                if (line[i] == '$')
                                {
                                    startsWithDollar = true;
                                    continue;
                                }

                                if (startsWithDollar && line[i] != ' ')
                                {
                                    continue;
                                }
                                startsWithDollar = false;
                                newLine += line[i];
                            }

                            if (newLine.Length == 0)
                            {
                                continue;
                            }
                            var strings = newLine.Split(" ".ToCharArray());
                            foreach (var s in strings)
                            {
                                if (currentGame != null && currentGame.Result.Contains(s))
                                {
                                    continue;
                                }

                                if (s.Contains("."))
                                {
                                    if (s.EndsWith("."))
                                    {
                                        continue;
                                    }

                                    currentGame?.AddMove(s.Substring(s.IndexOf(".") + 1));
                                    continue;
                                }


                                currentGame?.AddMove(s);
                            }
                        }
                    }
                }
            }
       

            if (currentGame != null)
            {
                yield return currentGame;
            }
        }

        public void AddGame(string pgnGame)
        {
            LoadFromArray(pgnGame.Replace(Environment.NewLine, string.Empty).Replace("]", $"]{Environment.NewLine}")
                                 .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                 .ToArray());
        }

        public void AddGame(PgnGame pgnGame)
        {
            pgnGames[pgnGame.Id] = pgnGame;
            Games.Add(pgnGame);
            if (string.IsNullOrEmpty(Filename))
            {
                return;
            }

            File.AppendAllText(Filename, pgnGame.GetGame() + Environment.NewLine);
        }

        public void DeleteGame(PgnGame pgnGame)
        {
            pgnGames.Remove(pgnGame.Id);
            Games.Remove(pgnGame);
            Save();
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(Filename))
            {
                return;
            }

            File.WriteAllLines(Filename, pgnGames.Select(g => g.Value.GetGame() + Environment.NewLine));
        }

        public void Save(string fileName)
        {
            File.WriteAllLines(fileName, pgnGames.Select(g => g.Value.GetGame() + Environment.NewLine));
        }


        private void LoadFromArray(string[] allLines)
        {
            PgnGame currentGame = null;
            int insideComment = 0;
            //using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //{
            //    using (BufferedStream bs = new BufferedStream(fs))
            //    {
            //        using (StreamReader sr = new StreamReader(bs))
            //        {
            //            string line;
            //            while ((line = sr.ReadLine()) != null)
            //            {

            //            }
            //        }
            //    }
            //}
            foreach (var allLine in allLines)
            {
                var line = allLine.Trim();
                if (line.StartsWith("[Event ", StringComparison.OrdinalIgnoreCase))
                {
                    if (currentGame != null)
                    {
                        pgnGames[currentGame.Id] = currentGame;
                        Games.Add(currentGame);
                    }

                    currentGame = new PgnGame
                                  {
                                      GameEvent = line
                                                  .Replace("[Event", string.Empty)
                                                  .Replace("]", string.Empty)
                                  };
                    insideComment = 0;
                    continue;
                }
                if (insideComment==0 && line.StartsWith("["))
                {
                    line = line.Replace("[", string.Empty).Replace("]", string.Empty);
                    var indexOf = line.IndexOf(" ", StringComparison.Ordinal);
                    currentGame?.AddValue(line.Substring(0, indexOf), line.Substring(indexOf));
                    continue;
                }

                bool startsWithDollar = false;
                string newLine = string.Empty;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '{' || line[i] == '(')
                    {
                        insideComment++;
                        continue;
                    }

                    if (line[i] == '}' || line[i] == ')')
                    {
                        insideComment--;
                        continue;
                    }

                    if (insideComment>0)
                    {
                        continue;
                    }

                    if (line[i] == '$')
                    {
                        startsWithDollar = true;
                        continue;
                    }

                    if (startsWithDollar && line[i] != ' ')
                    {
                        continue;
                    }
                    startsWithDollar = false;
                    newLine += line[i];
                }

                if (newLine.Length == 0)
                {
                    continue;
                }
                var strings = newLine.Split(" ".ToCharArray());
                foreach (var s in strings)
                {
                    if (currentGame != null && currentGame.Result.Contains(s))
                    {
                        continue;
                    }

                    if (s.Contains("."))
                    {
                        if (s.EndsWith("."))
                        {
                            continue;
                        }

                        currentGame?.AddMove(s.Substring(s.IndexOf(".") + 1));
                        continue;
                    }


                    currentGame?.AddMove(s);
                }
            }

            if (currentGame != null )
            {
                pgnGames[currentGame.Id] = currentGame;
                Games.Add(currentGame);
                //  AddGame(currentGame);
            }
        }

        private void LoadFromFile()
        {
            Clear();
            if (!File.Exists(Filename))
            {
                return;
            }

            LoadFromArray(File.ReadAllLines(Filename));
        }
    }
}
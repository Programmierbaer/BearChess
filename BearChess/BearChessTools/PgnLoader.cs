using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public class PgnLoader
    {

        public string Filename { get; private set; }
        //public PgnGame[] Games => pgnGames.Values.ToArray();


        
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

        public PgnGame GetGame(string pgnGame)
        {

            PgnGame currentGame = null;
            int insideComment = 0;
            using (var fs = new MemoryStream(Encoding.UTF8.GetBytes(pgnGame ?? "")))
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
                                    return currentGame;
                                    
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


            return currentGame;
        }



      
    }
}
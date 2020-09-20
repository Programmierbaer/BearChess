using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessTools.Annotations;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public class PgnLoader 
    {

        private readonly Dictionary<Guid,PgnGame> pgnGames = new Dictionary<Guid, PgnGame>();

        public string Filename { get; private set; }

        public BindingList<PgnGame> Games = new BindingList<PgnGame>();
        //public PgnGame[] Games => pgnGames.Values.ToArray();

        public void Clear()
        {
            pgnGames.Clear();
            Games.Clear();

        }

        public void Load(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {

            }
            Filename = fileName;
            LoadFromFile();
        }

        public void AddGame(PgnGame pgnGame)
        {
            pgnGames[pgnGame.Id] = pgnGame;
            Games.Add(pgnGame);
            if (string.IsNullOrEmpty(Filename))
            {
                return;
            }
            File.AppendAllText(Filename,pgnGame.GetGame() + Environment.NewLine);


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
            File.WriteAllLines(fileName,pgnGames.Select(g => g.Value.GetGame()+Environment.NewLine));
        }

        private void LoadFromFile()
        {
            Clear();
            PgnGame currentGame = null;
            if (!File.Exists(Filename))
            {
                return;
            }

            var allLines = File.ReadAllLines(Filename);
            foreach (var allLine in allLines)
            {
                string line = allLine.Trim();
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
                    continue;
                }

                if (line.StartsWith("["))
                {
                    line = line.Replace("[", string.Empty).Replace("]", string.Empty);
                    var indexOf = line.IndexOf(" ", StringComparison.Ordinal);
                    currentGame?.AddValue(line.Substring(0,indexOf),line.Substring(indexOf));
                    continue;
                }

                var strings = line.Split(" ".ToCharArray());
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
                        currentGame?.AddMove(s.Substring(s.IndexOf(".")+1));
                        continue;
                    }

                 
                    currentGame?.AddMove(s);
                }
            }
            if (currentGame != null && !string.IsNullOrEmpty(currentGame.GameEvent))
            {
                pgnGames[currentGame.Id] = currentGame;
                Games.Add(currentGame);
              //  AddGame(currentGame);
            }

        }


    }
}

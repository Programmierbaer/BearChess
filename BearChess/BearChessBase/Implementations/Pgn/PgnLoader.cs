using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    public class PgnLoader
    {

        public string Filename { get; private set; }
        //public PgnGame[] Games => pgnGames.Values.ToArray();

        public IEnumerable<PgnGame> Load(string fileName)
        {
             Filename = fileName;
            PgnGame currentGame = null;
            StringBuilder sb = new StringBuilder();
            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string allLine;
                        bool startNewGame = false;
                        while ((allLine = sr.ReadLine()) != null)
                        {
                            var line = allLine.Trim();
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }
                            if (!startNewGame && line.StartsWith("[Event", StringComparison.OrdinalIgnoreCase))
                            {
                                startNewGame = true;
                                if (sb.Length> 0)
                                {
                                    currentGame = GetGame(sb.ToString());
                                    sb.Clear();
                                    yield return currentGame;
                                }

                                sb.AppendLine(line);
                            }
                            else
                            {
                                if (startNewGame && !line.StartsWith("["))
                                {
                                    startNewGame = false;
                                }
                                sb.AppendLine(line);
                            }
                        }
                        if (sb.Length > 0)
                        {
                            currentGame = GetGame(sb.ToString());
                            sb.Clear();
                            yield return currentGame;
                        }
                    }
                }
            }
       

            //if (currentGame != null)
            //{
            //    yield return currentGame;
            //}
        }

       

        public PgnGame GetGame(string pgnGame)
        {
            PgnGame currentGame = null;
            int insideComment = 0;
            var allLines = pgnGame.Split(new[] { Environment.NewLine },StringSplitOptions.RemoveEmptyEntries);
            int ji = 0;
            string line;
            bool startNewGame = false;
            for (int j= 0; j < allLines.Length; j++)
            {
                line = allLines[j];
                if (line.Length == 0)
                {
                    continue;
                }
                if (!startNewGame && line.StartsWith("[", StringComparison.OrdinalIgnoreCase))
                {
                    startNewGame = true;
                   // ji += line.Length +2;
                    currentGame = new PgnGame();

                    //                    continue;
                }
                if (line.StartsWith("["))
                {
                    ji += line.Length + 2;
                    line = line.Replace("[", string.Empty).Replace("]", string.Empty);
                    var indexOf = line.IndexOf(" ", StringComparison.Ordinal);
                    currentGame?.AddValue(line.Substring(0, indexOf), line.Substring(indexOf));
                    continue;
                }
                break;
            }

            line = pgnGame.Replace("\r", " ").Replace("\n", " ");
            bool startsWithDollar = false;
            string newLine = string.Empty;
            string prevLine = string.Empty;
            string comment = string.Empty;
            string emt = string.Empty;
            string currentSign = string.Empty;
            for (int i = ji; i < line.Length; i++)
            {
                if (line[i] == '{' || line[i] == '(')
                {
                    insideComment++;
                    comment += line[i];
                    continue;
                }

                if (line[i] == '}' || line[i] == ')')
                {
                    insideComment--;
                    comment += line[i];
                    continue;
                }

                if (insideComment > 0)
                {
                    comment += line[i];
                    continue;
                }

                if (comment.Contains("[%emt"))
                {
                    var indexOf = comment.IndexOf("[%emt");
                    emt = string.Empty;
                    string dummy = string.Empty;
                    while (!comment[indexOf].Equals(']'))
                    {
                        if (comment[indexOf].Equals(' ') || emt.Length>0)
                        {
                            emt += comment[indexOf];
                        }
                        dummy += comment[indexOf];
                        indexOf++;
                    }

                    emt = emt.Trim();
                    dummy += "]";
                    comment = comment.Replace(dummy, string.Empty);

                }
                if (line[i] == '$')
                {
                    if (!string.IsNullOrWhiteSpace(currentSign))
                    {
                        comment += currentSign + " ";
                    }
                    currentSign = "$";
                    startsWithDollar = true;
                    continue;
                }

                if (startsWithDollar && line[i] != ' ')
                {
                    currentSign += line[i];
                    continue;
                }
                startsWithDollar = false;
                if (!string.IsNullOrWhiteSpace(currentSign))
                {   
                    comment += currentSign + " "; 
                    currentSign = string.Empty;
                    
                }

                if (line[i].Equals(' '))
                {
                    bool nextIsComment = false;
                    if (!string.IsNullOrWhiteSpace(comment) || !string.IsNullOrWhiteSpace(currentSign))
                    {
                        for (int k = i + 1; k < line.Length; k++)
                        {
                            if (line[k] == '{' || line[k] == '(' || line[k] == '$')
                            {
                                nextIsComment = true;
                                break;
                            }

                            if (line[k] != ' ')
                            {
                                break;
                            }
                        }
                    }

                    if (nextIsComment)
                    {
                        continue;
                    }
                    if (newLine.Contains("."))
                    {
                        if (newLine.EndsWith("."))
                        {
                            newLine = string.Empty;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(prevLine))
                        {
                            prevLine = newLine.Substring(newLine.LastIndexOf(".", StringComparison.Ordinal) + 1);
                            newLine = string.Empty;
                            comment = string.Empty;
                            emt = string.Empty;
                            currentSign = string.Empty;
                            continue;
                        }
                        currentGame?.AddMove(prevLine.Substring(prevLine.LastIndexOf(".", StringComparison.Ordinal) + 1),  comment);
                        prevLine = newLine.Substring(newLine.LastIndexOf(".", StringComparison.Ordinal) + 1);
                        newLine = string.Empty;
                        comment = string.Empty;
                        emt = string.Empty;
                        currentSign = string.Empty;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(prevLine))
                    {
                        prevLine = newLine;
                        newLine = string.Empty;
                        comment = string.Empty;
                        emt = string.Empty;
                        currentSign = string.Empty;
                        continue;
                    }
                    currentGame?.AddMove(prevLine, comment, emt);
                    prevLine = newLine;
                    newLine = string.Empty;
                    comment = string.Empty;
                    currentSign = string.Empty;
                    continue;
                }
                newLine += line[i];
            }
           
            return currentGame;
        }

        private string NagToSymbo(string nag)
        {
            if (PgnDefinitions.NagToSymbol.ContainsKey(nag))
            {
                return PgnDefinitions.NagToSymbol[nag];
            }

            return string.Empty;
        }

      
    }
}
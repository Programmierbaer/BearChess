﻿using System;
using System.Collections.Generic;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    [Serializable]
    public class PgnGame
    {
        public Guid Id { get; set; }
        private readonly List<string> _moveList = new List<string>();
        private readonly List<string> _commentList = new List<string>();
        private readonly List<string> _emtList = new List<string>();
        private readonly Dictionary<string, string> _userDefined = new Dictionary<string, string>();
        private readonly string[] _order = {"Event", "Site", "Date", "Round", "White", "Black", "Result"};

        public string GameEvent
        {
            get => GetValue("Event");
            set => AddValue("Event", value);
        }

        public string GameSite
        {
            get => GetValue("Site");
            set => AddValue("Site", value);
        }
        public string GameDate
        {
            get => GetValue("Date");
            set => AddValue("Date", value);
        }
        public string PlayerWhite
        {
            get => GetValue("White");
            set => AddValue("White", value);
        }
        public string PlayerBlack
        {
            get => GetValue("Black");
            set => AddValue("Black", value);
        }
        public string Result
        {
            get => GetValue("Result");
            set => AddValue("Result", value);
        }
        public string Round
        {
            get => GetValue("Round");
            set => AddValue("Round", value);
        }

        public string WhiteElo
        {
            get => GetValue("WhiteElo");
            set => AddValue("WhiteElo", value);
        }

        public string BlackElo
        {
            get => GetValue("BlackElo");
            set => AddValue("BlackElo", value);
        }
        public string FENLine
        {
            get => GetValue("FEN");
            set => AddValue("FEN", value);
        }

        public string SetUp
        {
            get => GetValue("SetUp");
            set => AddValue("SetUp", value);
        }

        public string MoveList => GetMoveList();

        public int MoveCount => _moveList.Count;

        public string GetMove(int index)
        {
            return _moveList[index];
        }

        public string GetComment(int index)
        {
            return _commentList[index];
        }

        public string GetEMT(int index)
        {
            return _emtList[index];
        }

        public PgnGame()
        {
            Id = Guid.NewGuid();
        }

        public string GetValue(string keyWord)
        {
            return _userDefined.ContainsKey(keyWord) ? _userDefined[keyWord] : string.Empty;
        }

        public void AddValue(string keyWord, string keyValue)
        {
            if (keyValue==null)
            {
                return;
            }

            if (keyWord.StartsWith("Result") && keyValue.StartsWith("1/2"))
            {
                keyValue = "1/2-1/2";
            }
            _userDefined[keyWord] = keyValue.Replace("\"",string.Empty).Trim();
        }

        public void AddMove(string move)
        {
            AddMove(move, string.Empty);
        }
        public void AddMove(string move, string comment)
        {
            AddMove(move,comment,string.Empty);
        }

        public void AddMove(string move, string comment, string emt)
        {
            _moveList.Add(move);
            _commentList.Add(comment);
            _emtList.Add(emt);
        }

        public string GetMoveList()
        {
            var sb = new StringBuilder();
            int moveCnt = 0;
            bool newMove = true;
            foreach (var s in _moveList)
            {
                if (newMove)
                {
                    moveCnt++;
                    sb.Append($"{moveCnt}.{s}");
                    newMove = false;
                }
                else
                {
                    sb.Append($" {s} ");
                    newMove = true;
                }
            }
            return sb.ToString();
        }

        public void ClearMoveList()
        {
            _moveList.Clear();
            _commentList.Clear();
            _emtList.Clear();
        }


        public string GetGame()
        {
            var sb = new StringBuilder();
            var mandatory = new HashSet<string>(_order);
            foreach (var s in _order)
            {
                if (s.Equals("Date"))
                {
                    var value = GetValue(s);
                    if (DateTime.TryParse(value.Replace("??", "01"), out  DateTime gameDate))
                    {
                        value = gameDate.ToString("yyyy.MM.dd");

                    }
                    sb.AppendLine($"[{s} \"{value}\"]");
                }
                else
                {
                    var value = GetValue(s);
                    if (s.Equals("Result"))
                    {
                        if (value.StartsWith("1/2"))
                        {
                            value = "1/2-1/2";
                        }
                    }
                    sb.AppendLine($"[{s} \"{value}\"]");
                }
            }
            foreach (var key in _userDefined.Keys)
            {
                if (mandatory.Contains(key))
                {
                    continue;
                }

                var value = GetValue(key);
                if (key.Equals("FEN") || key.Equals("SetUp"))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }
                }
                sb.AppendLine($"[{key} \"{value}\"]");
            }

            int moveCnt = 0;
            bool newMove = true;
            foreach (var s in _moveList)
            {
                if (newMove)
                {
                    moveCnt++;
                    sb.Append($"{moveCnt}.{s}");
                    newMove = false;
                }
                else
                {
                    sb.Append($" {s} ");
                    newMove = true;
                }
            }

            sb.Append($" {Result}");
            return sb.ToString();
        }
    }
}
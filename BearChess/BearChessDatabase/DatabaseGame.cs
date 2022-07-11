using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    [Serializable]
    public class DatabaseGame
    {
        public CurrentGame CurrentGame { get; set; }
        public PgnGame PgnGame { get; set; }
        public ClockTime WhiteClockTime { get; set; }
        public ClockTime BlackClockTime { get; set; }
        public int Round  { get; set; }

        [XmlArrayItem("ListOfMoves")]
        public Move[] AllMoves;

        private DateTime _gameDate;

        [XmlIgnore]
        public string White => PgnGame.PlayerWhite;

        [XmlIgnore]
        public string Black => PgnGame.PlayerBlack;

        [XmlIgnore]
        public string GameEvent => PgnGame.GameEvent;

        [XmlIgnore]
        public string Pgn => PgnGame.GetGame();

        [XmlIgnore]
        public string PgnMoveList => PgnGame.GetMoveList();

        [XmlIgnore]
        public string Result => PgnGame.Result;

        [XmlIgnore]
        public string WhiteElo => PgnGame.WhiteElo;

        [XmlIgnore]
        public string BlackElo => PgnGame.BlackElo;

        [XmlIgnore]
        public DateTime GameDate
        {
            get => _gameDate;
        }

        [XmlIgnore]
        public Move[] MoveList => AllMoves;

        [XmlIgnore]
        public int Id { get; set; }

        public DatabaseGame(PgnGame pgnGame, Move[] moveList, CurrentGame currentGame)
        {
            CurrentGame = currentGame;
            PgnGame = pgnGame;
            if (!DateTime.TryParse(pgnGame.GameDate.Replace("??", "01"), out _gameDate))
            {
                _gameDate = DateTime.Now;
            }
            List<Move> myMove = new List<Move>();
            foreach (var move in moveList)
            {
                myMove.Add(move);
            }

            AllMoves = myMove.ToArray();
            if (currentGame!=null)
              Round = currentGame.Round;
            Id = 0;
        }

        public DatabaseGame()
        {
            
        }
        public void Reset()
        {
            AllMoves = Array.Empty<Move>();
            WhiteClockTime = new ClockTime();
            BlackClockTime = new ClockTime();
            PgnGame.Result = "*";
            PgnGame.ClearMoveList();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
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

        [XmlIgnore]
        public string White => PgnGame.PlayerWhite;
        [XmlIgnore]
        public string Black => PgnGame.PlayerBlack;
        [XmlIgnore]
        public string GameEvent => PgnGame.GameEvent;
        [XmlIgnore]
        public string Pgn => PgnGame.GetGame();
        [XmlIgnore]
        public string Result => PgnGame.Result;
        
        [XmlIgnore]
        public DateTime GameDate => DateTime.Parse(PgnGame.GameDate);
        [XmlIgnore]
        public Move[] MoveList => AllMoves;

        public DatabaseGame(PgnGame pgnGame, Move[] moveList, CurrentGame currentGame)
        {
            CurrentGame = currentGame;
            PgnGame = pgnGame;
            List<Move> myMove = new List<Move>();
            foreach (var move in moveList)
            {
                myMove.Add(move);
            }

            AllMoves = myMove.ToArray();
            if (currentGame!=null)
              Round = currentGame.Round;
        }

        public DatabaseGame()
        {
            
        }
    }
}
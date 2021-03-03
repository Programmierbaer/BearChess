using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    [Serializable]
    public class DatabaseGameSimple
    {
        public int Id { get; set; }

        public string White { get; set; }
        
        public string Black { get; set; }
        
        public string GameEvent { get; set; }

        public string GameSite { get; set; }

        public string Result { get; set; }
        
        public string GameDate { get; set; }
        
        public string MoveList { get; set; }

        public DatabaseGameSimple()
        {
            
        }

    }

    [Serializable]
    public class DatabaseGame
    {
        public PgnGame PgnGame;
        
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


        public DatabaseGame(PgnGame pgnGame, Move[] moveList)
        {
            PgnGame = pgnGame;
            List<Move> myMove = new List<Move>();
            foreach (var move in moveList)
            {
                myMove.Add(move);
            }

            AllMoves = myMove.ToArray();
        }

        public DatabaseGame()
        {
            
        }
    }
}
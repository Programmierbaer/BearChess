using System;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase
{
    [Serializable]
    public class CurrentGame
    {

        public UciInfo WhiteConfig { get; set; }

        public UciInfo BlackConfig { get; set; }

        public TimeControl TimeControl { get; set; }

        public string PlayerWhite { get; set; }
        
        public string PlayerBlack { get; set; }
        
        public bool StartFromBasePosition { get; set; }
        public bool DuelEngine { get; set; }
        public bool DuelEnginePlayer { get; set; }
        public int CurrentDuelGame { get; set; }
        public int DuelGames { get; set; }
        public int Round { get; set; }
        public bool SwitchedColor { get; set; }
        public string GameEvent { get; set; }

        public bool RepeatedGame { get; set; }
     
        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControl,
                           string playerWhite, string playerBlack, bool startFromBasePosition)
        : this (whiteConfig, blackConfig,gameEvent, timeControl,playerWhite,playerBlack,startFromBasePosition,false,1, false)
        {
        
        }

        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControl,
                           string playerWhite, string playerBlack, bool startFromBasePosition, bool duelEngine, int duelGames, bool duelEnginePlayer)
        {
            WhiteConfig = whiteConfig;
            BlackConfig = blackConfig;
            GameEvent = gameEvent;
            TimeControl = timeControl;
            PlayerWhite = playerWhite;
            PlayerBlack = playerBlack;
            StartFromBasePosition = startFromBasePosition;
            DuelEngine = duelEngine;
            CurrentDuelGame = 1;
            Round = 1;
            DuelGames = duelGames;
            DuelEnginePlayer = duelEnginePlayer;
            SwitchedColor = false;
            RepeatedGame = false;
        }

        public CurrentGame()
        {
            
        }
    }
}

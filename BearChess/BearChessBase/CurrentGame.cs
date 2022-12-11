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
        public TimeControl TimeControlBlack { get; set; }

        public string PlayerWhite { get; set; }
        
        public string PlayerBlack { get; set; }
        
        public bool StartFromBasePosition { get; set; }
        public bool ContinueGame { get; set; }
        public bool DuelEngine { get; set; }
        public bool DuelEnginePlayer { get; set; }
        public int CurrentDuelGame { get; set; }
        public int DuelGames { get; set; }
        public int Round { get; set; }
        public bool SwitchedColor { get; set; }
        public string GameEvent { get; set; }
        public string GameNumber { get; set; }

        public bool RepeatedGame { get; set; }
     
        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControl,
                           string playerWhite, string playerBlack, bool startFromBasePosition, bool continueGame)
        : this (whiteConfig, blackConfig,gameEvent, timeControl,null,playerWhite,playerBlack,startFromBasePosition,false,1, continueGame)
        {
        
        }
        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControlWhite, TimeControl timeControlBlack,
                           string playerWhite, string playerBlack, bool startFromBasePosition, bool continueGame)
            : this(whiteConfig, blackConfig, gameEvent, timeControlWhite, timeControlBlack, playerWhite, playerBlack, startFromBasePosition, false, 1, false, continueGame)
        {

        }

        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControlWhite, TimeControl timeControlBlack,
                           string playerWhite, string playerBlack, bool startFromBasePosition, bool duelEngine,
                           int duelGames, bool duelEnginePlayer): this(whiteConfig, blackConfig, gameEvent, timeControlWhite,timeControlBlack, playerWhite, playerBlack, startFromBasePosition, duelEngine,duelGames,duelEnginePlayer, false)
        {
           
           
        }

        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControlWhite, TimeControl timeControlBlack,
                           string playerWhite, string playerBlack, bool startFromBasePosition, bool duelEngine, int duelGames, bool duelEnginePlayer, bool continueGame)
        {

            WhiteConfig = whiteConfig;
            BlackConfig = blackConfig;
            GameEvent = gameEvent;
            TimeControl = timeControlWhite;
            TimeControlBlack = timeControlBlack ?? timeControlWhite;
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
            ContinueGame = continueGame;
        }

        public CurrentGame()
        {
            
        }
    }
}

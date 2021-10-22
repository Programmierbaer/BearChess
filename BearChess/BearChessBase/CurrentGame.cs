using System;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase
{
    [Serializable]
    public class CurrentGame
    {
        private UciInfo _whiteConfig;
        private UciInfo _blackConfig;
        private string _playerWhite;
        private string _playerBlack;

        public UciInfo WhiteConfig
        {
            get => SwitchedColor ? _blackConfig : _whiteConfig;
            set => _whiteConfig = value;
        }

        public UciInfo BlackConfig
        {
            get => SwitchedColor ? _whiteConfig : _blackConfig;
            set => _blackConfig = value;
        }

        public TimeControl TimeControl { get; set; }

        public string PlayerWhite
        {
            get => SwitchedColor ? _playerBlack : _playerWhite;
            set => _playerWhite = value;
        }

        public string PlayerBlack
        {
            get => SwitchedColor ? _playerWhite : _playerBlack;
            set => _playerBlack = value;
        }

        public bool StartFromBasePosition { get; set; }
        public bool DuelEngine { get; set; }
        public int CurrentDuelGame { get; set; }
        public int DuelGames { get; set; }
        public int Round { get; set; }
        public bool SwitchedColor { get; set; }
        public string GameEvent { get; set; }

        public bool PlayerWhiteIsMessChess => WhiteConfig.FileName.EndsWith("MessChess.exe", StringComparison.OrdinalIgnoreCase);
        public bool PlayerBlackIsMessChess => BlackConfig.FileName.EndsWith("MessChess.exe", StringComparison.OrdinalIgnoreCase);


        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControl,
                           string playerWhite, string playerBlack, bool startFromBasePosition)
        : this (whiteConfig, blackConfig,gameEvent, timeControl,playerWhite,playerBlack,startFromBasePosition,false,1)
        {
        
        }

        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, string gameEvent, TimeControl timeControl,
                           string playerWhite, string playerBlack, bool startFromBasePosition, bool duelEngine, int duelGames)
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
            SwitchedColor = false;
        }

        public CurrentGame()
        {
            
        }
    }
}

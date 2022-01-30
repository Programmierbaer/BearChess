using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase
{
    [Serializable]
    public class CurrentDuel
    {
        [XmlArray("Players")]
        public UciInfo[] Players;
        public TimeControl TimeControl { get; set; }
        public bool DuelSwitchColor { get; set; }
        public bool DuelSaveGames { get; set; }
        public string GameEvent { get; set; }
        public bool StartFromBasePosition { get; set; }
        public int Cycles { get; set; }
        public bool AdjustEloWhite { get; set; }
        public bool AdjustEloBlack { get; set; }
        public int AdjustEloStep { get; set; }
        public int CurrentMaxElo { get; set; }
        public int CurrentMinElo { get; set; }


        public CurrentDuel(List<UciInfo> players, TimeControl timeControl,  int cycles, 
                           bool duelSwitchColor, string gameEvent, bool startFromBasePosition, bool adjustEloWhite, bool adjustEloBlack,
                           int adjustEloStep)
        {
            TimeControl = timeControl;
            Cycles = cycles;
            DuelSwitchColor = duelSwitchColor;
            GameEvent = gameEvent;
            StartFromBasePosition = startFromBasePosition;
            AdjustEloWhite = adjustEloWhite;
            AdjustEloBlack = adjustEloBlack;
            AdjustEloStep = adjustEloStep;
            Players = players.ToArray();
            DuelSaveGames = true;
            CurrentMaxElo = -1;
            CurrentMinElo = -1;
        }

        public CurrentDuel()
        {

        }


    }
}
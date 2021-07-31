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


        public CurrentDuel(List<UciInfo> players, TimeControl timeControl,  int cycles, bool duelSwitchColor, string gameEvent, bool startFromBasePosition)
        {
            TimeControl = timeControl;
            Cycles = cycles;
            DuelSwitchColor = duelSwitchColor;
            GameEvent = gameEvent;
            StartFromBasePosition = startFromBasePosition;
            Players = players.ToArray();
            DuelSaveGames = true;
        }

        public CurrentDuel()
        {

        }


    }
}
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase
{
    [Serializable]
    public class CurrentTournament
    {
        [XmlArray("Players")]
        public UciInfo[] Players;
        public TimeControl TimeControl { get; set; }
        public bool TournamentSwitchColor { get; set; }
        public bool TournamentSaveGames { get; set; }
        public string GameEvent { get; set; }
        public bool StartFromBasePosition { get; set; }
        public TournamentTypeEnum TournamentType { get; set; }
        public int Cycles { get; set; }
        public int Deliquent { get; set; }


        public CurrentTournament(List<UciInfo> players, int deliquent, TimeControl timeControl,TournamentTypeEnum tournamentType, int cycles,  bool tournamentSwitchColor, string gameEvent, bool startFromBasePosition)
        {
            Deliquent = deliquent;
            TimeControl = timeControl;
            TournamentType = tournamentType;
            Cycles = cycles;
            TournamentSwitchColor = tournamentSwitchColor;
            GameEvent = gameEvent;
            StartFromBasePosition = startFromBasePosition;
            Players = players.ToArray();
            TournamentSaveGames = true;
        }

        public CurrentTournament()
        {

        }


    }
}
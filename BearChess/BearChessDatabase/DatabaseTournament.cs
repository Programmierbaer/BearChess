using System;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    [Serializable]
    public class DatabaseTournament
    {
        public int TournamentId { get; set; }
        public CurrentTournament CurrentTournament { get; set; }
        public int PlayedGames { get; set; }
        public int GamesToPlay { get; set; }
        public string State { get; set; }
        public DateTime EventDate { get; set; }
        public string Participants { get; set; }
    }
}
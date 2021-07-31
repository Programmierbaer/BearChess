using System;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    [Serializable]
    public class DatabaseDuel
    {
        public int DuelId { get; set; }
        public CurrentDuel CurrentDuel { get; set; }
        public int PlayedGames { get; set; }
        public int GamesToPlay { get; set; }
        public string State { get; set; }
        public DateTime EventDate { get; set; }
        public string Participants { get; set; }
    }
}
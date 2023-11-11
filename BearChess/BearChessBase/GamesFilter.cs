using System;

namespace www.SoLaNoSoft.com.BearChessBase
{
    [Serializable]
    public class GamesFilter
    {
        public bool FilterIsActive { get; set; }
        public bool NoTournamentGames { get; set; }
        public bool NoDuelGames { get; set; }
        public bool OnlyDuplicates { get; set; }
        public string WhitePlayer { get; set; }
        public bool WhitePlayerWhatever { get; set; }
        public string BlackPlayer { get; set; }
        public bool BlackPlayerWhatever { get; set; }
        public string GameEvent { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

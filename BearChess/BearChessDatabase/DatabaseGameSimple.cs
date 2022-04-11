using System;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    [Serializable]
    public class DatabaseGameSimple
    {
        public int Id { get; set; }

        public string White { get; set; }

        public string WhiteElo { get; set; }

        public string Black { get; set; }
        public string BlackElo { get; set; }
        
        public string GameEvent { get; set; }

        public string GameSite { get; set; }

        public string Result { get; set; }
        
        public DateTime GameDate { get; set; }
        
        public string MoveList { get; set; }

        public string Round { get; set; }

        public int PgnHash { get; set; }

        public DatabaseGameSimple()
        {
            
        }

    }
}
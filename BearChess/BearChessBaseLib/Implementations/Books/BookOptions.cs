using System;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    [Serializable]
    public class BookOptions
    {
       
        public string FileName { get; set; }
        public int MaxLength { get; set; }
        public bool PreferMainLines { get; set; }
        public bool TournamentMode { get; set; }

        public double Random = 0;

        public BookOptions()
        {
            
        }

        public BookOptions(BookOptions other)
        {
            FileName = other.FileName;
            MaxLength = other.MaxLength;
            PreferMainLines = other.PreferMainLines;
            TournamentMode = other.TournamentMode;
            Random = other.Random;
        }
    }
}
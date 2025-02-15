using System;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    [Serializable]
    public class TwicDownload
    {
        public int Id { get; set; }
        public int TwicNumber { get; set; }
        public int NumberOfGames { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime FileDate { get; set; }
        public string FileName { get; set; }

    }
}
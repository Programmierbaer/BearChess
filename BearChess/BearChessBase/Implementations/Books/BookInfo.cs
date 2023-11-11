using System;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    [Serializable]
    public class BookInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public int PositionsCount { get; set; }
        public int MovesCount { get; set; }
    }
}
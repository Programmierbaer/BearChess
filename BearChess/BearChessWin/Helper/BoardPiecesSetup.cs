using System;

namespace www.SoLaNoSoft.com.BearChessWin
{
    [Serializable]
    public class BoardPiecesSetup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string WhiteKingFileName { get; set; }
        public string WhiteQueenFileName { get; set; }
        public string WhiteRookFileName { get; set; }
        public string WhiteBishopFileName { get; set; }
        public string WhiteKnightFileName { get; set; }
        public string WhitePawnFileName { get; set; }
        public string BlackKingFileName { get; set; }
        public string BlackQueenFileName { get; set; }
        public string BlackRookFileName { get; set; }
        public string BlackBishopFileName { get; set; }
        public string BlackKnightFileName { get; set; }
        public string BlackPawnFileName { get; set; }
        public bool OneLine { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public class DataFromBoard
    {
        public string FromBoard { get; }
        public ulong Repeated { get; }
        public bool PlayWithWhite { get; }
        public bool BasePosition { get; }

        public DataFromBoard(string fromBoard, ulong repeated=0)
        {
            FromBoard = fromBoard;
            Repeated = repeated;
            BasePosition = fromBoard.Equals("RNBKQBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbkqbnr") ||
                           fromBoard.Equals("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
            PlayWithWhite = fromBoard.Equals("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        }
    }
}
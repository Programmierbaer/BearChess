using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public class DataFromBoard
    {
        public string FromBoard { get; }
       
        public ulong Repeated { get; }
        public bool PlayWithWhite { get; }
        public bool BasePosition { get; set; }
        public bool Invalid { get; set; }

        public bool IsFieldDump { get; set; }

        public DataFromBoard(string fromBoard, ulong repeated=0)
        {
            FromBoard = fromBoard;
            Repeated = repeated;
            BasePosition = fromBoard.StartsWith(FenCodes.BlackBoardBasePosition) ||
                           fromBoard.StartsWith(FenCodes.WhiteBoardBasePosition);
            PlayWithWhite = fromBoard.StartsWith(FenCodes.WhiteBoardBasePosition);
            Invalid = false;
            IsFieldDump = false;
        }
    }
}
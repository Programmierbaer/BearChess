using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    public class PgnBoard : AbstractPgnCreator
    {
   
        public PgnBoard()
        {
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
        }

        public PgnBoard(string fenStartPosition) : this()
        {
            _fenStartPosition = fenStartPosition;
            _chessBoard.SetPosition(fenStartPosition, false);
        }

        public void NewGame()
        {
            _chessBoard.Init();
            _chessBoard.NewGame();
        }

        public void AddMove(Move move)
        {
            _chessBoard.MakeMove(move);
        }

        public string[] GetMoveList()
        {
            List<string> moveList = new List<string>();
            var generateMoveList = _chessBoard.GenerateMoveList();
            var moveCnt = 0;
            var newMove = true;
            foreach (var move in generateMoveList)
            {
                if (newMove)
                {
                    moveCnt++;

                }
                moveList.Add(ConvertToPgnMove(move, moveCnt));
                newMove = !newMove;
            }

            return moveList.ToArray();
        }
    }
}
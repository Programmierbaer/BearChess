using System.Collections.Generic;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    public class PgnCreator : AbstractPgnCreator
    {
       

        public PgnCreator(bool purePGN)
        {
            _purePGN = purePGN;
            _allPgnMoves = new List<string>();
            _allMoves = new List<Move>();
            _fenStartPosition = string.Empty;
        }

        public PgnCreator(string fenStartPosition, bool purePGN) : this(purePGN)
        {
            _fenStartPosition = fenStartPosition;
        }



        public string[] GetAllMoves()
        {
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            if (!string.IsNullOrEmpty(_fenStartPosition))
            {
                _chessBoard.SetPosition(_fenStartPosition, false);
            }
            _allPgnMoves.Clear();
            var moveCnt = 0;
            var newMove = true;
            foreach (var move in _allMoves)
            {
                if (newMove)
                {
                    moveCnt++;
                
                }
                _allPgnMoves.Add(ConvertToPgnMove(move, moveCnt));
                newMove = !newMove;
            }

            return _allPgnMoves.ToArray();
        }

        public void AddMove(Move move)
        {
            _allMoves.Add(move);
        }

        public string GetMoveList()
        {
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _chessBoard.NewGame();
            if (!string.IsNullOrEmpty(_fenStartPosition))
            {
                _chessBoard.SetPosition(_fenStartPosition, false);
            }
            var sb = new StringBuilder();
            var moveCnt = 0;
            var newMove = true;
            foreach (var move in _allMoves)
            {
                if (newMove)
                {
                    moveCnt++;
                }
                var m = ConvertToPgnMove(move, moveCnt);
                if (newMove)
                {
                    sb.Append($"{moveCnt}.{m}");
                    newMove = false;
                }
                else
                {
                    sb.Append($" {m} ");
                    newMove = true;
                }
            }

            return sb.ToString();
        }

    }
}
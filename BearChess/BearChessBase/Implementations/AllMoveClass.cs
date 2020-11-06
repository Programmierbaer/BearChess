using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class AllMoveClass
    {
        public int MoveNumber { get; }
        private readonly Dictionary<int, IMove> _moves = new Dictionary<int, IMove>();
        private readonly Dictionary<int, string> _fens = new Dictionary<int, string>();

        public AllMoveClass(int moveNumber)
        {
            MoveNumber = moveNumber;
        }

        public void SetMove(int color, IMove move, string fenPosition)
        {
            _moves[color] = move;
            _fens[color] = fenPosition;
        }

        public IMove GetMove(int color)
        {
            return _moves.ContainsKey(color) ? _moves[color] : null;
        }

        public string GetFen(int color)
        {
            return _fens.ContainsKey(color) ? _fens[color] : null;
        }

    }
}
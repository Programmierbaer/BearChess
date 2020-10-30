using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class AllMoveClass
    {
        private readonly Dictionary<int, IMove> moves = new Dictionary<int, IMove>();
        private readonly Dictionary<int, string> fens = new Dictionary<int, string>();

        public void SetMove(int color, IMove move, string fenPosition)
        {
            moves[color] = move;
            fens[color] = fenPosition;
        }

        public IMove GetMove(int color)
        {
            return moves.ContainsKey(color) ? moves[color] : null;
        }

        public string GetFen(int color)
        {
            return fens.ContainsKey(color) ? fens[color] : null;
        }

    }
}
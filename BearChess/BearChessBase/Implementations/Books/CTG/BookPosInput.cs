using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    public class BookPosInput
    {
        private Position currPos;


        private Position prevPos;
        private List<Move> moves;

        public BookPosInput(Position currPos, Position prevPos, List<Move> moves)
        {
            this.currPos = currPos;
            this.prevPos = prevPos;
            this.moves = moves;
        }


        public Position getCurrPos()
        {
            return currPos;
        }

        public Position getPrevPos()
        {
            lazyInit();
            return prevPos;
        }

        public List<Move> getMoves()
        {
            lazyInit();
            return moves;
        }

        private void lazyInit()
        {
        }
    }
}
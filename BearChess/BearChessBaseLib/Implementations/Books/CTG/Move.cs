using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    public class Move
    {
        /** From square, 0-63. */
        public int From { get; set; }

        /** To square, 0-63. */
        public int To { get; set; }

        /** Promotion piece. */
        public int PromoteTo { get; set; }

        /** Create a move object. */
        public Move(int from, int to, int promoteTo)
        {
            From = from;
            To = to;
            PromoteTo = promoteTo;
        }

        public Move(Move m)
        {
            From = m.From;
            To = m.To;
            PromoteTo = m.PromoteTo;
        }

        /** Create object from compressed representation. */
        public static Move FromCompressed(int cm)
        {
            return new Move(cm >> 10 & 63, cm >> 4 & 63, cm & 15);
        }


        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;
            Move other = (Move)obj;
            if (From != other.From)
                return false;
            if (To != other.To)
                return false;
            if (PromoteTo != other.PromoteTo)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return GetCompressedMove();
        }



        /** Get move as a 16-bit value. */
        private int GetCompressedMove()
        {
            return (From * 64 + To) * 16 + PromoteTo;
        }

        /** Useful for debugging. */
        public override string ToString()
        {
            return StringHelper.MoveToUCIString(this);
        }
       


    }
}
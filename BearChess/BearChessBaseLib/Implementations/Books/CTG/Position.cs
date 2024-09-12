using System;
using System.Security.Cryptography;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    public  class Position
    {
        private int[] squares;

        public bool whiteMove;

        /** Bit definitions for the castleMask bit mask. */
        public static  int A1_CASTLE = 0; /** White long castle. */
        public static  int H1_CASTLE = 1; /** White short castle. */
        public static  int A8_CASTLE = 2; /** Black long castle. */
        public static  int H8_CASTLE = 3; /** Black short castle. */

        private int castleMask;

        private int epSquare;

        /** Number of half-moves since last 50-move reset. */
        public int halfMoveClock;

        /** Game move number, starting from 1. */
        public int fullMoveCounter;

        private long hashKey;           // Cached Zobrist hash key
        private int wKingSq, bKingSq;   // Cached king positions


        private static long[,] psHashKeys;    // [piece][square]
        private static long whiteHashKey;
        private static long[] castleHashKeys;  // [castleMask]
        private static long[] epHashKeys;      // [epFile + 1] (epFile==-1 for no ep)


        public Position()
        {
            psHashKeys = new long [Piece.NPieceTypes,64];
            //psHashKeys = new [Piece.nPieceTypes,64];
            castleHashKeys = new long[16];
            epHashKeys = new long[9];
            int rndNo = 0;
            for (int p = 0; p < Piece.NPieceTypes; p++)
            {
                for (int sq = 0; sq < 64; sq++)
                {
                    psHashKeys[p,sq] = GetRandomHashVal(rndNo++);
                }
            }
            whiteHashKey = GetRandomHashVal(rndNo++);
            for (int cm = 0; cm < castleHashKeys.Length; cm++)
                castleHashKeys[cm] = GetRandomHashVal(rndNo++);
            for (int f = 0; f < epHashKeys.Length; f++)
                epHashKeys[f] = GetRandomHashVal(rndNo++);
            squares = new int[64];
            for (int i = 0; i < 64; i++)
                squares[i] = Piece.EMPTY;
            whiteMove = true;
            castleMask = 0;
            epSquare = -1;
            halfMoveClock = 0;
            fullMoveCounter = 1;
            hashKey = ComputeZobristHash();
            wKingSq = bKingSq = -1;
        }

        public Position(Position other)
        {
            squares = new int[64];
            Array.Copy(other.squares, 0, squares, 0, 64);
            whiteMove = other.whiteMove;
            castleMask = other.castleMask;
            epSquare = other.epSquare;
            halfMoveClock = other.halfMoveClock;
            fullMoveCounter = other.fullMoveCounter;
            hashKey = other.hashKey;
            wKingSq = other.wKingSq;
            bKingSq = other.bKingSq;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType()))
                return false;
            Position other = (Position)obj;
            if (!DrawRuleEquals(other))
                return false;
            if (halfMoveClock != other.halfMoveClock)
                return false;
            if (fullMoveCounter != other.fullMoveCounter)
                return false;
            if (hashKey != other.hashKey)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return (int)hashKey;
        }

        public bool DrawRuleEquals(Position other)
        {
            for (int i = 0; i < 64; i++)
            {
                if (squares[i] != other.squares[i])
                    return false;
            }
            if (whiteMove != other.whiteMove)
                return false;
            if (castleMask != other.castleMask)
                return false;
            if (epSquare != other.epSquare)
                return false;
            return true;
        }

        public static int GetX(int square)
        {
            return square & 7;
        }

        public static int GetSquare(int x, int y)
        {
            return y * 8 + x;
        }


        public static int GetY(int square)
        {
            return square >> 3;
        }

        public static bool DarkSquare(int x, int y)
        {
            return (x & 1) == (y & 1);
        }


        public int GetPiece(int square)
        {
            return squares[square];
        }

        public void SetPiece(int square, int piece)
        {
            // Update hash key
            int oldPiece = squares[square];
            hashKey ^= psHashKeys[oldPiece, square];
            hashKey ^= psHashKeys[piece, square];

            // Update board
            squares[square] = piece;

            // Update king position
            if (piece == Piece.WKING)
            {
                wKingSq = square;
            }
            else if (piece == Piece.BKING)
            {
                bKingSq = square;
            }
        }

        public long ZobristHash()
        {
            return hashKey;
        }

        public bool A1Castle()
        {
            return (castleMask & (1 << A1_CASTLE)) != 0;
        }
        /** Return true if white short castling right has not been lost. */
        public bool H1Castle()
        {
            return (castleMask & (1 << H1_CASTLE)) != 0;
        }
        /** Return true if black long castling right has not been lost. */
        public  bool A8Castle()
        {
            return (castleMask & (1 << A8_CASTLE)) != 0;
        }
        /** Return true if black short castling right has not been lost. */
        public bool H8Castle()
        {
            return (castleMask & (1 << H8_CASTLE)) != 0;
        }
        /** Bitmask describing castling rights. */
        public int GetCastleMask()
        {
            return castleMask;
        }
        public void SetCastleMask(int castleMask)
        {
            hashKey ^= castleHashKeys[this.castleMask];
            hashKey ^= castleHashKeys[castleMask];
            this.castleMask = castleMask;
        }

        /** En passant square, or -1 if no ep possible. */
        public int GetEpSquare()
        {
            return epSquare;
        }
        public void SetEpSquare(int epSquare)
        {
            if (this.epSquare != epSquare)
            {
                hashKey ^= epHashKeys[(this.epSquare >= 0) ? GetX(this.epSquare) + 1 : 0];
                hashKey ^= epHashKeys[(epSquare >= 0) ? GetX(epSquare) + 1 : 0];
                this.epSquare = epSquare;
            }
        }

        public void SetWhiteMove(bool whiteMove)
        {
            if (whiteMove != this.whiteMove)
            {
                hashKey ^= whiteHashKey;
                this.whiteMove = whiteMove;
            }
        }


        public int GetKingSq(bool whiteMove)
        {
            return whiteMove ? wKingSq : bKingSq;
        }

        
        public int NPieces(int pType)
        {
            int ret = 0;
            for (int sq = 0; sq < 64; sq++)
                if (squares[sq] == pType)
                    ret++;
            return ret;
        }

        /** Count total number of pieces. */
        public int NPieces()
        {
            int ret = 0;
            for (int sq = 0; sq < 64; sq++)
                if (squares[sq] != Piece.EMPTY)
                    ret++;
            return ret;
        }

        public void MakeMove(Move move, UndoInfo ui)
        {
            ui.capturedPiece = squares[move.To];
            ui.castleMask = castleMask;
            ui.epSquare = epSquare;
            ui.halfMoveClock = halfMoveClock;
            bool wtm = whiteMove;

            int p = squares[move.From];
            int capP = squares[move.To];

            bool nullMove = (move.From == 0) && (move.To == 0);

            if (nullMove || (capP != Piece.EMPTY) || (p == (wtm ? Piece.WPAWN : Piece.BPAWN)))
            {
                halfMoveClock = 0;
            }
            else
            {
                halfMoveClock++;
            }
            if (!wtm)
            {
                fullMoveCounter++;
            }

            // Handle castling
            int king = wtm ? Piece.WKING : Piece.BKING;
            int k0 = move.From;
            if (p == king)
            {
                if (move.To == k0 + 2)
                { // O-O
                    SetPiece(k0 + 1, squares[k0 + 3]);
                    SetPiece(k0 + 3, Piece.EMPTY);
                }
                else if (move.To == k0 - 2)
                { // O-O-O
                    SetPiece(k0 - 1, squares[k0 - 4]);
                    SetPiece(k0 - 4, Piece.EMPTY);
                }
                if (wtm)
                {
                    SetCastleMask(castleMask & ~(1 << Position.A1_CASTLE));
                    SetCastleMask(castleMask & ~(1 << Position.H1_CASTLE));
                }
                else
                {
                    SetCastleMask(castleMask & ~(1 << Position.A8_CASTLE));
                    SetCastleMask(castleMask & ~(1 << Position.H8_CASTLE));
                }
            }
            if (!nullMove)
            {
                int rook = wtm ? Piece.WROOK : Piece.BROOK;
                if (p == rook)
                {
                    RemoveCastleRights(move.From);
                }
                int oRook = wtm ? Piece.BROOK : Piece.WROOK;
                if (capP == oRook)
                {
                    RemoveCastleRights(move.To);
                }
            }

            // Handle en passant and epSquare
            int prevEpSquare = epSquare;
            SetEpSquare(-1);
            if (p == Piece.WPAWN)
            {
                if (move.To - move.From == 2 * 8)
                {
                    int x = Position.GetX(move.To);
                    if (((x > 0) && (squares[move.To - 1] == Piece.BPAWN)) ||
                            ((x < 7) && (squares[move.To + 1] == Piece.BPAWN)))
                    {
                        SetEpSquare(move.From + 8);
                    }
                }
                else if (move.To == prevEpSquare)
                {
                    SetPiece(move.To - 8, Piece.EMPTY);
                }
            }
            else if (p == Piece.BPAWN)
            {
                if (move.To - move.From == -2 * 8)
                {
                    int x = Position.GetX(move.To);
                    if (((x > 0) && (squares[move.To - 1] == Piece.WPAWN)) ||
                            ((x < 7) && (squares[move.To + 1] == Piece.WPAWN)))
                    {
                        SetEpSquare(move.From - 8);
                    }
                }
                else if (move.To == prevEpSquare)
                {
                    SetPiece(move.To + 8, Piece.EMPTY);
                }
            }

            // Perform move
            SetPiece(move.From, Piece.EMPTY);
            // Handle promotion
            if (move.PromoteTo != Piece.EMPTY)
            {
                SetPiece(move.To, move.PromoteTo);
            }
            else
            {
                SetPiece(move.To, p);
            }
            SetWhiteMove(!wtm);
        }

        public void UnMakeMove(Move move, UndoInfo ui)
        {
            SetWhiteMove(!whiteMove);
            int p = squares[move.To];
            SetPiece(move.From, p);
            SetPiece(move.To, ui.capturedPiece);
            SetCastleMask(ui.castleMask);
            SetEpSquare(ui.epSquare);
            halfMoveClock = ui.halfMoveClock;
            bool wtm = whiteMove;
            if (move.PromoteTo != Piece.EMPTY)
            {
                p = wtm ? Piece.WPAWN : Piece.BPAWN;
                SetPiece(move.From, p);
            }
            if (!wtm)
            {
                fullMoveCounter--;
            }

            // Handle castling
            int king = wtm ? Piece.WKING : Piece.BKING;
            int k0 = move.From;
            if (p == king)
            {
                if (move.To == k0 + 2)
                { // O-O
                    SetPiece(k0 + 3, squares[k0 + 1]);
                    SetPiece(k0 + 1, Piece.EMPTY);
                }
                else if (move.To == k0 - 2)
                { // O-O-O
                    SetPiece(k0 - 4, squares[k0 - 1]);
                    SetPiece(k0 - 1, Piece.EMPTY);
                }
            }

            // Handle en passant
            if (move.To == epSquare)
            {
                if (p == Piece.WPAWN)
                {
                    SetPiece(move.To - 8, Piece.BPAWN);
                }
                else if (p == Piece.BPAWN)
                {
                    SetPiece(move.To + 8, Piece.WPAWN);
                }
            }
        }

        private void RemoveCastleRights(int square)
        {
            if (square == Position.GetSquare(0, 0))
            {
                SetCastleMask(castleMask & ~(1 << Position.A1_CASTLE));
            }
            else if (square == Position.GetSquare(7, 0))
            {
                SetCastleMask(castleMask & ~(1 << Position.H1_CASTLE));
            }
            else if (square == Position.GetSquare(0, 7))
            {
                SetCastleMask(castleMask & ~(1 << Position.A8_CASTLE));
            }
            else if (square == Position.GetSquare(7, 7))
            {
                SetCastleMask(castleMask & ~(1 << Position.H8_CASTLE));
            }
        }
        private long ComputeZobristHash()
        {
            long hash = 0;
            for (int sq = 0; sq < 64; sq++)
            {
                int p = squares[sq];
                hash ^= psHashKeys[p, sq];
            }
            if (whiteMove)
                hash ^= whiteHashKey;
            hash ^= castleHashKeys[castleMask];
            hash ^= epHashKeys[(epSquare >= 0) ? GetX(epSquare) + 1 : 0];
            return hash;
        }

        private long GetRandomHashVal(int rndNo)
        {
            try
            {
                var md = SHA1Managed.Create();
                byte[] input = new byte[4];
                for (int i = 0; i < 4; i++)
                    input[i] = (byte)((rndNo >> (i * 8)) & 0xff);
                byte[] digest = md.ComputeHash(input);
                long ret = 0;
                for (int i = 0; i < 8; i++)
                {
                    ret ^= ((long)digest[i]) << (i * 8);
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("SHA-1 not available");
            }
        }
    }
}

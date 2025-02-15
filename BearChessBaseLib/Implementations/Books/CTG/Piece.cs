using System;
using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    /** Constants for different piece types. */
    public static class Piece
    {
        public const int EMPTY = 0;

        public const int WKING = 1;
        public const int WQUEEN = 2;
        public const int WROOK = 3;
        public const int WBISHOP = 4;
        public const int WKNIGHT = 5;
        public const int WPAWN = 6;

        public const int BKING = 7;
        public const int BQUEEN = 8;
        public const int BROOK = 9;
        public const int BBISHOP = 10;
        public const int BKNIGHT = 11;
        public const int BPAWN = 12;

        public const int NPieceTypes = 13;

        public static bool IsWhite(int pType)
        {
            return pType < BKING;
        }
        public static int MakeWhite(int pType)
        {
            return pType < BKING ? pType : pType - (BKING - WKING);
        }
        public static int MakeBlack(int pType)
        {
            return pType >= WKING && pType <= WPAWN ? pType + (BKING - WKING) : pType;
        }
        public static int SwapColor(int pType)
        {
            if (pType == EMPTY)
                return EMPTY;
            return IsWhite(pType) ? pType + (BKING - WKING) : pType - (BKING - WKING);
        }

    }
}
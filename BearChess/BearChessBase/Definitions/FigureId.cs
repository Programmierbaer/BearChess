using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace www.SoLaNoSoft.com.BearChessBase.Definitions
{
    public static class FigureId
    {
        public const int WHITE_PAWN = 1;
        public const int WHITE_BISHOP = 2;
        public const int WHITE_KNIGHT = 3;
        public const int WHITE_ROOK = 4;
        public const int WHITE_QUEEN = 5;
        public const int WHITE_KING = 6;

        public const int BLACK_PAWN = 7;
        public const int BLACK_BISHOP = 8;
        public const int BLACK_KNIGHT = 9;
        public const int BLACK_ROOK = 10;
        public const int BLACK_QUEEN = 11;
        public const int BLACK_KING = 12;

        public const int OUTSIDE_PIECE = 100;

        public const int NO_PIECE = 0;
        public const int PAWN = 1;
        public const int BISHOP = 2;
        public const int KNIGHT = 3;
        public const int ROOK = 4;
        public const int QUEEN = 5;
        public const int KING = 6;

        public static Dictionary<int, string> FigureIdToFenCharacter = new Dictionary<int, string>()
        {
            {NO_PIECE, string.Empty},
            {OUTSIDE_PIECE, string.Empty},
            {WHITE_PAWN, FenCodes.WhitePawn},
            {WHITE_BISHOP, FenCodes.WhiteBishop},
            {WHITE_KNIGHT, FenCodes.WhiteKnight},
            {WHITE_ROOK, FenCodes.WhiteRook},
            {WHITE_QUEEN, FenCodes.WhiteQueen},
            {WHITE_KING, FenCodes.WhiteKing},
            {BLACK_PAWN, FenCodes.BlackPawn},
            {BLACK_BISHOP, FenCodes.BlackBishop},
            {BLACK_KNIGHT, FenCodes.BlackKnight},
            {BLACK_ROOK, FenCodes.BlackRook},
            {BLACK_QUEEN, FenCodes.BlackQueen},
            {BLACK_KING, FenCodes.BlackKing},
        };

        public static Dictionary<string, int> FenCharacterToFigureId = new Dictionary<string, int>()
        {
            {string.Empty, NO_PIECE},
            {FenCodes.WhitePawn, WHITE_PAWN},
            {FenCodes.WhiteBishop, WHITE_BISHOP},
            {FenCodes.WhiteKnight, WHITE_KNIGHT},
            {FenCodes.WhiteRook, WHITE_ROOK},
            {FenCodes.WhiteQueen, WHITE_QUEEN},
            {FenCodes.WhiteKing, WHITE_KING},
            {FenCodes.BlackPawn, BLACK_PAWN},
            {FenCodes.BlackBishop, BLACK_BISHOP},
            {FenCodes.BlackKnight, BLACK_KNIGHT},
            {FenCodes.BlackRook, BLACK_ROOK},
            {FenCodes.BlackQueen, BLACK_QUEEN},
            {FenCodes.BlackKing, BLACK_KING},
        };
    }
}
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

        public static readonly Dictionary<int, string> FigureIdToFenCharacter = new Dictionary<int, string>()
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

        public static readonly Dictionary<string, string> FigureGBtoDE = new Dictionary<string, string>()
                                                                         {
                                                                             { "K","K" },
                                                                             { "Q","D" },
                                                                             { "R","T" },
                                                                             { "N","S" },
                                                                             { "B","L" },
                                                                             { "","" }
                                                                         };
        public static readonly Dictionary<string, string> FigureGBtoFR = new Dictionary<string, string>()
                                                                         {
                                                                             { "K","R" },
                                                                             { "Q","D" },
                                                                             { "R","T" },
                                                                             { "N","C" },
                                                                             { "B","F" },
                                                                             { "","" }
                                                                         };
        public static readonly Dictionary<string, string> FigureGBtoIT = new Dictionary<string, string>()
                                                                         {
                                                                             { "K","R" },
                                                                             { "Q","D" },
                                                                             { "R","T" },
                                                                             { "N","C" },
                                                                             { "B","A" },
                                                                             { "","" }
                                                                         };

        public static readonly Dictionary<string, string> FigureGBtoSP = new Dictionary<string, string>()
                                                                         {
                                                                             { "K","R" },
                                                                             { "Q","D" },
                                                                             { "R","T" },
                                                                             { "N","C" },
                                                                             { "B","A" },
                                                                             { "","" }
                                                                         };

        public static readonly Dictionary<string, string> FigureGBtoDA = new Dictionary<string, string>()
                                                                         {
                                                                             { "K","K" },
                                                                             { "Q","D" },
                                                                             { "R","T" },
                                                                             { "N","S" },
                                                                             { "B","L" },
                                                                             { "","" }
                                                                         };

        public static readonly Dictionary<int, string> FigureIdToEnName = new Dictionary<int, string>()
                                                                          {
                                                                              {NO_PIECE, string.Empty},
                                                                              {OUTSIDE_PIECE, string.Empty},
                                                                              {WHITE_PAWN, "Pawn"},
                                                                              {WHITE_BISHOP, "Bishop"},
                                                                              {WHITE_KNIGHT,"Knight"},
                                                                              {WHITE_ROOK, "Rook"},
                                                                              {WHITE_QUEEN, "Queen"},
                                                                              {WHITE_KING, "King"},
                                                                              {BLACK_PAWN, "Pawn"},
                                                                              {BLACK_BISHOP, "Bishop"},
                                                                              {BLACK_KNIGHT, "Knight"},
                                                                              {BLACK_ROOK, "Rook"},
                                                                              {BLACK_QUEEN, "Queen"},
                                                                              {BLACK_KING, "King"},
                                                                          };
        public static readonly Dictionary<int, string> FigureIdToDeName = new Dictionary<int, string>()
                                                                          {
                                                                              {NO_PIECE, string.Empty},
                                                                              {OUTSIDE_PIECE, string.Empty},
                                                                              {WHITE_PAWN, "Bauer"},
                                                                              {WHITE_BISHOP, "Läufer"},
                                                                              {WHITE_KNIGHT,"Springer"},
                                                                              {WHITE_ROOK, "Turm"},
                                                                              {WHITE_QUEEN, "Dame"},
                                                                              {WHITE_KING, "König"},
                                                                              {BLACK_PAWN, "Bauer"},
                                                                              {BLACK_BISHOP, "Läufer"},
                                                                              {BLACK_KNIGHT, "Springer"},
                                                                              {BLACK_ROOK, "Turm"},
                                                                              {BLACK_QUEEN, "Dame"},
                                                                              {BLACK_KING, "König"},
                                                                          };

        public static readonly Dictionary<string, int> FenCharacterToFigureId = new Dictionary<string, int>()
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
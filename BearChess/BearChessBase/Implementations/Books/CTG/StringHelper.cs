using System;
using System.Collections.Generic;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    public static class StringHelper
    {
        public static string MoveToUCIString(Move m)
        {
            string ret = SquareToString(m.From);
            ret += SquareToString(m.To);
            switch (m.PromoteTo)
            {
                case Piece.WQUEEN:
                case Piece.BQUEEN:
                    ret += "q";
                    break;
                case Piece.WROOK:
                case Piece.BROOK:
                    ret += "r";
                    break;
                case Piece.WBISHOP:
                case Piece.BBISHOP:
                    ret += "b";
                    break;
                case Piece.WKNIGHT:
                case Piece.BKNIGHT:
                    ret += "n";
                    break;
                default:
                    break;
            }

            return ret;
        }

        public static int GetX(int square)
        {
            return square & 7;
        }

        /** Return y position (rank) corresponding to a square. */
        public static int GetY(int square)
        {
            return square >> 3;
        }

        public static string SquareToString(int square)
        {
            StringBuilder ret = new StringBuilder();
            int x = GetX(square);
            int y = GetY(square);
            ret.Append((char)(x + 'a'));
            ret.Append((char)(y + '1'));
            return ret.ToString();
        }

        public static void FixupEPSquare(Position pos)
        {
            int epSquare = pos.GetEpSquare();
            if (epSquare >= 0)
            {
                List<Move> moves = MoveGen.Instance.LegalMoves(pos);
                bool epValid = false;
                foreach (var m in moves)
                {
                    if (m.To == epSquare)
                    {
                        if (pos.GetPiece(m.From) == (pos.whiteMove ? Piece.WPAWN : Piece.BPAWN))
                        {
                            epValid = true;
                            break;
                        }
                    }
                }

                if (!epValid)
                {
                    pos.SetEpSquare(-1);
                }
            }
        }

        private static void safeSetPiece(Position pos, int col, int row, int p)
        {
            pos.SetPiece(Position.GetSquare(col, row), p);
        }

        public static void RemoveBogusCastleFlags(Position pos)
        {
            int castleMask = pos.GetCastleMask();
            int validCastle = 0;
            if (pos.GetPiece(4) == Piece.WKING)
            {
                if (pos.GetPiece(0) == Piece.WROOK) validCastle |= (1 << Position.A1_CASTLE);
                if (pos.GetPiece(7) == Piece.WROOK) validCastle |= (1 << Position.H1_CASTLE);
            }

            if (pos.GetPiece(60) == Piece.BKING)
            {
                if (pos.GetPiece(56) == Piece.BROOK) validCastle |= (1 << Position.A8_CASTLE);
                if (pos.GetPiece(63) == Piece.BROOK) validCastle |= (1 << Position.H8_CASTLE);
            }

            castleMask &= validCastle;
            pos.SetCastleMask(castleMask);
        }

        public static int GetSquare(String s)
        {
            int x = s[0] - 'a';
            int y = s[1] - '1';
            if ((x < 0) || (x > 7) || (y < 0) || (y > 7))
                return -1;
            return Position.GetSquare(x, y);
        }

        public static Position readFEN(String fen)
        {
            fen = fen.Trim();
            Position pos = new Position();
            String[] words = fen.Split(" ".ToCharArray());
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Trim();
            }

            // Piece placement
            int row = 7;
            int col = 0;
            for (int i = 0; i < words[0].Length; i++)
            {
                char c = words[0][i];
                switch (c)
                {
                    case '1':
                        col += 1;
                        break;
                    case '2':
                        col += 2;
                        break;
                    case '3':
                        col += 3;
                        break;
                    case '4':
                        col += 4;
                        break;
                    case '5':
                        col += 5;
                        break;
                    case '6':
                        col += 6;
                        break;
                    case '7':
                        col += 7;
                        break;
                    case '8':
                        col += 8;
                        break;
                    case '/':
                        row--;
                        col = 0;
                        break;
                    case 'P':
                        safeSetPiece(pos, col, row, Piece.WPAWN);
                        col++;
                        break;
                    case 'N':
                        safeSetPiece(pos, col, row, Piece.WKNIGHT);
                        col++;
                        break;
                    case 'B':
                        safeSetPiece(pos, col, row, Piece.WBISHOP);
                        col++;
                        break;
                    case 'R':
                        safeSetPiece(pos, col, row, Piece.WROOK);
                        col++;
                        break;
                    case 'Q':
                        safeSetPiece(pos, col, row, Piece.WQUEEN);
                        col++;
                        break;
                    case 'K':
                        safeSetPiece(pos, col, row, Piece.WKING);
                        col++;
                        break;
                    case 'p':
                        safeSetPiece(pos, col, row, Piece.BPAWN);
                        col++;
                        break;
                    case 'n':
                        safeSetPiece(pos, col, row, Piece.BKNIGHT);
                        col++;
                        break;
                    case 'b':
                        safeSetPiece(pos, col, row, Piece.BBISHOP);
                        col++;
                        break;
                    case 'r':
                        safeSetPiece(pos, col, row, Piece.BROOK);
                        col++;
                        break;
                    case 'q':
                        safeSetPiece(pos, col, row, Piece.BQUEEN);
                        col++;
                        break;
                    case 'k':
                        safeSetPiece(pos, col, row, Piece.BKING);
                        col++;
                        break;
                    default: throw new Exception("Invalid");
                }
            }

            if (words[1].Length > 0)
            {
                bool wtm;
                switch (words[1][0])
                {
                    case 'w':
                        wtm = true;
                        break;
                    case 'b':
                        wtm = false;
                        break;
                    default: throw new Exception("Invalid");
                }

                pos.SetWhiteMove(wtm);
            }
            else
            {
                throw new Exception("Invalid");
            }

// Castling rights
            int castleMask = 0;
            if (words.Length > 2)
            {
                for (int i = 0; i < words[2].Length; i++)
                {
                    char c = words[2][i];
                    switch (c)
                    {
                        case 'K':
                            castleMask |= (1 << Position.H1_CASTLE);
                            break;
                        case 'Q':
                            castleMask |= (1 << Position.A1_CASTLE);
                            break;
                        case 'k':
                            castleMask |= (1 << Position.H8_CASTLE);
                            break;
                        case 'q':
                            castleMask |= (1 << Position.A8_CASTLE);
                            break;
                        case '-':
                            break;
                        default: throw new Exception("Invalid");
                    }
                }
            }

            pos.SetCastleMask(castleMask);
            RemoveBogusCastleFlags(pos);

            if (words.Length > 3)
            {
                // En passant target square
                String epString = words[3];
                if (!epString.Equals("-"))
                {
                    int epSq = GetSquare(epString);
                    if (epSq != -1)
                    {
                        if (pos.whiteMove)
                        {
                            if ((Position.GetY(epSq) != 5) || (pos.GetPiece(epSq) != Piece.EMPTY) ||
                                (pos.GetPiece(epSq - 8) != Piece.BPAWN))
                                epSq = -1;
                        }
                        else
                        {
                            if ((Position.GetY(epSq) != 2) || (pos.GetPiece(epSq) != Piece.EMPTY) ||
                                (pos.GetPiece(epSq + 8) != Piece.WPAWN))
                                epSq = -1;
                        }

                        pos.SetEpSquare(epSq);
                    }
                }
            }

            try
            {
                if (words.Length > 4)
                {
                    pos.halfMoveClock = int.Parse(words[4]);
                }

                if (words.Length > 5)
                {
                    pos.fullMoveCounter = int.Parse(words[5]);
                }
            }
            catch
            {
                // Ignore errors here, since the fields are optional
            }

// Each side must have exactly one king
            int[] nPieces = new int[Piece.NPieceTypes];
            for (int i = 0; i < Piece.NPieceTypes; i++)
                nPieces[i] = 0;
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                nPieces[pos.GetPiece(Position.GetSquare(x, y))]++;


            FixupEPSquare(pos);

            return pos;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    public class CTGReader
    {

        private string _fileName;

        private readonly Dictionary<string, List<IBookMoveBase>> _fenToBookMoves =
            new Dictionary<string, List<IBookMoveBase>>();

        private byte[] _page = new byte[4096];

        private BookOptions _options = new BookOptions();
        private FileInfo _ctgFile;
        private FileInfo _ctbFile;
        private FileInfo _ctoFile;
        private static float _bigWeight = 1e6f;
        private FileStream _ctgF = null;
        private FileStream _ctbF = null;
        private FileStream _ctoF = null;
        private CtbFile _ctb = null;
        private CtoFile _cto = null;
        private CtgFile _ctg = null;


        public bool HasHeader { get; private set; }

        public int PositionsCount { get; set; }
        public int MovesCount { get; set; }
        public int GamesCount { get; set; }


        public CTGReader()
        {
            PositionsCount = 0;
            MovesCount = 0;
            GamesCount = 0;
        }

        public bool Enabled()
        {
            return _ctgFile.Exists &&
                   _ctbFile.Exists &&
                   _ctoFile.Exists;
        }


        public void ReadFile(string fileName)
        {
            _fileName = string.Empty;
            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                return;
            }

            _fileName = fileName;
            SetOptions(new BookOptions() { FileName = _fileName, MaxLength = int.MaxValue, PreferMainLines = false, Random = 0, TournamentMode = false });
            _fenToBookMoves.Clear();
        }

        private string GetAnnotation(int annotation)
        {
            switch (annotation)
            {
                default:
                case 0x00:
                    return string.Empty;
                case 0x01:
                    return "!";
                    
                case 0x02:
                    return "?";

                case 0x03:
                    return "!!";
                case 0x04:
                    return "??";
                case 0x05:
                    return "!?";
                case 0x06:
                    return "?!";
                case 0x08: // Only move
                    return string.Empty;
            }
        }

        private string GetCommentary(int commentary)
        {
            switch (commentary)
            {
                default:
                case 0x00:
                    return string.Empty;
                case 0x0b:
                    return "=";
                case 0x0d:
                    return "unclear";
                case 0x0e:
                    return "=+";
                case 0x0f:
                    return "+=";
                case 0x10:
                    return "-/+";
                case 0x11:
                    return "+/-";
                case 0x13: 
                    return "+-";
                case 0x20:
                    return "Development adv.";
                case 0x24:
                    return "Initiative";
                case 0x28:
                    return "With attack";
                case 0x2c:
                    return "Compensation";
                case 0x84:
                    return "Counterplay";
                case 0x8a:
                    return "Zeitnot";
                case 0x92:
                    return "Novelty";
            }
        }

        public IBookMoveBase[] GetMoves(string fenPosition)
        {
            if (_fenToBookMoves.TryGetValue(fenPosition, out var moves))
            {
                return moves.OrderByDescending(b => b.Recommendations).ThenByDescending(b => b.Weight).ToArray();
            }
            List<IBookMoveBase> bookMoves = new List<IBookMoveBase>();
            var position = StringHelper.readFEN(fenPosition);
            var bookPosInput = new BookPosInput(position, null, null);
            var bookEntries = getBookEntries(bookPosInput);
            if (bookEntries == null)
            {
                return bookMoves.ToArray();
            }
            
            foreach (var bookEntry in bookEntries)
            {
                var moveToUciString = StringHelper.MoveToUCIString(bookEntry.Move);

                bookMoves.Add(new CTGBookMove(moveToUciString.Substring(0, 2), moveToUciString.Substring(2, 2)
                    , bookEntry.Weight, bookEntry.NumberOfGames, bookEntry.NumberOfWinsForWhite,
                    bookEntry.NumberOfWinsForBlack, bookEntry.NumberOfDraws)
                {
                    Recommendations = bookEntry.Recommendations,
                    Annotation  = GetAnnotation(bookEntry.Annotation),
                    Commentary = GetCommentary(bookEntry.Commentary)
                });
            }
            _fenToBookMoves[fenPosition] = bookMoves;
            return bookMoves.OrderByDescending(b => b.Recommendations).ThenByDescending(b => b.Weight).ToArray();
        }

        #region private

        private class CtbFile
        {
            public int LowerPageBound { get; set; }
            public int UpperPageBound { get; set; }

            public CtbFile(FileStream f)
            {
                byte[] buffer = new byte[f.Length];
                f.Read(buffer, 0, buffer.Length);
                sbyte[] buf = ReadBytes(buffer, 4, 8);
                LowerPageBound = ExtractInt(buf, 0, 4);
                UpperPageBound = ExtractInt(buf, 4, 4);
            }
        }
        private class CtoFile
        {
            private FileStream _file;
            private byte[] _buffer;

            public CtoFile(FileStream f)
            {
                _file = f;
                _buffer = new byte[(int)f.Length];
                f.Read(_buffer, 0, (int)f.Length);
            }

            public static List<int> GetHashIndices(sbyte[] encodedPos, CtbFile ctb)
            {
                List<int> ret = new List<int>();
                int hash = GetHashValue(encodedPos);
                for (int n = 0; n < 0x7fffffff; n = 2 * n + 1)
                {
                    int c = (hash & n) + n;
                    if (c < ctb.LowerPageBound)
                        continue;
                    ret.Add(c);
                    if (c >= ctb.UpperPageBound)
                        break;
                }

                return ret;
            }

            public int GetPage(int hashIndex)
            {
                sbyte[] buf = ReadBytes(_buffer, 16 + 4 * hashIndex, 4);
                return ExtractInt(buf, 0, 4);
            }

            private static int[] tbl =
            {
                0x3100d2bf, 0x3118e3de, 0x34ab1372, 0x2807a847,
                0x1633f566, 0x2143b359, 0x26d56488, 0x3b9e6f59,
                0x37755656, 0x3089ca7b, 0x18e92d85, 0x0cd0e9d8,
                0x1a9e3b54, 0x3eaa902f, 0x0d9bfaae, 0x2f32b45b,
                0x31ed6102, 0x3d3c8398, 0x146660e3, 0x0f8d4b76,
                0x02c77a5f, 0x146c8799, 0x1c47f51f, 0x249f8f36,
                0x24772043, 0x1fbc1e4d, 0x1e86b3fa, 0x37df36a6,
                0x16ed30e4, 0x02c3148e, 0x216e5929, 0x0636b34e,
                0x317f9f56, 0x15f09d70, 0x131026fb, 0x38c784b1,
                0x29ac3305, 0x2b485dc5, 0x3c049ddc, 0x35a9fbcd,
                0x31d5373b, 0x2b246799, 0x0a2923d3, 0x08a96e9d,
                0x30031a9f, 0x08f525b5, 0x33611c06, 0x2409db98,
                0x0ca4feb2, 0x1000b71e, 0x30566e32, 0x39447d31,
                0x194e3752, 0x08233a95, 0x0f38fe36, 0x29c7cd57,
                0x0f7b3a39, 0x328e8a16, 0x1e7d1388, 0x0fba78f5,
                0x274c7e7c, 0x1e8be65c, 0x2fa0b0bb, 0x1eb6c371
            };

            private static int GetHashValue(sbyte[] encodedPos)
            {
                int hash = 0;
                int tmp = 0;
                foreach (int ch in encodedPos)
                {
                    tmp += ((0x0f - (ch & 0x0f)) << 2) + 1;
                    hash += tbl[tmp & 0x3f];
                    tmp += ((0xf0 - (ch & 0xf0)) >> 2) + 1;
                    hash += tbl[tmp & 0x3f];
                }

                return hash;
            }
        }
        private class CtgFile : IDisposable
        {
            private FileStream _ctgFileStream;
            private CtbFile _ctbFile;
            private CtoFile _ctoFile;
            private byte[] _ctgBuffer1 = null;
            private byte[] _ctgBuffer2 = null;
            public int NumberOfGames { get; private set; }

            public CtgFile(FileStream ctg, CtbFile ctb, CtoFile cto)
            {
                _ctgFileStream = ctg;
                _ctbFile = ctb;
                _ctoFile = cto;
                _ctgBuffer1 = new byte[_ctgFileStream.Length / 2];
                _ctgBuffer2 = new byte[_ctgFileStream.Length / 2];
                _ctgFileStream.Read(_ctgBuffer1, 0, _ctgBuffer1.Length);
                _ctgFileStream.Read(_ctgBuffer2, 0, _ctgBuffer2.Length);
                NumberOfGames = ExtractInt(_ctgBuffer1, 28, 4);
                
            }

            public PositionData getPositionData(Position pos)
            {
                bool mirrorColor = !pos.whiteMove;
                bool needCopy = true;
                if (mirrorColor)
                {
                    pos = mirrorPosColor(pos);
                    needCopy = false;
                }

                bool mirrorLeftRight = false;
                if ((pos.GetCastleMask() == 0) && (Position.GetX(pos.GetKingSq(true)) < 4))
                {
                    pos = mirrorPosLeftRight(pos);
                    mirrorLeftRight = true;
                    needCopy = false;
                }

                if (needCopy)
                    pos = new Position(pos);

                sbyte[] encodedPos = positionToByteArray(pos);
                List<int> hashIdxList = CtoFile.GetHashIndices(encodedPos, _ctbFile);

                PositionData pd = null;
                for (int i = 0; i < hashIdxList.Count; i++)
                {
                    int page = _ctoFile.GetPage(hashIdxList[i]);
                    if (page < 0)
                        continue;
                    pd = findInPage(page, encodedPos);
                    if (pd != null)
                    {
                        pd.pos = pos;
                        pd.mirrorColor = mirrorColor;
                        pd.mirrorLeftRight = mirrorLeftRight;
                        break;
                    }
                }
                hashIdxList.Clear();
                encodedPos = null;
                return pd;
            }

            private PositionData findInPage(int page, sbyte[] encodedPos)
            {
                byte[] pageByteBuf = new byte[4096];
                if (((page + 1) * 4096) <= _ctgBuffer1.Length)
                {
                    Array.Copy(_ctgBuffer1, (page + 1) * 4096, pageByteBuf, 0, 4096);
                }
                else
                {
                    Array.Copy(_ctgBuffer2, (page + 1) * 4096 - _ctgBuffer1.Length, pageByteBuf, 0, 4096);
                }

                //sbyte[] pageBuf = ReadBytes(_ctgFileStream, (page + 1) * 4096, 4096);
                sbyte[] pageBuf = Array.ConvertAll(pageByteBuf, b => unchecked((sbyte)b));
                // Array.ConvertAll(pageByteBuf, b => unchecked((sbyte)b));
                try
                {
                    int nPos = ExtractInt(pageBuf, 0, 2);
                    int nBytes = ExtractInt(pageBuf, 2, 2);
                    for (int i = nBytes; i < 4096; i++)
                        pageBuf[i] = 0; // Don't depend on trailing garbage
                    int offs = 4;
                    for (int p = 0; p < nPos; p++)
                    {
                        bool match = true;
                        for (int i = 0; i < encodedPos.Length; i++)
                            if (encodedPos[i] != pageBuf[offs + i])
                            {
                                match = false;
                                break;
                            }

                        if (match)
                            return new PositionData(pageBuf, offs);

                        int posLen = pageBuf[offs] & 0x1f;
                        offs += posLen;
                        int moveBytes = ExtractInt(pageBuf, offs, 1);
                        offs += moveBytes;
                        offs += PositionData.posInfoBytes;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    return null; // Ignore corrupt book file entries
                }
            }

            public void Dispose()
            {
                _ctgBuffer1 = null;
                _ctgBuffer2 = null;
            }
        }
        private class BitVector
        {
            private List<sbyte> buf = new List<sbyte>();
            public int length = 0;

            public void addBit(bool value)
            {
                int byteIdx = length / 8;
                int bitIdx = 7 - (length & 7);
                while (buf.Count <= byteIdx)
                    buf.Add((sbyte)0);
                if (value)
                    buf[byteIdx] = (sbyte)((buf[byteIdx] | (1 << bitIdx)));
                length++;
            }

            public void addBits(int mask, int numBits)
            {
                for (int i = 0; i < numBits; i++)
                {
                    int b = numBits - 1 - i;
                    addBit((mask & (1 << b)) != 0);
                }
            }

            /** Number of bits left in current byte. */
            public int padBits()
            {
                int bitIdx = length & 7;
                return (bitIdx == 0) ? 0 : 8 - bitIdx;
            }

            public sbyte[] toByteArray()
            {
                sbyte[] ret = new sbyte[buf.Count];
                for (int i = 0; i < buf.Count; i++)
                    ret[i] = buf[i];
                return ret;
            }
        }
        private class PositionData
        {
            private sbyte[] buf;
            private int posLen;
            private int moveBytes;
            public static int posInfoBytes = 3 * 4 + 4 + (3 + 4) * 2 + 1 + 1 + 1;

            public Position pos;
            public bool mirrorColor = false;
            public bool mirrorLeftRight = false;

            public PositionData(sbyte[] pageBuf, int offs)
            {
                moveInfo[0x00] = MI(Piece.WPAWN, 4, +1, +1);
                moveInfo[0x01] = MI(Piece.WKNIGHT, 1, -2, -1);
                moveInfo[0x03] = MI(Piece.WQUEEN, 1, +2, +0);
                moveInfo[0x04] = MI(Piece.WPAWN, 1, +0, +1);
                moveInfo[0x05] = MI(Piece.WQUEEN, 0, +0, +1);
                moveInfo[0x06] = MI(Piece.WPAWN, 3, -1, +1);
                moveInfo[0x08] = MI(Piece.WQUEEN, 1, +4, +0);
                moveInfo[0x09] = MI(Piece.WBISHOP, 1, +6, +6);
                moveInfo[0x0a] = MI(Piece.WKING, 0, +0, -1);
                moveInfo[0x0c] = MI(Piece.WPAWN, 0, -1, +1);
                moveInfo[0x0d] = MI(Piece.WBISHOP, 0, +3, +3);
                moveInfo[0x0e] = MI(Piece.WROOK, 1, +3, +0);
                moveInfo[0x0f] = MI(Piece.WKNIGHT, 0, -2, -1);
                moveInfo[0x12] = MI(Piece.WBISHOP, 0, +7, +7);
                moveInfo[0x13] = MI(Piece.WKING, 0, +0, +1);
                moveInfo[0x14] = MI(Piece.WPAWN, 7, +1, +1);
                moveInfo[0x15] = MI(Piece.WBISHOP, 0, +5, +5);
                moveInfo[0x18] = MI(Piece.WPAWN, 6, +0, +1);
                moveInfo[0x1a] = MI(Piece.WQUEEN, 1, +0, +6);
                moveInfo[0x1b] = MI(Piece.WBISHOP, 0, -1, +1);
                moveInfo[0x1d] = MI(Piece.WBISHOP, 1, +7, +7);
                moveInfo[0x21] = MI(Piece.WROOK, 1, +7, +0);
                moveInfo[0x22] = MI(Piece.WBISHOP, 1, -2, +2);
                moveInfo[0x23] = MI(Piece.WQUEEN, 1, +6, +6);
                moveInfo[0x24] = MI(Piece.WPAWN, 7, -1, +1);
                moveInfo[0x26] = MI(Piece.WBISHOP, 0, -7, +7);
                moveInfo[0x27] = MI(Piece.WPAWN, 2, -1, +1);
                moveInfo[0x28] = MI(Piece.WQUEEN, 0, +5, +5);
                moveInfo[0x29] = MI(Piece.WQUEEN, 0, +6, +0);
                moveInfo[0x2a] = MI(Piece.WKNIGHT, 1, +1, -2);
                moveInfo[0x2d] = MI(Piece.WPAWN, 5, +1, +1);
                moveInfo[0x2e] = MI(Piece.WBISHOP, 0, +1, +1);
                moveInfo[0x2f] = MI(Piece.WQUEEN, 0, +1, +0);
                moveInfo[0x30] = MI(Piece.WKNIGHT, 1, -1, -2);
                moveInfo[0x31] = MI(Piece.WQUEEN, 0, +3, +0);
                moveInfo[0x32] = MI(Piece.WBISHOP, 1, +5, +5);
                moveInfo[0x34] = MI(Piece.WKNIGHT, 0, +1, +2);
                moveInfo[0x36] = MI(Piece.WKNIGHT, 0, +2, +1);
                moveInfo[0x37] = MI(Piece.WQUEEN, 0, +0, +4);
                moveInfo[0x38] = MI(Piece.WQUEEN, 1, -4, +4);
                moveInfo[0x39] = MI(Piece.WQUEEN, 0, +5, +0);
                moveInfo[0x3a] = MI(Piece.WBISHOP, 0, +6, +6);
                moveInfo[0x3b] = MI(Piece.WQUEEN, 1, -5, +5);
                moveInfo[0x3c] = MI(Piece.WBISHOP, 0, -5, +5);
                moveInfo[0x41] = MI(Piece.WQUEEN, 1, +5, +5);
                moveInfo[0x42] = MI(Piece.WQUEEN, 0, -7, +7);
                moveInfo[0x44] = MI(Piece.WKING, 0, +1, -1);
                moveInfo[0x45] = MI(Piece.WQUEEN, 0, +3, +3);
                moveInfo[0x4a] = MI(Piece.WPAWN, 7, +0, +2);
                moveInfo[0x4b] = MI(Piece.WQUEEN, 0, -5, +5);
                moveInfo[0x4c] = MI(Piece.WKNIGHT, 1, +1, +2);
                moveInfo[0x4d] = MI(Piece.WQUEEN, 1, +0, +1);
                moveInfo[0x50] = MI(Piece.WROOK, 0, +0, +6);
                moveInfo[0x52] = MI(Piece.WROOK, 0, +6, +0);
                moveInfo[0x54] = MI(Piece.WBISHOP, 1, -1, +1);
                moveInfo[0x55] = MI(Piece.WPAWN, 2, +0, +1);
                moveInfo[0x5c] = MI(Piece.WPAWN, 6, +1, +1);
                moveInfo[0x5f] = MI(Piece.WPAWN, 4, +0, +2);
                moveInfo[0x61] = MI(Piece.WQUEEN, 0, +6, +6);
                moveInfo[0x62] = MI(Piece.WPAWN, 1, +0, +2);
                moveInfo[0x63] = MI(Piece.WQUEEN, 1, -7, +7);
                moveInfo[0x66] = MI(Piece.WBISHOP, 0, -3, +3);
                moveInfo[0x67] = MI(Piece.WKING, 0, +1, +1);
                moveInfo[0x69] = MI(Piece.WROOK, 1, +0, +7);
                moveInfo[0x6a] = MI(Piece.WBISHOP, 0, +4, +4);
                moveInfo[0x6b] = MI(Piece.WKING, 0, +2, +0);
                moveInfo[0x6e] = MI(Piece.WROOK, 0, +5, +0);
                moveInfo[0x6f] = MI(Piece.WQUEEN, 1, +7, +7);
                moveInfo[0x72] = MI(Piece.WBISHOP, 1, -7, +7);
                moveInfo[0x74] = MI(Piece.WQUEEN, 0, +2, +0);
                moveInfo[0x79] = MI(Piece.WBISHOP, 1, -6, +6);
                moveInfo[0x7a] = MI(Piece.WROOK, 0, +0, +3);
                moveInfo[0x7b] = MI(Piece.WROOK, 1, +0, +6);
                moveInfo[0x7c] = MI(Piece.WPAWN, 2, +1, +1);
                moveInfo[0x7d] = MI(Piece.WROOK, 1, +0, +1);
                moveInfo[0x7e] = MI(Piece.WQUEEN, 0, -3, +3);
                moveInfo[0x7f] = MI(Piece.WROOK, 0, +1, +0);
                moveInfo[0x80] = MI(Piece.WQUEEN, 0, -6, +6);
                moveInfo[0x81] = MI(Piece.WROOK, 0, +0, +1);
                moveInfo[0x82] = MI(Piece.WPAWN, 5, -1, +1);
                moveInfo[0x85] = MI(Piece.WKNIGHT, 0, -1, +2);
                moveInfo[0x86] = MI(Piece.WROOK, 0, +7, +0);
                moveInfo[0x87] = MI(Piece.WROOK, 0, +0, +5);
                moveInfo[0x8a] = MI(Piece.WKNIGHT, 0, +1, -2);
                moveInfo[0x8b] = MI(Piece.WPAWN, 0, +1, +1);
                moveInfo[0x8c] = MI(Piece.WKING, 0, -1, -1);
                moveInfo[0x8e] = MI(Piece.WQUEEN, 1, -2, +2);
                moveInfo[0x8f] = MI(Piece.WQUEEN, 0, +7, +0);
                moveInfo[0x92] = MI(Piece.WQUEEN, 1, +1, +1);
                moveInfo[0x94] = MI(Piece.WQUEEN, 0, +0, +3);
                moveInfo[0x96] = MI(Piece.WPAWN, 1, +1, +1);
                moveInfo[0x97] = MI(Piece.WKING, 0, -1, +0);
                moveInfo[0x98] = MI(Piece.WROOK, 0, +3, +0);
                moveInfo[0x99] = MI(Piece.WROOK, 0, +0, +4);
                moveInfo[0x9a] = MI(Piece.WQUEEN, 0, +0, +6);
                moveInfo[0x9b] = MI(Piece.WPAWN, 2, +0, +2);
                moveInfo[0x9d] = MI(Piece.WQUEEN, 0, +0, +2);
                moveInfo[0x9f] = MI(Piece.WBISHOP, 1, -4, +4);
                moveInfo[0xa0] = MI(Piece.WQUEEN, 1, +0, +3);
                moveInfo[0xa2] = MI(Piece.WQUEEN, 0, +2, +2);
                moveInfo[0xa3] = MI(Piece.WPAWN, 7, +0, +1);
                moveInfo[0xa5] = MI(Piece.WROOK, 1, +0, +5);
                moveInfo[0xa9] = MI(Piece.WROOK, 1, +2, +0);
                moveInfo[0xab] = MI(Piece.WQUEEN, 1, -6, +6);
                moveInfo[0xad] = MI(Piece.WROOK, 1, +4, +0);
                moveInfo[0xae] = MI(Piece.WQUEEN, 1, +3, +3);
                moveInfo[0xb0] = MI(Piece.WQUEEN, 1, +0, +4);
                moveInfo[0xb1] = MI(Piece.WPAWN, 5, +0, +2);
                moveInfo[0xb2] = MI(Piece.WBISHOP, 0, -6, +6);
                moveInfo[0xb5] = MI(Piece.WROOK, 1, +5, +0);
                moveInfo[0xb7] = MI(Piece.WQUEEN, 0, +0, +5);
                moveInfo[0xb9] = MI(Piece.WBISHOP, 1, +3, +3);
                moveInfo[0xbb] = MI(Piece.WPAWN, 4, +0, +1);
                moveInfo[0xbc] = MI(Piece.WQUEEN, 1, +5, +0);
                moveInfo[0xbd] = MI(Piece.WQUEEN, 1, +0, +2);
                moveInfo[0xbe] = MI(Piece.WKING, 0, +1, +0);
                moveInfo[0xc1] = MI(Piece.WBISHOP, 0, +2, +2);
                moveInfo[0xc2] = MI(Piece.WBISHOP, 1, +2, +2);
                moveInfo[0xc3] = MI(Piece.WBISHOP, 0, -2, +2);
                moveInfo[0xc4] = MI(Piece.WROOK, 1, +1, +0);
                moveInfo[0xc5] = MI(Piece.WROOK, 1, +0, +4);
                moveInfo[0xc6] = MI(Piece.WQUEEN, 1, +0, +5);
                moveInfo[0xc7] = MI(Piece.WPAWN, 6, -1, +1);
                moveInfo[0xc8] = MI(Piece.WPAWN, 6, +0, +2);
                moveInfo[0xc9] = MI(Piece.WQUEEN, 1, +0, +7);
                moveInfo[0xca] = MI(Piece.WBISHOP, 1, -3, +3);
                moveInfo[0xcb] = MI(Piece.WPAWN, 5, +0, +1);
                moveInfo[0xcc] = MI(Piece.WBISHOP, 1, -5, +5);
                moveInfo[0xcd] = MI(Piece.WROOK, 0, +2, +0);
                moveInfo[0xcf] = MI(Piece.WPAWN, 3, +0, +1);
                moveInfo[0xd1] = MI(Piece.WPAWN, 1, -1, +1);
                moveInfo[0xd2] = MI(Piece.WKNIGHT, 1, +2, +1);
                moveInfo[0xd3] = MI(Piece.WKNIGHT, 1, -2, +1);
                moveInfo[0xd7] = MI(Piece.WQUEEN, 0, -1, +1);
                moveInfo[0xd8] = MI(Piece.WROOK, 1, +6, +0);
                moveInfo[0xd9] = MI(Piece.WQUEEN, 0, -2, +2);
                moveInfo[0xda] = MI(Piece.WKNIGHT, 0, -1, -2);
                moveInfo[0xdb] = MI(Piece.WPAWN, 0, +0, +2);
                moveInfo[0xde] = MI(Piece.WPAWN, 4, -1, +1);
                moveInfo[0xdf] = MI(Piece.WKING, 0, -1, +1);
                moveInfo[0xe0] = MI(Piece.WKNIGHT, 1, +2, -1);
                moveInfo[0xe1] = MI(Piece.WROOK, 0, +0, +7);
                moveInfo[0xe3] = MI(Piece.WROOK, 1, +0, +3);
                moveInfo[0xe5] = MI(Piece.WQUEEN, 0, +4, +0);
                moveInfo[0xe6] = MI(Piece.WPAWN, 3, +0, +2);
                moveInfo[0xe7] = MI(Piece.WQUEEN, 0, +4, +4);
                moveInfo[0xe8] = MI(Piece.WROOK, 0, +0, +2);
                moveInfo[0xe9] = MI(Piece.WKNIGHT, 0, +2, -1);
                moveInfo[0xeb] = MI(Piece.WPAWN, 3, +1, +1);
                moveInfo[0xec] = MI(Piece.WPAWN, 0, +0, +1);
                moveInfo[0xed] = MI(Piece.WQUEEN, 0, +7, +7);
                moveInfo[0xee] = MI(Piece.WQUEEN, 1, -1, +1);
                moveInfo[0xef] = MI(Piece.WROOK, 0, +4, +0);
                moveInfo[0xf0] = MI(Piece.WQUEEN, 1, +7, +0);
                moveInfo[0xf1] = MI(Piece.WQUEEN, 0, +1, +1);
                moveInfo[0xf3] = MI(Piece.WKNIGHT, 1, -1, +2);
                moveInfo[0xf4] = MI(Piece.WROOK, 1, +0, +2);
                moveInfo[0xf5] = MI(Piece.WBISHOP, 1, +1, +1);
                moveInfo[0xf6] = MI(Piece.WKING, 0, -2, +0);
                moveInfo[0xf7] = MI(Piece.WKNIGHT, 0, -2, +1);
                moveInfo[0xf8] = MI(Piece.WQUEEN, 1, +1, +0);
                moveInfo[0xf9] = MI(Piece.WQUEEN, 1, +0, +6);
                moveInfo[0xfa] = MI(Piece.WQUEEN, 1, +3, +0);
                moveInfo[0xfb] = MI(Piece.WQUEEN, 1, +2, +2);
                moveInfo[0xfd] = MI(Piece.WQUEEN, 0, +0, +7);
                moveInfo[0xfe] = MI(Piece.WQUEEN, 1, -3, +3);
                posLen = pageBuf[offs] & 0x1f;
                moveBytes = ExtractInt(pageBuf, offs + posLen, 1);
                int bufLen = posLen + moveBytes + posInfoBytes;
                buf = new sbyte[bufLen];
                for (int i = 0; i < bufLen; i++)
                    buf[i] = pageBuf[offs + i];
            }

            public List<BookEntry> GetBookMoves(BookOptions options)
            {
                List<BookEntry> entries = new List<BookEntry>();
                int nMoves = (moveBytes - 1) / 2;
                for (int mi = 0; mi < nMoves; mi++)
                {

                    int move = ExtractInt(buf, posLen + 1 + mi * 2, 1);
                    int flags = ExtractInt(buf, posLen + 1 + mi * 2 + 1, 1);


                    Move m = decodeMove(pos, move);
                    if (m == null)
                        continue;
                    //                System.out.printf("mi:%d m:%s flags:%d\n", mi, TextIO.moveToUCIString(m), flags);
                    BookEntry ent = new BookEntry(m);
                    ent.Annotation = flags;

                    switch (flags)
                    {
                        default:
                        case 0x00:
                            ent.Weight = 1f;
                            break; // No annotation
                        case 0x01:
                            ent.Weight = 1.23f;
                            break; // !
                        case 0x02:
                            ent.Weight = 0f;
                            break; // ?
                        case 0x03:
                            ent.Weight = 1.46f;
                            break; // !!
                        case 0x04:
                            ent.Weight = 0f;
                            break; // ??
                        case 0x05:
                            ent.Weight = 1f;
                            break; // !?
                        case 0x06:
                            ent.Weight = 0f;
                            break; // ?!
                        case 0x08: // Only move
                            if (options.PreferMainLines)
                                ent.Weight = _bigWeight;
                            else
                                ent.Weight = 1.0f;
                            break;
                    }

                    entries.Add(ent);
                }

                return entries;
            }

            /** Return (loss * 2 + draws). */
            public int GetOpponentScore()
            {
                int statStart = posLen + moveBytes;
                //            int wins  = extractInt(buf, statStart + 3, 3);
                int loss = ExtractInt(buf, statStart + 6, 3);
                int draws = ExtractInt(buf, statStart + 9, 3);
                return loss * 2 + draws;
            }


            public int GetWhiteWins()
            {
                int statStart = posLen + moveBytes;
                int wins = ExtractInt(buf, statStart + 3, 3);
                int loss = ExtractInt(buf, statStart + 6, 3);
                int draws = ExtractInt(buf, statStart + 9, 3);
                return wins;
            }

            public int GetBlackWins()
            {
                int statStart = posLen + moveBytes;
                int loss = ExtractInt(buf, statStart + 6, 3);
                return loss;
            }
            public int GetDraws()
            {
                int statStart = posLen + moveBytes;
                int draws = ExtractInt(buf, statStart + 9, 3);
                return draws;
            }

            public int GetRecommendation()
            {
                int statStart = posLen + moveBytes;
                return ExtractInt(buf, statStart + 30, 1);
            }

            public int GetCommentary()
            {
                int statStart = posLen + moveBytes;
                return ExtractInt(buf, statStart + 32, 1);
            }

            private class MoveInfo
            {
                public int piece;
                public int pieceNo;
                public int dx;
                public int dy;
            }

            private static MoveInfo MI(int piece, int pieceNo, int dx, int dy)
            {
                MoveInfo mi = new MoveInfo();
                mi.piece = piece;
                mi.pieceNo = pieceNo;
                mi.dx = dx;
                mi.dy = dy;
                return mi;
            }

            private static MoveInfo[] moveInfo = new MoveInfo[256];


            private static int findPiece(Position pos, int piece, int pieceNo)
            {
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                    {
                        int sq = Position.GetSquare(x, y);
                        if (pos.GetPiece(sq) == piece)
                            if (pieceNo-- == 0)
                                return sq;
                    }

                return -1;
            }

            private Move decodeMove(Position pos, int moveCode)
            {
                MoveInfo mi = moveInfo[moveCode];
                if (mi == null)
                    return null;
                int from = findPiece(pos, mi.piece, mi.pieceNo);
                if (from < 0)
                    return null;
                int toX = (Position.GetX(from) + mi.dx) & 7;
                int toY = (Position.GetY(from) + mi.dy) & 7;
                int to = Position.GetSquare(toX, toY);
                int promoteTo = Piece.EMPTY;
                if ((pos.GetPiece(from) == Piece.WPAWN) && (toY == 7))
                    promoteTo = Piece.WQUEEN;
                return new Move(from, to, promoteTo);
            }
        }

        private void SetOptions(BookOptions options)
        {
            _options = new BookOptions(options);
            _ctgFile = new FileInfo(_options.FileName);
            _ctbFile = new FileInfo(_ctgFile.FullName.ToLower().Replace(".ctg", ".ctb"));
            _ctoFile = new FileInfo(_ctgFile.FullName.ToLower().Replace(".ctg", ".cto"));
        }

        private static sbyte[] ReadBytes(byte[] buffer, int offs, int len)
        {
            byte[] ret = new byte[len];
            Array.Copy(buffer,offs,ret,0,len);
            return Array.ConvertAll(ret, b => unchecked((sbyte)b));
        }

        private static int ExtractInt(sbyte[] buf, int offs, int len)
        {
            int ret = 0;
            for (int i = 0; i < len; i++)
            {
                int b = buf[offs + i];
                if (b < 0) b += 256;
                ret = (ret << 8) + b;
            }

            return ret;
        }

        private static int ExtractInt(byte[] buf, int offs, int len)
        {
            int ret = 0;
            for (int i = 0; i < len; i++)
            {
                int b = buf[offs + i];
                if (b < 0) b += 256;
                ret = (ret << 8) + b;
            }

            return ret;
        }

        private static Position mirrorPosLeftRight(Position pos)
        {
            Position ret = new Position(pos);
            for (int sq = 0; sq < 64; sq++)
            {
                int mSq = mirrorSquareLeftRight(sq);
                int piece = pos.GetPiece(sq);
                ret.SetPiece(mSq, piece);
            }

            int epSquare = pos.GetEpSquare();
            if (epSquare >= 0)
            {
                int mEpSquare = mirrorSquareLeftRight(epSquare);
                ret.SetEpSquare(mEpSquare);
            }

            ret.halfMoveClock = pos.halfMoveClock;
            ret.fullMoveCounter = pos.fullMoveCounter;
            return ret;
        }

        private static Position mirrorPosColor(Position pos)
        {
            Position ret = new Position(pos);
            for (int sq = 0; sq < 64; sq++)
            {
                int mSq = mirrorSquareColor(sq);
                int piece = pos.GetPiece(sq);
                int mPiece = mirrorPieceColor(piece);
                ret.SetPiece(mSq, mPiece);
            }

            ret.SetWhiteMove(!pos.whiteMove);
            int castleMask = 0;
            if (pos.A1Castle()) castleMask |= (1 << Position.A8_CASTLE);
            if (pos.H1Castle()) castleMask |= (1 << Position.H8_CASTLE);
            if (pos.A8Castle()) castleMask |= (1 << Position.A1_CASTLE);
            if (pos.H8Castle()) castleMask |= (1 << Position.H1_CASTLE);
            ret.SetCastleMask(castleMask);
            int epSquare = pos.GetEpSquare();
            if (epSquare >= 0)
            {
                int mEpSquare = mirrorSquareColor(epSquare);
                ret.SetEpSquare(mEpSquare);
            }

            ret.halfMoveClock = pos.halfMoveClock;
            ret.fullMoveCounter = pos.fullMoveCounter;
            return ret;
        }


        private static int mirrorSquareColor(int sq)
        {
            int x = Position.GetX(sq);
            int y = 7 - Position.GetY(sq);
            return Position.GetSquare(x, y);
        }

        private static int mirrorPieceColor(int piece)
        {
            if (Piece.IsWhite(piece))
            {
                piece = Piece.MakeBlack(piece);
            }
            else
            {
                piece = Piece.MakeWhite(piece);
            }

            return piece;
        }

        private static Move mirrorMoveColor(Move m)
        {
            if (m == null) return null;
            Move ret = new Move(m);
            ret.From = mirrorSquareColor(m.From);
            ret.To = mirrorSquareColor(m.To);
            ret.PromoteTo = mirrorPieceColor(m.PromoteTo);
            return ret;
        }

        private static int mirrorSquareLeftRight(int sq)
        {
            int x = 7 - Position.GetX(sq);
            int y = Position.GetY(sq);
            return Position.GetSquare(x, y);
        }

        private static Move mirrorMoveLeftRight(Move m)
        {
            if (m == null) return null;
            Move ret = new Move(m);
            ret.From = mirrorSquareLeftRight(m.From);
            ret.To = mirrorSquareLeftRight(m.To);
            ret.PromoteTo = m.PromoteTo;
            return ret;
        }

        private static sbyte[] positionToByteArray(Position pos)
        {
            BitVector bits = new BitVector();
            bits.addBits(0, 8); // Header byte
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    int p = pos.GetPiece(Position.GetSquare(x, y));
                    switch (p)
                    {
                        case Piece.EMPTY:
                            bits.addBits(0x00, 1);
                            break;
                        case Piece.WKING:
                            bits.addBits(0x20, 6);
                            break;
                        case Piece.WQUEEN:
                            bits.addBits(0x22, 6);
                            break;
                        case Piece.WROOK:
                            bits.addBits(0x16, 5);
                            break;
                        case Piece.WBISHOP:
                            bits.addBits(0x14, 5);
                            break;
                        case Piece.WKNIGHT:
                            bits.addBits(0x12, 5);
                            break;
                        case Piece.WPAWN:
                            bits.addBits(0x06, 3);
                            break;
                        case Piece.BKING:
                            bits.addBits(0x21, 6);
                            break;
                        case Piece.BQUEEN:
                            bits.addBits(0x23, 6);
                            break;
                        case Piece.BROOK:
                            bits.addBits(0x17, 5);
                            break;
                        case Piece.BBISHOP:
                            bits.addBits(0x15, 5);
                            break;
                        case Piece.BKNIGHT:
                            bits.addBits(0x13, 5);
                            break;
                        case Piece.BPAWN:
                            bits.addBits(0x07, 3);
                            break;
                    }
                }
            }

            StringHelper.FixupEPSquare(pos);
            bool ep = pos.GetEpSquare() != -1;
            bool cs = pos.GetCastleMask() != 0;
            if (!ep && !cs)
                bits.addBit(false); // At least one pad bit

            int specialBits = (ep ? 3 : 0) + (cs ? 4 : 0);
            while (bits.padBits() != specialBits)
                bits.addBit(false);

            if (ep)
                bits.addBits(Position.GetX(pos.GetEpSquare()), 3);
            if (cs)
            {
                bits.addBit(pos.H8Castle());
                bits.addBit(pos.A8Castle());
                bits.addBit(pos.H1Castle());
                bits.addBit(pos.A1Castle());
            }

            if ((bits.length & 7) != 0) throw new Exception();
            int header = bits.length / 8;
            if (ep) header |= 0x20;
            if (cs) header |= 0x40;

            sbyte[] buf = bits.toByteArray();
            buf[0] = (sbyte)header;
            return buf;
        }

        private BookEntry[] getBookEntries(BookPosInput posInput)
        {
            Position pos = posInput.getCurrPos();
            
            try
            {
                if (_ctbF == null)
                {
                    _ctgF = File.OpenRead(_ctgFile.FullName);
                    _ctbF = File.OpenRead(_ctbFile.FullName);
                    _ctoF = File.OpenRead(_ctoFile.FullName);

                    _ctb = new CtbFile(_ctbF);
                    _cto = new CtoFile(_ctoF);
                    _ctg = new CtgFile(_ctgF, _ctb, _cto);
                    GamesCount = _ctg.NumberOfGames;
                }

                List<BookEntry> ret = new List<BookEntry>();
                PositionData pd = _ctg.getPositionData(pos);
                if (pd != null)
                {
                    bool mirrorColor = pd.mirrorColor;
                    bool mirrorLeftRight = pd.mirrorLeftRight;
                    ret = pd.GetBookMoves(_options);
                    UndoInfo ui = new UndoInfo();
                    foreach (BookEntry be in ret)
                    {
                        pd.pos.MakeMove(be.Move, ui);
                        PositionData movePd = _ctg.getPositionData(pd.pos);
                        pd.pos.UnMakeMove(be.Move, ui);
                        float weight = be.Weight;
                        if (movePd == null)
                        {
                            //                        System.out.printf("%s : no pos\n", TextIO.moveToUCIString(be.move));
                            weight = 0;
                            be.Recommendations = 0;
                        }
                        else
                        {
                            int recom = movePd.GetRecommendation();
                            if (recom == 0)
                            {
                                recom = 100;
                            }
                            be.Recommendations = recom;
                            be.Commentary = movePd.GetCommentary();
                            be.NumberOfWinsForWhite = pos.whiteMove ? movePd.GetBlackWins() : movePd.GetWhiteWins();
                            be.NumberOfWinsForBlack = pos.whiteMove ? movePd.GetWhiteWins() :movePd.GetBlackWins();
                            be.NumberOfDraws = movePd.GetDraws();
                            be.NumberOfGames = be.NumberOfDraws + be.NumberOfWinsForWhite + be.NumberOfWinsForBlack;
                            if ((recom >= 64) && (recom < 128))
                            {
                                if (_options.TournamentMode)
                                    weight = 0;
                            }
                            else if (recom >= 128)
                            {
                                if (_options.PreferMainLines)
                                    weight = weight == _bigWeight ? _bigWeight : weight * _bigWeight;
                            }

                            float score = movePd.GetOpponentScore() + 1e-4f;
                            //                      float w0 = weight;
                            weight = weight * score;
                            //                      System.out.printf("bk %s : w0:%.3f rec:%d score:%d %.3f\n",
                            //                                        TextIO.moveToUCIString(be.move),
                            //                                        w0, recom, (int)score, weight);
                        }

                        be.Weight = weight;
                    }

                    if (mirrorLeftRight)
                    {
                        for (int i = 0; i < ret.Count; i++)
                            ret[i].Move = mirrorMoveLeftRight(ret[i].Move);
                    }

                    if (mirrorColor)
                    {
                        for (int i = 0; i < ret.Count; i++)
                            ret[i].Move = mirrorMoveColor(ret[i].Move);
                    }
                }
               // _ctg.Dispose();
                return ret.ToArray();
            }
            catch (Exception e)
            {
                if (_ctgF != null)
                {
                    _ctgF.Close();
                    _ctgF.Dispose();
                }
                if (_ctbF != null)
                {
                    _ctbF.Close();
                    _ctbF.Dispose();

                }
                if (_ctoF != null)
                {
                    _ctoF.Close();
                    _ctoF.Dispose();

                }
                return null;
            }
            finally
            {
                //if (ctgF != null)
                //{
                //    ctgF.Close();
                //    ctgF.Dispose();
                //}
                //if (ctbF != null)
                //{
                //    ctbF.Close();
                //    ctbF.Dispose();

                //}
                //if (ctoF != null)
                //{
                //    ctoF.Close();
                //    ctoF.Dispose();

                //}
            }
        }

        #endregion      
    }
}

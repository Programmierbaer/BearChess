using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class AbkReader
    {

        public string Author { get; private set; }
        public string Comment { get; private set; }
        public uint BookDepth { get; private set; }
        public uint BookMoves { get; private set; }

        private byte[] _book;

        private readonly string[] _fields =
        {
            "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
            "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
            "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8"
        };

        public void ReadFile(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                _book = null;
                return;
            }

            try
            {
                _book = File.ReadAllBytes(fileName);
                byte[] header = new byte[254];
                byte[] comment = new byte[header[12]];
                byte[] author = new byte[header[133]];
                byte[] fourBytes = new byte[4];
                Array.Copy(_book, 0, header, 0, 254);
                Array.Copy(header, 134, author, 0, author.Length);
                Array.Copy(header, 13, comment, 0, comment.Length);
                Author = Encoding.Default.GetString(author);
                Comment = Encoding.Default.GetString(comment);

                Array.Copy(header, 214, fourBytes, 0, fourBytes.Length);
                BookDepth = BitConverter.ToUInt32(fourBytes, 0);

                Array.Copy(header, 218, fourBytes, 0, fourBytes.Length);
                BookMoves = BitConverter.ToUInt32(fourBytes, 0);
            }
            catch
            {
                _book = null;
            }
        }

        public IBookMoveBase[] GetMoves(IBookMoveBase lastMove)
        {
            return GetMoves(((IArenaBookMove)lastMove).NextMovePointer);
        }

        public IBookMoveBase[] FirstMoves()
        {
            if (_book == null)
            {
                return null;
            }
            try
            {
                return GetMoves(900);
            }
            catch
            {
                return null;
            }
        }

        private IBookMoveBase[] GetMoves(uint nextMovePointer)
        {
            if (_book == null)
            {
                return null;
            }

            if (nextMovePointer.Equals(uint.MaxValue))
            {
                return Array.Empty<IBookMoveBase>();
            }
            List<ArenaBookMove> result = new List<ArenaBookMove>();
            byte[] moveBytes = new byte[28];
            byte[] fourBytes = new byte[4];
            Array.Copy(_book, nextMovePointer * 28, moveBytes, 0, 28);
            Array.Copy(moveBytes, 16, fourBytes, 0, 4);
            int plyCount = BitConverter.ToInt32(fourBytes, 0);

            Array.Copy(moveBytes, 20, fourBytes, 0, 4);
            nextMovePointer = BitConverter.ToUInt32(fourBytes, 0);
            Array.Copy(moveBytes, 4, fourBytes, 0, 4);
            uint noOfGames = BitConverter.ToUInt32(fourBytes, 0);
            Array.Copy(moveBytes, 8, fourBytes, 0, 4);
            uint noOfWins = BitConverter.ToUInt32(fourBytes, 0);
            Array.Copy(moveBytes, 12, fourBytes, 0, 4);
            uint noOfLoss = BitConverter.ToUInt32(fourBytes, 0);
            byte priority = moveBytes[3];
            result.Add(new ArenaBookMove(_fields[moveBytes[0]], _fields[moveBytes[1]], priority, plyCount, nextMovePointer,
                noOfGames, noOfWins, noOfLoss));

            Array.Copy(moveBytes, 24, fourBytes, 0, 4);
            uint siblingPointer = BitConverter.ToUInt32(fourBytes, 0);
            if (siblingPointer < uint.MaxValue)
            {
                Array.Copy(_book, siblingPointer * 28, moveBytes, 0, 28);
                result.AddRange(NextMoves(moveBytes));
            }

            return AdjustBookMoves(result);
        }

        private ArenaBookMove[] NextMoves(byte[] moveBytes)
        {
            byte[] fourBytes = new byte[4];
            List<ArenaBookMove> result = new List<ArenaBookMove>();
            Array.Copy(moveBytes, 16, fourBytes, 0, 4);
            int plyCount = BitConverter.ToInt32(fourBytes, 0);

            Array.Copy(moveBytes, 20, fourBytes, 0, 4);
            uint nextMovePointer = BitConverter.ToUInt32(fourBytes, 0);
            Array.Copy(moveBytes, 4, fourBytes, 0, 4);
            uint noOfGames = BitConverter.ToUInt32(fourBytes, 0);
            Array.Copy(moveBytes, 8, fourBytes, 0, 4);
            uint noOfWins = BitConverter.ToUInt32(fourBytes, 0);
            Array.Copy(moveBytes, 12, fourBytes, 0, 4);
            uint noOfLoss = BitConverter.ToUInt32(fourBytes, 0);
            byte priority = moveBytes[3];
            result.Add(new ArenaBookMove(_fields[moveBytes[0]], _fields[moveBytes[1]], priority, plyCount,nextMovePointer,noOfGames, noOfWins, noOfLoss));

            Array.Copy(moveBytes, 24, fourBytes, 0, 4);
            uint siblingPointer = BitConverter.ToUInt32(fourBytes, 0);
            if (siblingPointer < uint.MaxValue)
            {
                Array.Copy(_book, siblingPointer * 28, moveBytes, 0, 28);
                result.AddRange(NextMoves(moveBytes));
            }
            return result.ToArray();
        }

        private IBookMoveBase[] AdjustBookMoves(IReadOnlyCollection<IArenaBookMove> bookMoves)
        {
            uint max = bookMoves.Max(m => m.NoOfGames);
            foreach (IArenaBookMove bookMove in bookMoves.Where(b => b.Weight>0))
            {
                decimal factor =  (decimal) bookMove.NoOfGames / (decimal) max;
                if (factor <= (decimal)0.01 && factor > (decimal)0.001)
                {
                    factor = (decimal) 0.05;
                }
                bookMove.Weight = (uint)( (decimal) bookMove.Weight *  factor);
            }
            return bookMoves.OrderByDescending(b => b.Weight).ToArray();
        }
    }
}

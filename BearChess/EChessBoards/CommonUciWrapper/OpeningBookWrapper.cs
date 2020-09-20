using System;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public class OpeningBookWrapper
    {

        private OpeningBook _openingBook;

        private readonly ILogging _logger;

        public bool AcceptFenPosition { get; private set; }

        public class Move
        {
            public string FromField { get; }
            public string ToField { get; }
            public uint Weight { get; }
            public int PlyCount { get; }
            public uint NextMovePointer { get; }
            public string FenPosition { get; set; }

            public Move(string fromField, string toField, uint weight)
            {
                FromField = fromField;
                ToField = toField;
                Weight = weight;
                PlyCount = 0;
                NextMovePointer = 0;
            }

            public Move(string fromField, string toField, uint weight, int plyCount, uint nextMovePointer)
            {
                FromField = fromField;
                ToField = toField;
                Weight = weight;
                PlyCount = plyCount;
                NextMovePointer = nextMovePointer;
            }
        }

        public OpeningBookWrapper(string fileName, ILogging logger = null)
        {
            _logger = logger;
            _openingBook = new www.SoLaNoSoft.com.BearChessBase.Implementations.OpeningBook();
            _openingBook.LoadBook(fileName, true);
            AcceptFenPosition = _openingBook.AcceptFenPosition;
        }

        public void SetVariation(string variation)
        {
            _openingBook.SetVariation(variation);

        }

        public void LoadFile(string fileName)
        {
            _openingBook.LoadBook(fileName, true);
            AcceptFenPosition = _openingBook.AcceptFenPosition;

        }



        public Move GetMove(Move previousMove)
        {
            var bookMove = _openingBook.GetMove(new BookMove(previousMove.FromField, previousMove.ToField, (ushort)previousMove.Weight, previousMove.PlyCount, previousMove.NextMovePointer, 0, 0, 0));

            return new Move(bookMove.FromField, bookMove.ToField, bookMove.Weight, bookMove.PlyCount, bookMove.NextMovePointer);


        }



        public Move GetMoveByMoveList(string allMoves)
        {
            var allMovesArray = allMoves.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var bookMove = _openingBook.GetMove(allMovesArray);
            return new Move(bookMove.FromField, bookMove.ToField, bookMove.Weight, bookMove.PlyCount, bookMove.NextMovePointer);



        }

        public Move GetMoveByFen(string fenPosition)
        {
            var bookMove = _openingBook.GetMove(fenPosition);
            return new Move(bookMove.FromField, bookMove.ToField, bookMove.Weight, bookMove.PlyCount, bookMove.NextMovePointer);

        }
    }
}

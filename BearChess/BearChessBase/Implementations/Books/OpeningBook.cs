using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class OpeningBook
    {

        private enum VariationsEnum
        {
            BestMove = 0,
            Flexible = 1,
            Wide = 2
        }

        private AbkReader _abkReader;
        private PolyglotReader _polyglotReader;
        private VariationsEnum _variation = VariationsEnum.Flexible;

        private readonly BookMove _emptyMove = new BookMove(string.Empty, string.Empty, 0);
        private static readonly Random LineRandomness = new Random();

        public string FileName { get; private set; }

        public bool Available { get; private set; }

        public bool AcceptFenPosition { get; private set; }

        public int PositionsCount { get; private set; }
        public int MovesCount { get; private set; }

        public OpeningBook()
        {
        }

        public bool LoadBook(string fileName, bool checkFile)
        {
            Available = false;
            if (!File.Exists(fileName))
            {
                Available = false;
                _abkReader = null;
                _polyglotReader = null;
                return false;
            }
            FileName = fileName;
            if (FileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                AcceptFenPosition = true;
                _abkReader = null;
                Available = LoadPolyglot(checkFile);
            }

            if (FileName.EndsWith(".abk", StringComparison.OrdinalIgnoreCase))
            {
                AcceptFenPosition = false;
                _polyglotReader = null;
                Available = LoadArena();
            }

            return Available;
        }

        public void SetVariation(string variation)
        {
            switch (variation)
            {
                case "best move":
                    _variation = VariationsEnum.BestMove;
                    break;
                case "flexible":
                    _variation = VariationsEnum.Flexible;
                    break;
                case "wide":
                    _variation = VariationsEnum.Wide;
                    break;
                default:
                    _variation = VariationsEnum.Flexible;
                    break;
            }
        }

        public BookMove GetMove(BookMove previousMove)
        {
            if (!Available)
            {
                return _emptyMove;
            }
            if (string.IsNullOrWhiteSpace(previousMove.FromField))
            {
                return GetMove(_abkReader != null ? _abkReader.FirstMoves() : _polyglotReader.GetMoves(FenCodes.BasePosition));
            }

            return GetMove(_abkReader != null ? _abkReader.GetMoves(previousMove) : _polyglotReader.GetMoves(previousMove.FenPosition));
        }

        public BookMove GetMove(string fenPosition)
        {
            if (!Available)
            {
                return _emptyMove;
            }
            if (string.IsNullOrWhiteSpace(fenPosition))
            {
                return GetMove(_emptyMove);
            }
            return GetMove(_polyglotReader != null ? _polyglotReader.GetMoves(fenPosition) : new BookMove[0]);
        }

        public BookMove GetMove(string[] moveList)
        {
            if (!Available)
            {
                return _emptyMove;
            }
            if (moveList.Length == 0)
            {
                return GetMove(_emptyMove);
            }

            return GetMove(GetMoveList(moveList));
        }

        public BookMove[] GetMoveList(string[] moveList)
        {
            if (!Available)
            {
                return new BookMove[0];
            }
           
            var bookMoves = GetMoveList(_emptyMove);
            foreach (var move in moveList)
            {
                if (move.StartsWith("position"))
                {
                    continue;
                }
                BookMove foundMove = null;
                foreach (var bookMove in bookMoves)
                {
                    if ($"{bookMove.FromField}{bookMove.ToField}".Equals(move))
                    {
                        foundMove = bookMove;
                        break;
                    }
                }

                if (foundMove != null)
                {
                    bookMoves = GetMoveList(foundMove);
                }
                else
                {
                    return new BookMove[0];
                }
            }

            return bookMoves;
        }

        public BookMove[] GetMoveList(BookMove previousMove)
        {
            if (string.IsNullOrWhiteSpace(previousMove.FromField))
            {
                return _abkReader != null ? _abkReader.FirstMoves() : _polyglotReader.GetMoves(FenCodes.BasePosition);
            }
            return _abkReader != null
                ? _abkReader.GetMoves(previousMove)
                : _polyglotReader.GetMoves(previousMove.FenPosition);
        }

        public BookMove[] GetMoveList(string fenPosition)
        {
            return _polyglotReader != null
                ? _polyglotReader.GetMoves(fenPosition)
                : new BookMove[0];
        }

        public BookMove[] GetMoveList()
        {
            return GetMoveList(_emptyMove);
        }

        #region private

        private BookMove GetMove(BookMove[] moves)
        {
            if (moves.Length == 0)
            {
                return _emptyMove;
            }
            List<BookMove> candidates = new List<BookMove>();
            if (_variation == VariationsEnum.BestMove || moves.Length==1)
            {
                return moves[0];
            }
            long sumWeights = moves.Sum(w => w.Weight);
            foreach (var bookMove in moves)
            {
                long moveWeight = 100 * bookMove.Weight / sumWeights;
                if (_variation == VariationsEnum.Flexible && moveWeight <= 5)
                {
                    continue;
                }
                for (int i = 0; i < moveWeight; i++)
                {
                    candidates.Add(bookMove);
                }
            }

            if (candidates.Count == 0)
            {
                return moves[0];
            }
            BookMove move =
                candidates.ToArray().OrderBy(m => (LineRandomness.Next(2) % 2) == 0)
                          .ToArray()[LineRandomness.Next(0, candidates.Count)];
            return move;
        }

      

        private bool LoadArena()
        {
            _abkReader = new AbkReader();
           _abkReader.ReadFile(FileName);
            var bookMoves = _abkReader.FirstMoves();
            return bookMoves != null && bookMoves.Length != 0;
        }

        private bool LoadPolyglot(bool checkFile)
        {
            _polyglotReader = new PolyglotReader();
            _polyglotReader.ReadFile(FileName);
            if (checkFile)
            {
                var bookMoves = _polyglotReader.GetMoves(FenCodes.BasePosition);
                PositionsCount = _polyglotReader.PositionsCount;
                MovesCount = _polyglotReader.MovesCount;
                return bookMoves != null && bookMoves.Length != 0;
            }

            return true;
        }
        
        #endregion
    }
}

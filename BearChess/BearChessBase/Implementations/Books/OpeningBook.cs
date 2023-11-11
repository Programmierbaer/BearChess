using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class OpeningBook
    {

        public enum VariationsEnum
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

            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            var bookMoves = GetMoveList(_emptyMove, false);
            foreach (var move in moveList)
            {
                if (move.StartsWith("position"))
                {
                    continue;
                }

                var fromField = move.Substring(0, 2).ToLower();
                var toField = move.Substring(2, 2).ToLower();
              
                chessBoard.MakeMove(fromField, toField);

                BookMove foundMove = null;
                foreach (var bookMove in bookMoves)
                {

                    if ($"{bookMove.FromField}{bookMove.ToField}".Equals(move))
                    {
                        foundMove = bookMove;
                        foundMove.FenPosition = chessBoard.GetFenPosition();
                        break;
                    }
                }

                if (foundMove != null)
                {
                    bookMoves = GetMoveList(foundMove, true);
                }
                else
                {
                    return new BookMove[0];
                }
            }

            return bookMoves;
        }

        public BookMove[] GetMoveList(BookMove previousMove, bool checkCastle)
        {
            if (string.IsNullOrWhiteSpace(previousMove.FromField))
            {
                return _abkReader != null ? _abkReader.FirstMoves() : _polyglotReader.GetMoves(FenCodes.BasePosition);
            }

            var bookMoves = _abkReader != null
                                ? _abkReader.GetMoves(previousMove)
                                : _polyglotReader.GetMoves(previousMove.FenPosition);
            if (checkCastle)
            {
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();
                chessBoard.SetPosition(previousMove.FenPosition);
                foreach (var bookMove in bookMoves)
                {
                    if (bookMove.FromField.Equals("e1") && bookMove.ToField.Equals("h1"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE1);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "g1";
                        }
                    }

                    if (bookMove.FromField.Equals("e1") && bookMove.ToField.Equals("a1"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE1);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "c1";
                        }
                    }

                    if (bookMove.FromField.Equals("e8") && bookMove.ToField.Equals("h8"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE8);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "g8";
                        }
                    }

                    if (bookMove.FromField.Equals("e8") && bookMove.ToField.Equals("a8"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE8);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "c8";
                        }
                    }

                }
            }

            return bookMoves;
        }

        public BookMove[] GetMoveList(string fenPosition, bool checkCastle)
        {
            var bookMoves = 
            _polyglotReader != null
                ? _polyglotReader.GetMoves(fenPosition)
                : new BookMove[0];
            if (checkCastle)
            {
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();
                chessBoard.SetPosition(fenPosition);
                foreach (var bookMove in bookMoves)
                {
                    if (bookMove.FromField.Equals("e1") && bookMove.ToField.Equals("h1"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE1);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "g1";
                        }
                    }

                    if (bookMove.FromField.Equals("e1") && bookMove.ToField.Equals("a1"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE1);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "c1";
                        }
                    }

                    if (bookMove.FromField.Equals("e8") && bookMove.ToField.Equals("h8"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE8);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "g8";
                        }
                    }

                    if (bookMove.FromField.Equals("e8") && bookMove.ToField.Equals("a8"))
                    {
                        var chessFigure = chessBoard.GetFigureOn(Fields.FE8);
                        if (chessFigure.GeneralFigureId == FigureId.KING)
                        {
                            bookMove.ToField = "c8";
                        }
                    }

                }
            }

            return bookMoves;
        }

        public BookMove[] GetMoveList()
        {
            return GetMoveList(_emptyMove, false);
        }


        public BookMove[] GetCandidateMoveList()
        {
            return GetCandidates(GetMoveList());
        }

        #region private

        private BookMove[] GetCandidates(BookMove[] moves)
        {
            List<BookMove> candidates = new List<BookMove>();
            if (_variation == VariationsEnum.BestMove || moves.Length == 1)
            {
                return new BookMove[] { moves[0]};
            }
            long sumWeights = moves.Sum(w => w.Weight);
            if (sumWeights == 0)
            {
                sumWeights = 1;
            }
            foreach (var bookMove in moves)
            {
                if (_variation == VariationsEnum.Wide)
                {
                    candidates.Add(bookMove);
                    continue;

                }
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
                return moves;
            }

            return candidates.ToArray();
        }


        private BookMove GetMove(BookMove[] moves)
        {
            if (moves.Length == 0)
            {
                return _emptyMove;
            }

            var bookMoves = GetCandidates(moves);
            BookMove move =
                bookMoves.ToArray().OrderBy(m => (LineRandomness.Next(2) % 2) == 0)
                          .ToArray()[LineRandomness.Next(0, bookMoves.Length)];
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

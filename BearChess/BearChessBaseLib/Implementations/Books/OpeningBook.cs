using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class OpeningBook
    {
        private  DisplayFigureType _displayFigureType = DisplayFigureType.Letter;
        private  DisplayMoveType _displayMoveType =DisplayMoveType.FromToField;
        private  DisplayCountryType _displayCountryType=DisplayCountryType.GB;

        public enum VariationsEnum
        {
            BestMove = 0,
            Flexible = 1,
            Wide = 2
        }

        private AbkReader _abkReader;
        private PolyglotReader _polyglotReader;
        private CTGReader _ctgReader;
        private VariationsEnum _variation = VariationsEnum.Flexible;

        private readonly IBookMoveBase _emptyMove = new PolyglotBookMove(string.Empty, string.Empty, 0);
        private static readonly Random LineRandomness = new Random();


        public string FileName { get; private set; }

        public bool Available { get; private set; }

        public bool AcceptFenPosition { get; private set; }

        public int PositionsCount { get; private set; }
        public int MovesCount { get; private set; }
        public int GamesCount { get; private set; }

        public OpeningBook()
        {
            
        }
    
        public OpeningBook(DisplayFigureType displayFigureType, DisplayMoveType displayMoveType, DisplayCountryType displayCountryType)
        {
            _displayFigureType = displayFigureType;
            _displayMoveType = displayMoveType;
            _displayCountryType = displayCountryType;
        }

        public bool LoadBook(string fileName, bool checkFile)
        {
            Available = false;
            if (!File.Exists(fileName))
            {
                Available = false;
                _abkReader = null;
                _polyglotReader = null;
                _ctgReader = null;
                return false;
            }
            FileName = fileName;
            if (FileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                AcceptFenPosition = true;
                _abkReader = null;
                _ctgReader = null;
                Available = LoadPolyglot(checkFile);
            }

            if (FileName.EndsWith(".abk", StringComparison.OrdinalIgnoreCase))
            {
                AcceptFenPosition = false;
                _polyglotReader = null;
                _ctgReader = null;
                Available = LoadArena();
            }
            if (FileName.EndsWith(".ctg", StringComparison.OrdinalIgnoreCase))
            {
                AcceptFenPosition = true;
                _polyglotReader = null;
                _abkReader = null;
                Available = LoadCTG(checkFile);
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

        public IBookMoveBase GetMove(IBookMoveBase previousMove)
        {
            if (!Available)
            {
                return _emptyMove;
            }
            if (string.IsNullOrWhiteSpace(previousMove.FromField))
            {
                if (_abkReader!=null)
                {
                    return GetMove(_abkReader.FirstMoves());
                }
                if (_polyglotReader != null)
                {
                    return GetMove(FenCodes.BasePosition);
                }
                return GetMove(_ctgReader.GetMoves(FenCodes.BasePosition));
            }
            if (_abkReader != null)
            {
                return GetMove((IArenaBookMove)previousMove);
            }
            if (_polyglotReader != null)
            {
                return GetMove(previousMove.FenPosition);
            }
            return GetMove(_ctgReader.GetMoves(previousMove.FenPosition));

        }

        public IBookMoveBase GetMove(string fenPosition)
        {
            if (!Available)
            {
                return _emptyMove;
            }
            if (string.IsNullOrWhiteSpace(fenPosition))
            {
                return GetMove((IArenaBookMove)_emptyMove);
            }
            if (_polyglotReader != null)
            {
                return GetMove(_polyglotReader.GetMoves(fenPosition));
            }
            return GetMove(_ctgReader.GetMoves(fenPosition));            
        }

        public IBookMoveBase GetMove(string[] moveList)
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

        public IBookMoveBase[] GetMoveList(string[] moveList)
        {
            if (!Available)
            {
                return Array.Empty<IBookMoveBase>();
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

                IBookMoveBase foundMove = null;
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
                    return Array.Empty<IBookMoveBase>();
                }
            }

            return bookMoves;
        }

        public IBookMoveBase[] GetMoveList(IBookMoveBase previousMove, bool checkCastle)
        {
            if (string.IsNullOrWhiteSpace(previousMove.FromField))
            {
                if (_abkReader != null)
                {
                    return _abkReader.FirstMoves();
                }
                if (_polyglotReader != null)
                {
                    return _polyglotReader.GetMoves(FenCodes.BasePosition);
                }
                return _ctgReader.GetMoves(FenCodes.BasePosition);
            }

            IBookMoveBase[] bookMoves = null;
            if (_abkReader != null)
                bookMoves = _abkReader.GetMoves(previousMove);
            if (_polyglotReader != null)
                bookMoves = _polyglotReader.GetMoves(previousMove.FenPosition);
            if (_ctgReader != null)
                bookMoves = _ctgReader.GetMoves(previousMove.FenPosition);
            if (bookMoves == null)
                bookMoves = Array.Empty<IBookMoveBase>();
            // if (checkCastle)
            {
                var fastChessBoard = new FastChessBoard();
                fastChessBoard.SetDisplayTypes(_displayFigureType,_displayMoveType,_displayCountryType);
                fastChessBoard.Init(previousMove.FenPosition, Array.Empty<string>());
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();
                chessBoard.SetPosition(previousMove.FenPosition);
                foreach (var bookMove in bookMoves)
                {
                    bookMove.MoveText = fastChessBoard.GetMoveString($"{bookMove.FromField}{bookMove.ToField}");
                    bookMove.MoveText += $"{bookMove.Annotation} ";
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

        public IBookMoveBase[] GetMoveList(string fenPosition, bool checkCastle)
        {
            IBookMoveBase[] bookMoves = null;
            if (_polyglotReader != null)
                bookMoves = _polyglotReader.GetMoves(fenPosition);
            if (_ctgReader != null)
                bookMoves = _ctgReader.GetMoves(fenPosition);
            if (bookMoves==null)
                bookMoves = Array.Empty<IBookMoveBase>();
            if (bookMoves.Length>0)
            {
                var fastChessBoard = new FastChessBoard();
                fastChessBoard.SetDisplayTypes(_displayFigureType, _displayMoveType, _displayCountryType);
                var chessBoard = new ChessBoard();
                fastChessBoard.Init(fenPosition,Array.Empty<string>());
                chessBoard.NewGame();
                chessBoard.SetPosition(fenPosition);
                foreach (var bookMove in bookMoves)
                {
                    bookMove.MoveText = fastChessBoard.GetMoveString($"{bookMove.FromField}{bookMove.ToField}");

                    bookMove.MoveText += $"{bookMove.Annotation} ";
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

        public IBookMoveBase[] GetMoveList()
        {
            return GetMoveList(_emptyMove, false);
        }


        public IBookMoveBase[] GetCandidateMoveList()
        {
            return GetCandidates(GetMoveList());
        }

        #region private

        private IBookMoveBase[] GetCandidates(IBookMoveBase[] moves)
        {
            List<IBookMoveBase> candidates = new List<IBookMoveBase>();
            if (_variation == VariationsEnum.BestMove || moves.Length == 1)
            {
                return new IBookMoveBase[] { moves[0]};
            }
            float sumWeights = moves.Sum(w => w.Weight);
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
                float moveWeight = 100 * bookMove.Weight / sumWeights;
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


        private IBookMoveBase GetMove(IBookMoveBase[] moves)
        {
            if (moves.Length == 0)
            {
                return _emptyMove;
            }

            var bookMoves = GetCandidates(moves);
            IBookMoveBase move =
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

        private bool LoadCTG(bool checkFile)
        {
            _ctgReader = new CTGReader();
            _ctgReader.ReadFile(FileName);
            if (checkFile)
            {
                var bookMoves = _ctgReader.GetMoves(FenCodes.BasePosition);
                PositionsCount = _ctgReader.PositionsCount;
                MovesCount = _ctgReader.MovesCount;
                GamesCount = _ctgReader.GamesCount;
                return bookMoves != null && bookMoves.Length != 0;
            }

            return true;
        }

        #endregion

        public void SetDisplayTypes(DisplayFigureType displayFigureType, DisplayMoveType displayMoveType, DisplayCountryType displayCountryType)
        {
            _displayFigureType = displayFigureType;
            _displayMoveType = displayMoveType;
            _displayCountryType = displayCountryType;
        }
    }
}

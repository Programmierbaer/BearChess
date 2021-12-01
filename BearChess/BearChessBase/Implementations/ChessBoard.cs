using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class ChessBoard : IChessBoard
    {

        private readonly Castling[] _canCastling = new Castling[2];
        private readonly bool[] _castled = { false, false };
        private readonly int[] _kingPosition = { 0, 0 };
        private readonly int[] _material = { 0, 0 };
        private int _currentColor;
        private IChessFigure[] _figures = new IChessFigure[Fields.MAX_FIELD];
        private int _lastMoveFromField;
        private IChessFigure _lastMoveFromFigure;
        private int _lastMoveToField;
        private IChessFigure _lastMoveToFigure;
        private readonly OutsideFigure _outsideFigure;
        private readonly Dictionary<int, AllMoveClass> _allPlayedMoves;
        private readonly Dictionary<string, int> _repetition;
        private string _initialFenPosition;
        private bool _analyzeMode;
        public bool DrawByRepetition { get; private set; }
        public bool DrawByMaterial { get; private set; }
        private FileLogger _fileLogger;


        public ChessBoard()
        {
            _outsideFigure = new OutsideFigure(this, Fields.OutsideFields[0]);
            CurrentFigureList = new HashSet<int>();
            CurrentMoveList = new List<Move>(0);
            EnemyMoveList = new List<Move>(0);
            CapturedFigure = _outsideFigure;
            _allPlayedMoves = new Dictionary<int, AllMoveClass>();
            _repetition = new Dictionary<string, int>();
            _initialFenPosition = string.Empty;
        }


        /// <inheritdoc />
        public HashSet<int> CurrentFigureList { get; private set; }

        /// <inheritdoc />
        public int CurrentColor
        {
            get => _currentColor;
            set
            {
                _currentColor = value;
                EnemyColor = _currentColor == Fields.COLOR_WHITE ? Fields.COLOR_BLACK : Fields.COLOR_WHITE;
            }
        }

        /// <inheritdoc />
        public int EnemyColor { get; private set; }
        
        /// <inheritdoc />
        public List<Move> CurrentMoveList { get; private set; }

        public Move[] GetPlayedMoveList()
        {
            List<Move> result = new List<Move>();
            foreach (var key in _allPlayedMoves.Keys.OrderBy(k => k))
            {
                result.Add(_allPlayedMoves[key].GetMove(Fields.COLOR_WHITE));
                var move = _allPlayedMoves[key].GetMove(Fields.COLOR_BLACK);
                if (move != null)
                {
                    result.Add(move);
                }
            }

            return result.ToArray();
        }

        public Move GetPlayedMove(int moveNumber, int color)
        {
            if (_allPlayedMoves.ContainsKey(moveNumber))
            {
                return _allPlayedMoves[moveNumber].GetMove(color);
            }

            return null;
        }

        public string GetPlayedFenPosition(int moveNumber, int color)
        {
            if (_allPlayedMoves.ContainsKey(moveNumber))
            {
                return _allPlayedMoves[moveNumber].GetFen(color);
            }

            return null;
        }

        /// <inheritdoc />
        public List<Move> EnemyMoveList { get; private set; }

        public void SetGameAnalyzeMode(bool analyzeMode)
        {
            _analyzeMode = analyzeMode;
//            _fileLogger = new FileLogger(Path.Combine(@"d:\", "chessboard.log"), 10, 10);
        }

        /// <inheritdoc />
        public string EnPassantTargetField { get; set; }

        /// <inheritdoc />
        public int EnPassantTargetFieldNumber { get; set; }

        /// <inheritdoc />
        public int HalfMoveClock { get; set; }

        /// <inheritdoc />
        public int FullMoveNumber { get; set; }

        /// <inheritdoc />
        public IChessFigure CapturedFigure { get; private set; }

        /// <inheritdoc />
        public IChessFigure GetFigureOn(int field)
        {
            return _figures[field];
        }

        /// <inheritdoc />
        public IChessFigure[] GetFiguresOnRow(int row)
        {
            return Fields.RowFields(row).Select(rowField => _figures[rowField]).ToArray();
        }

        /// <inheritdoc />
        public IChessFigure[] GetFiguresOnLine(Fields.Lines line)
        {
            return Fields.LineFields(line).Select(lineField => _figures[lineField]).ToArray();
        }

        public IChessFigure[] GetFigures(int color)
        {
            return _figures.Where(f => f.Color == color).ToArray();
        }

        /// <inheritdoc />
        public IChessFigure GetKingFigure(int color)
        {
            return GetFigureOn(_kingPosition[color]);
        }

        /// <inheritdoc />
        public int GetMaterialFor(int color)
        {
            return _material[color];
        }

        /// <inheritdoc />
        public bool Castled(int color)
        {
            return _castled[color];
        }

        /// <inheritdoc />
        public bool CanCastling(int color, CastlingEnum castlingEnum)
        {
            return castlingEnum == CastlingEnum.Short
                       ? _canCastling[color].ShortCastling
                       : _canCastling[color].LongCastling;
        }

        /// <inheritdoc />
        public void SetCanCastling(int color, CastlingEnum castlingEnum, bool canCastling)
        {
            if (castlingEnum == CastlingEnum.Short)
            {
                if (color == Fields.COLOR_WHITE)
                {
                    _canCastling[color].ShortCastling = canCastling && GetFigureOn(Fields.FE1).FigureId == FigureId.WHITE_KING && GetFigureOn(Fields.FH1).FigureId == FigureId.WHITE_ROOK;
                }
                else
                {
                    _canCastling[color].ShortCastling = canCastling && GetFigureOn(Fields.FE8).FigureId == FigureId.BLACK_KING && GetFigureOn(Fields.FH8).FigureId == FigureId.BLACK_ROOK;
                }
            }
            if (castlingEnum == CastlingEnum.Long)
            {
                if (color == Fields.COLOR_WHITE)
                {
                    _canCastling[color].LongCastling = canCastling && GetFigureOn(Fields.FE1).FigureId == FigureId.WHITE_KING && GetFigureOn(Fields.FA1).FigureId == FigureId.WHITE_ROOK;
                }
                else
                {
                    _canCastling[color].LongCastling = canCastling && GetFigureOn(Fields.FE8).FigureId == FigureId.BLACK_KING && GetFigureOn(Fields.FA8).FigureId == FigureId.BLACK_ROOK;
                }
            }
        }

        /// <inheritdoc />
        public void NewGame()
        {
            _allPlayedMoves.Clear();
            _repetition.Clear();
            DrawByRepetition = false;
            DrawByMaterial = false;
            _initialFenPosition = string.Empty;

            CurrentColor = Fields.COLOR_WHITE;

            _canCastling[Fields.COLOR_WHITE].ShortCastling = true;
            _canCastling[Fields.COLOR_WHITE].LongCastling = true;
            _canCastling[Fields.COLOR_BLACK].ShortCastling = true;
            _canCastling[Fields.COLOR_BLACK].LongCastling = true;
            _castled[Fields.COLOR_WHITE] = false;
            _castled[Fields.COLOR_BLACK] = false;
            _kingPosition[Fields.COLOR_WHITE] = Fields.FE1;
            _kingPosition[Fields.COLOR_BLACK] = Fields.FE8;

            EnPassantTargetField = "-";
            EnPassantTargetFieldNumber = -1;
            HalfMoveClock = 0;
            FullMoveNumber = 1;

            _figures[Fields.FA1] = new WhiteRookFigure(this, Fields.FA1);
            _figures[Fields.FH1] = new WhiteRookFigure(this, Fields.FH1);
            _figures[Fields.FB1] = new WhiteKnightFigure(this, Fields.FB1);
            _figures[Fields.FG1] = new WhiteKnightFigure(this, Fields.FG1);
            _figures[Fields.FC1] = new WhiteBishopFigure(this, Fields.FC1);
            _figures[Fields.FF1] = new WhiteBishopFigure(this, Fields.FF1);
            _figures[Fields.FD1] = new WhiteQueenFigure(this, Fields.FD1);
            _figures[Fields.FE1] = new WhiteKingFigure(this, Fields.FE1);
            for (var i = Fields.FA2; i <= Fields.FH2; i++)
            {
                _figures[i] = new WhitePawnFigure(this, i);
            }

            _figures[Fields.FA8] = new BlackRookFigure(this, Fields.FA8);
            _figures[Fields.FH8] = new BlackRookFigure(this, Fields.FH8);
            _figures[Fields.FB8] = new BlackKnightFigure(this, Fields.FB8);
            _figures[Fields.FG8] = new BlackKnightFigure(this, Fields.FG8);
            _figures[Fields.FC8] = new BlackBishopFigure(this, Fields.FC8);
            _figures[Fields.FF8] = new BlackBishopFigure(this, Fields.FF8);
            _figures[Fields.FD8] = new BlackQueenFigure(this, Fields.FD8);
            _figures[Fields.FE8] = new BlackKingFigure(this, Fields.FE8);
            for (var i = Fields.FA7; i <= Fields.FH7; i++)
            {
                _figures[i] = new BlackPawnFigure(this, i);
            }

            _figures[Fields.FA3] = new NoFigure(this, Fields.FA3);
            _figures[Fields.FB3] = new NoFigure(this, Fields.FB3);
            _figures[Fields.FC3] = new NoFigure(this, Fields.FC3);
            _figures[Fields.FD3] = new NoFigure(this, Fields.FD3);
            _figures[Fields.FE3] = new NoFigure(this, Fields.FE3);
            _figures[Fields.FF3] = new NoFigure(this, Fields.FF3);
            _figures[Fields.FG3] = new NoFigure(this, Fields.FG3);
            _figures[Fields.FH3] = new NoFigure(this, Fields.FH3);

            _figures[Fields.FA4] = new NoFigure(this, Fields.FA4);
            _figures[Fields.FB4] = new NoFigure(this, Fields.FB4);
            _figures[Fields.FC4] = new NoFigure(this, Fields.FC4);
            _figures[Fields.FD4] = new NoFigure(this, Fields.FD4);
            _figures[Fields.FE4] = new NoFigure(this, Fields.FE4);
            _figures[Fields.FF4] = new NoFigure(this, Fields.FF4);
            _figures[Fields.FG4] = new NoFigure(this, Fields.FG4);
            _figures[Fields.FH4] = new NoFigure(this, Fields.FH4);

            _figures[Fields.FA5] = new NoFigure(this, Fields.FA5);
            _figures[Fields.FB5] = new NoFigure(this, Fields.FB5);
            _figures[Fields.FC5] = new NoFigure(this, Fields.FC5);
            _figures[Fields.FD5] = new NoFigure(this, Fields.FD5);
            _figures[Fields.FE5] = new NoFigure(this, Fields.FE5);
            _figures[Fields.FF5] = new NoFigure(this, Fields.FF5);
            _figures[Fields.FG5] = new NoFigure(this, Fields.FG5);
            _figures[Fields.FH5] = new NoFigure(this, Fields.FH5);

            _figures[Fields.FA6] = new NoFigure(this, Fields.FA6);
            _figures[Fields.FB6] = new NoFigure(this, Fields.FB6);
            _figures[Fields.FC6] = new NoFigure(this, Fields.FC6);
            _figures[Fields.FD6] = new NoFigure(this, Fields.FD6);
            _figures[Fields.FE6] = new NoFigure(this, Fields.FE6);
            _figures[Fields.FF6] = new NoFigure(this, Fields.FF6);
            _figures[Fields.FG6] = new NoFigure(this, Fields.FG6);
            _figures[Fields.FH6] = new NoFigure(this, Fields.FH6);

            CurrentFigureList.Clear();
            CurrentFigureList.Add(Fields.FA1);
            CurrentFigureList.Add(Fields.FB1);
            CurrentFigureList.Add(Fields.FC1);
            CurrentFigureList.Add(Fields.FD1);
            CurrentFigureList.Add(Fields.FE1);
            CurrentFigureList.Add(Fields.FF1);
            CurrentFigureList.Add(Fields.FG1);
            CurrentFigureList.Add(Fields.FH1);
            CurrentFigureList.Add(Fields.FA2);
            CurrentFigureList.Add(Fields.FB2);
            CurrentFigureList.Add(Fields.FC2);
            CurrentFigureList.Add(Fields.FD2);
            CurrentFigureList.Add(Fields.FE2);
            CurrentFigureList.Add(Fields.FF2);
            CurrentFigureList.Add(Fields.FG2);
            CurrentFigureList.Add(Fields.FH2);
            CurrentFigureList.Add(Fields.FA8);
            CurrentFigureList.Add(Fields.FB8);
            CurrentFigureList.Add(Fields.FC8);
            CurrentFigureList.Add(Fields.FD8);
            CurrentFigureList.Add(Fields.FE8);
            CurrentFigureList.Add(Fields.FF8);
            CurrentFigureList.Add(Fields.FG8);
            CurrentFigureList.Add(Fields.FH8);
            CurrentFigureList.Add(Fields.FA7);
            CurrentFigureList.Add(Fields.FB7);
            CurrentFigureList.Add(Fields.FC7);
            CurrentFigureList.Add(Fields.FD7);
            CurrentFigureList.Add(Fields.FE7);
            CurrentFigureList.Add(Fields.FF7);
            CurrentFigureList.Add(Fields.FG7);
            CurrentFigureList.Add(Fields.FH7);

            _material[Fields.COLOR_WHITE] = _figures[Fields.FA1].Material * 2;
            _material[Fields.COLOR_WHITE] += _figures[Fields.FB1].Material * 2;
            _material[Fields.COLOR_WHITE] += _figures[Fields.FC1].Material * 2;
            _material[Fields.COLOR_WHITE] += _figures[Fields.FD1].Material;
            _material[Fields.COLOR_WHITE] += _figures[Fields.FE1].Material;
            _material[Fields.COLOR_WHITE] += _figures[Fields.FA2].Material * 8;
            _material[Fields.COLOR_BLACK] = _material[Fields.COLOR_WHITE];
            
        }

        /// <inheritdoc />
        public void MakeMove(Move move)
        {
            //if (_analyzeMode)
            //{
            //    _fileLogger?.LogDebug($"Make move {move.FromFieldName}{move.ToFieldName}");
            //}
            var chessFigure = _figures[move.FromField];

            if (chessFigure.Color != CurrentColor)
            {
                return;
            }

            _lastMoveFromFigure = _figures[move.FromField];
            _lastMoveToFigure = _figures[move.ToField];
            _lastMoveFromField = move.FromField;
            _lastMoveToField = move.ToField;
            CapturedFigure = _lastMoveToFigure;
            move.CapturedFigure = CapturedFigure.FigureId;

            CurrentFigureList.Remove(move.FromField);
            CurrentFigureList.Add(move.ToField);

            if (_lastMoveToFigure.Color >= 0)
            {
                _material[_lastMoveToFigure.Color] -= _lastMoveToFigure.Material;
            }

            chessFigure.Field = move.ToField;
            _figures[move.ToField] = chessFigure;
            _figures[move.FromField] = new NoFigure(this, move.FromField);
            CurrentColor = EnemyColor;
            if (_lastMoveFromFigure.GeneralFigureId == FigureId.KING)
            {
                _kingPosition[_lastMoveFromFigure.Color] = move.ToField;
            }
            CheckPawnMove(move.PromotedFigure);
            CheckRochade();
            SetEnPassentTargetField();
            if (CapturedFigure.FigureId != FigureId.NO_PIECE)
            {
                HalfMoveClock = 0;
            }
            if (CurrentColor == Fields.COLOR_WHITE)
            {
                FullMoveNumber++;
            }
            var keysCount = _allPlayedMoves.Keys.Count;
            if (keysCount > FullMoveNumber)
            {
                for (int i = FullMoveNumber; i < keysCount; i++)
                {
                    _allPlayedMoves.Remove(i);
                }
            }
            keysCount = _allPlayedMoves.Keys.Count;
            if ((keysCount == 0) && EnemyColor == Fields.COLOR_BLACK)
            {
                return;
            }

            var fenPosition = GetFenPosition();
            var substring = fenPosition.Split(" ".ToCharArray())[0];
            if (chessFigure.GeneralFigureId == FigureId.PAWN)
            {
                _repetition.Clear();
                DrawByRepetition = false;
            }

            if (CapturedFigure.Color >= 0)
            {
                _repetition.Clear();
                DrawByRepetition = false;
            }
            if (_repetition.ContainsKey(substring))
            {
                _repetition[substring] = _repetition[substring] + 1;
            }
            else
            {
                _repetition[substring] = 1;
            }

            DrawByRepetition = _repetition[substring] > 2;
            if (EnemyColor == Fields.COLOR_WHITE)
            {
                var allPlayedMove = new AllMoveClass(keysCount);
                allPlayedMove.SetMove(Fields.COLOR_WHITE, move, fenPosition);

                _allPlayedMoves[keysCount] = allPlayedMove;
            }
            else
            {
                _allPlayedMoves[keysCount - 1].SetMove(Fields.COLOR_BLACK, move, fenPosition);
            }

            if (!DrawByMaterial)
            {
                bool checkMaterial = true;
                int whiteKnights = 0;
                int blackKnights = 0;
                int whiteBishops = 0;
                int blackBishops = 0;
                foreach (var figure in _figures)
                {
                    if (figure.GeneralFigureId == FigureId.KING)
                    {
                        continue;
                    }
                    if (figure.GeneralFigureId == FigureId.ROOK)
                    {
                        checkMaterial = false;
                        break;
                    }

                    if (figure.GeneralFigureId == FigureId.QUEEN)
                    {
                        checkMaterial = false;
                        break;
                    }

                    if (figure.GeneralFigureId == FigureId.PAWN)
                    {
                        checkMaterial = false;
                        break;
                    }
                    if (figure.FigureId == FigureId.WHITE_BISHOP)
                    {
                        whiteBishops++;
                        continue;
                    }
                    if (figure.FigureId == FigureId.BLACK_BISHOP)
                    {
                        blackBishops++;
                        continue;
                    }
                    if (figure.FigureId == FigureId.WHITE_KNIGHT)
                    {
                        whiteKnights++;
                        continue;
                    }
                    if (figure.FigureId == FigureId.BLACK_KNIGHT)
                    {
                        blackKnights++;
                        continue;
                    }
                }

                if (checkMaterial)
                {
                    bool whiteLessMaterial = ((whiteBishops < 2) && (whiteKnights == 0)) || ((whiteBishops == 0) && (whiteKnights < 3));
                    bool blackLessMaterial = ((blackBishops < 2) && (blackKnights == 0)) || ((blackBishops == 0) && (blackKnights < 3));

                    DrawByMaterial = whiteLessMaterial && blackLessMaterial;
                }
            }
        }

        /// <inheritdoc />
        public void MakeMove(int fromField, int toField)
        {
            MakeMove(fromField, toField, FigureId.NO_PIECE);
        }

        /// <inheritdoc />
        public void MakeMove(string pgnMove)
        {
            if (string.IsNullOrWhiteSpace(pgnMove) || pgnMove.Equals("*") || pgnMove.Equals("1-0") || pgnMove.Equals("0-1") || pgnMove.Equals("1/2-1/2"))
            {
                return;
            }

            int fieldNumber;
            var moveList = GenerateMoveList();
            if (pgnMove.Equals("0-0") || pgnMove.Equals("O-O"))
            {
                if (CurrentColor == Fields.COLOR_WHITE)
                {
                    MakeMove("e1","g1");
                    return;
                }
                MakeMove("e8", "g8");
                return;
            }
            if (pgnMove.Equals("0-0-0") || pgnMove.Equals("O-O-O"))
            {
                if (CurrentColor == Fields.COLOR_WHITE)
                {
                    MakeMove("e1", "c1");
                    return;
                }
                MakeMove("e8", "c8");
                return;
            }
            if (pgnMove.EndsWith("+"))
            {
                pgnMove = pgnMove.Replace("+", string.Empty);
            }
            if (pgnMove.EndsWith("#"))
            {
                pgnMove = pgnMove.Replace("#", string.Empty);
            }
            string figurCharacter = "P";
            string fromField = string.Empty;
            string promotionFigure = string.Empty;
            if (pgnMove.Length == 2)
            {
                fieldNumber = Fields.GetFieldNumber(pgnMove);
            }
            else
            {
                if (pgnMove.Contains("="))
                {
                    promotionFigure = pgnMove.Substring(pgnMove.Length - 1);
                    if (CurrentColor == Fields.COLOR_BLACK)
                    {
                        promotionFigure = promotionFigure.ToLower();
                    }

                    pgnMove = pgnMove.Substring(0, pgnMove.IndexOf("=", StringComparison.OrdinalIgnoreCase));
                }
                if (!"0123456789".Contains(pgnMove.Substring(pgnMove.Length - 1)))
                {
                    promotionFigure = pgnMove.Substring(pgnMove.Length - 1);
                    if (CurrentColor == Fields.COLOR_BLACK)
                    {
                        promotionFigure = promotionFigure.ToLower();
                    }
                    pgnMove = pgnMove.Substring(0, pgnMove.Length-1);
                }
                //else
                {
                    fieldNumber = Fields.GetFieldNumber(pgnMove.Substring(pgnMove.Length - 2));
                    if (pgnMove.Length > 2)
                    {
                        figurCharacter = pgnMove.Substring(0, 1);
                        if (figurCharacter.Equals(figurCharacter.ToLower()))
                        {
                            figurCharacter = "P";
                        }

                        if (pgnMove.Length == 4)
                        {
                            fromField = pgnMove.Substring(1, 1).ToUpper();
                            if (fromField.Equals("X"))
                            {
                                fromField = string.Empty;
                            }
                        }
                    }
                }
            }
            
            var moves = moveList.Where(f => f.ToField == fieldNumber).ToList();
            if (moves.Count == 1)
            {
                MakeMove(moves[0].FromField, moves[0].ToField, FigureId.FenCharacterToFigureId[promotionFigure]);
            }
            foreach (var move in moves)
            {
                var chessFigure = GetFigureOn(move.FromField);
                var fieldName = Fields.GetFieldName(move.FromField);
                var fenFigure = chessFigure.FenFigureCharacter.ToUpper();
                if (fenFigure.Equals(figurCharacter))
                {
                    if (string.IsNullOrWhiteSpace(fromField) || fieldName.Contains(fromField))
                    {
                        MakeMove(move.FromField,move.ToField,FigureId.FenCharacterToFigureId[promotionFigure]);
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public bool MoveIsValid(int fromField, int toField)
        {
            var moveList = GenerateMoveList();
            bool valid = moveList.FirstOrDefault(m => m.FromField == fromField && m.ToField == toField) != null;
            if (valid)
            {
                var color = GetFigureOn(fromField).Color;
                if (IsInCheck(color))
                {
                    ChessBoard chessBoard = new ChessBoard();
                    chessBoard.Init(this);
                    chessBoard.MakeMove(fromField, toField);
                    chessBoard.GenerateMoveList();
                    return !chessBoard.IsInCheck(color);
                }
            }

            return valid;
        }

        /// <inheritdoc />
        public void MakeMove(string fromField, string toField)
        {
            MakeMove(Fields.GetFieldNumber(fromField), Fields.GetFieldNumber(toField), FigureId.NO_PIECE);
        }

        /// <inheritdoc />
        public void MakeMove(string fromField, string toField, int promotionFigureId)
        {
            MakeMove(Fields.GetFieldNumber(fromField), Fields.GetFieldNumber(toField), promotionFigureId);
        }

        public void MakeMove(string fromField, string toField, string promotionFigure)
        {
            MakeMove(Fields.GetFieldNumber(fromField), Fields.GetFieldNumber(toField), FigureId.FenCharacterToFigureId[promotionFigure]);
        }

        public void MakeMove(int fromField, int toField, int promotionFigureId)
        {
            var chessFigure = _figures[fromField];

            if (chessFigure.Color != CurrentColor)
            {
                return;
            }

            _lastMoveFromFigure = _figures[fromField];
            _lastMoveToFigure = _figures[toField];
            _lastMoveFromField = fromField;
            _lastMoveToField = toField;
            CapturedFigure = _lastMoveToFigure;

            CurrentFigureList.Remove(fromField);
            CurrentFigureList.Add(toField);

            if (_lastMoveToFigure.Color >= 0)
            {
                _material[_lastMoveToFigure.Color] -= _lastMoveToFigure.Material;
            }

            chessFigure.Field = toField;
            _figures[toField] = chessFigure;
            _figures[fromField] = new NoFigure(this, fromField);
            CurrentColor = EnemyColor;
            if (_lastMoveFromFigure.GeneralFigureId == FigureId.KING)
            {
                _kingPosition[_lastMoveFromFigure.Color] = toField;
            }
            CheckPawnMove(promotionFigureId);
            CheckRochade();
            SetEnPassentTargetField();
            if (CapturedFigure.FigureId != FigureId.NO_PIECE)
            {
                HalfMoveClock = 0;
            }
            if (CurrentColor == Fields.COLOR_WHITE)
            {
                FullMoveNumber++;
            }
            var keysCount = _allPlayedMoves.Keys.Count;
            if (keysCount > FullMoveNumber)
            {
                for (int i = FullMoveNumber; i < keysCount; i++)
                {
                    _allPlayedMoves.Remove(i);
                }
            }
            keysCount = _allPlayedMoves.Keys.Count;
            if ((keysCount == 0) && EnemyColor == Fields.COLOR_BLACK)
            {
                return;
            }

            var fenPosition = GetFenPosition();
            var substring = fenPosition.Split(" ".ToCharArray())[0];
            if (chessFigure.GeneralFigureId == FigureId.PAWN)
            {
                _repetition.Clear();
                DrawByRepetition = false;
            }

            if (CapturedFigure.Color >= 0)
            {
                _repetition.Clear();
                DrawByRepetition = false;
            }
            if (_repetition.ContainsKey(substring))
            {
                _repetition[substring] = _repetition[substring] + 1;
            }
            else
            {
                _repetition[substring] = 1;
            }

            DrawByRepetition = _repetition[substring] > 2;
            if (EnemyColor == Fields.COLOR_WHITE)
            {
                var allPlayedMove = new AllMoveClass(keysCount);
                allPlayedMove.SetMove(Fields.COLOR_WHITE, new Move(fromField, toField, Fields.COLOR_WHITE, chessFigure.FigureId, CapturedFigure, promotionFigureId),
                                      fenPosition);

                _allPlayedMoves[keysCount] = allPlayedMove;
            }
            else
            {
                _allPlayedMoves[keysCount - 1].SetMove(Fields.COLOR_BLACK, new Move(fromField, toField, Fields.COLOR_BLACK, chessFigure.FigureId, CapturedFigure, promotionFigureId),
                                                   fenPosition);
            }

            if (!DrawByMaterial)
            {
                bool checkMaterial = true;
                int whiteKnights = 0;
                int blackKnights = 0;
                int whiteBishops = 0;
                int blackBishops = 0;
                foreach (var figure in _figures)
                {
                    if (figure.GeneralFigureId == FigureId.KING)
                    {
                        continue;
                    }
                    if (figure.GeneralFigureId == FigureId.ROOK)
                    {
                        checkMaterial = false;
                        break;
                    }

                    if (figure.GeneralFigureId == FigureId.QUEEN)
                    {
                        checkMaterial = false;
                        break;
                    }

                    if (figure.GeneralFigureId == FigureId.PAWN)
                    {
                        checkMaterial = false;
                        break;
                    }
                    if (figure.FigureId == FigureId.WHITE_BISHOP)
                    {
                        whiteBishops++;
                        continue;
                    }
                    if (figure.FigureId == FigureId.BLACK_BISHOP)
                    {
                        blackBishops++;
                        continue;
                    }
                    if (figure.FigureId == FigureId.WHITE_KNIGHT)
                    {
                        whiteKnights++;
                        continue;
                    }
                    if (figure.FigureId == FigureId.BLACK_KNIGHT)
                    {
                        blackKnights++;
                        continue;
                    }
                }

                if (checkMaterial)
                {
                    bool whiteLessMaterial = ((whiteBishops < 2) && (whiteKnights == 0)) || ((whiteBishops == 0) && (whiteKnights < 3));
                    bool blackLessMaterial = ((blackBishops < 2) && (blackKnights == 0)) || ((blackBishops == 0) && (blackKnights < 3));

                    DrawByMaterial = whiteLessMaterial && blackLessMaterial;
                }
            }
        }

        /// <inheritdoc />
        public void RemoveFigureFromField(int field)
        {

            CurrentFigureList.Remove(field);

            _figures[field] = new NoFigure(this, field);
        }

        public string GetInitialFenPosition()
        {
            return _initialFenPosition;
        }

        /// <inheritdoc />
        public void SetFigureOnPosition(int figureId, int field)
        {
            if (figureId == FigureId.WHITE_PAWN)
            {
                if (!Fields.InRow(1, field) && !Fields.InRow(8, field))
                {
                    _figures[field] = new WhitePawnFigure(this, field);
                }
            }
            if (figureId == FigureId.WHITE_BISHOP)
            {
                _figures[field] = new WhiteBishopFigure(this, field);
            }
            if (figureId == FigureId.WHITE_KNIGHT)
            {
                _figures[field] = new WhiteKnightFigure(this, field);
            }
            if (figureId == FigureId.WHITE_ROOK)
            {
                _figures[field] = new WhiteRookFigure(this, field);
            }
            if (figureId == FigureId.WHITE_QUEEN)
            {
                _figures[field] = new WhiteQueenFigure(this, field);
            }
            if (figureId == FigureId.WHITE_KING)
            {
                IChessFigure chessFigure = GetKingFigure(Fields.COLOR_WHITE);
                if (chessFigure != null)
                {
                    RemoveFigureFromField(chessFigure.Field);
                }
                _figures[field] = new WhiteKingFigure(this, field);
                _kingPosition[Fields.COLOR_WHITE] = field;
            }
            if (figureId == FigureId.BLACK_PAWN)
            {
                if (!Fields.InRow(1, field) && !Fields.InRow(8, field))
                {
                    _figures[field] = new BlackPawnFigure(this, field);
                }
            }
            if (figureId == FigureId.BLACK_BISHOP)
            {
                _figures[field] = new BlackBishopFigure(this, field);
            }
            if (figureId == FigureId.BLACK_KNIGHT)
            {
                _figures[field] = new BlackKnightFigure(this, field);
            }
            if (figureId == FigureId.BLACK_ROOK)
            {
                _figures[field] = new BlackRookFigure(this, field);
            }
            if (figureId == FigureId.BLACK_QUEEN)
            {
                _figures[field] = new BlackQueenFigure(this, field);
            }
            if (figureId == FigureId.BLACK_KING)
            {
                IChessFigure chessFigure = GetKingFigure(Fields.COLOR_BLACK);
                if (chessFigure != null)
                {
                    RemoveFigureFromField(chessFigure.Field);
                }
                _figures[field] = new BlackKingFigure(this, field);
                _kingPosition[Fields.COLOR_BLACK] = field;
            }
            CurrentFigureList.Add(field);

        }

        /// <inheritdoc />
        public void SetPosition(string fenPosition)
        {
            var currentPosition = GetFenPosition();
            try
            {
                ClearBoard();
                if (string.IsNullOrWhiteSpace(fenPosition))
                {
                    return;
                }
                var epdArray = fenPosition.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var positionArray = epdArray[0].Split("/".ToCharArray());
                var rIndex = 9;
                const string tRows = "ABCDEFGH";
                for (var i = 0; i < positionArray.Length; i++)
                {
                    rIndex--;
                    positionArray[i] = positionArray[i].Replace("1", "_");
                    positionArray[i] = positionArray[i].Replace("2", "__");
                    positionArray[i] = positionArray[i].Replace("3", "___");
                    positionArray[i] = positionArray[i].Replace("4", "____");
                    positionArray[i] = positionArray[i].Replace("5", "_____");
                    positionArray[i] = positionArray[i].Replace("6", "______");
                    positionArray[i] = positionArray[i].Replace("7", "_______");
                    positionArray[i] = positionArray[i].Replace("8", "________");
                    for (var c = 0; c < positionArray[i].Length; c++)
                    {
                        var tmpFigure = positionArray[i].Substring(c, 1);
                        var tmpPos = tRows.Substring(c, 1) + rIndex;
                        var intField = Fields.GetFieldNumber(tmpPos);
                        if (tmpFigure == "_")
                        {
                            continue;
                        }
                        if (tmpFigure == FenCodes.WhiteKing)
                        {
                            _figures[intField] = new WhiteKingFigure(this, intField);
                            _kingPosition[Fields.COLOR_WHITE] = intField;
                        }
                        if (tmpFigure == FenCodes.WhiteQueen)
                        {
                            _figures[intField] = new WhiteQueenFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.WhiteRook)
                        {
                            _figures[intField] = new WhiteRookFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.WhiteKnight)
                        {
                            _figures[intField] = new WhiteKnightFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.WhiteBishop)
                        {
                            _figures[intField] = new WhiteBishopFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.WhitePawn)
                        {
                            _figures[intField] = new WhitePawnFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.BlackKing)
                        {
                            _figures[intField] = new BlackKingFigure(this, intField);
                            _kingPosition[Fields.COLOR_BLACK] = intField;
                        }
                        if (tmpFigure == FenCodes.BlackQueen)
                        {
                            _figures[intField] = new BlackQueenFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.BlackRook)
                        {
                            _figures[intField] = new BlackRookFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.BlackKnight)
                        {
                            _figures[intField] = new BlackKnightFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.BlackBishop)
                        {
                            _figures[intField] = new BlackBishopFigure(this, intField);
                        }
                        if (tmpFigure == FenCodes.BlackPawn)
                        {
                            _figures[intField] = new BlackPawnFigure(this, intField);
                        }

                        CurrentFigureList.Add(intField);
                        _material[_figures[intField].Color] += _figures[intField].Material;
                    }
                }

                if (epdArray.Length <= 1)
                {
                    return;
                }
                CurrentColor = epdArray[1] == "w" ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
                if (IsInCheck(EnemyColor))
                {
                    throw new Exception("Invalid fen position");
                }
                if (epdArray.Length <= 2)
                {
                    return;
                }
                var castling = epdArray[2];
                _canCastling[Fields.COLOR_WHITE].ShortCastling = castling.Contains(FenCodes.WhiteKing);
                _canCastling[Fields.COLOR_WHITE].LongCastling = castling.Contains(FenCodes.WhiteQueen);
                _canCastling[Fields.COLOR_BLACK].ShortCastling = castling.Contains(FenCodes.BlackKing);
                _canCastling[Fields.COLOR_BLACK].LongCastling = castling.Contains(FenCodes.BlackQueen);
                _castled[Fields.COLOR_WHITE] =
                    !(_canCastling[Fields.COLOR_WHITE].ShortCastling ||
                      _canCastling[Fields.COLOR_WHITE].LongCastling);
                _castled[Fields.COLOR_BLACK] =
                    !(_canCastling[Fields.COLOR_BLACK].ShortCastling ||
                      _canCastling[Fields.COLOR_BLACK].LongCastling);

                if (epdArray.Length <= 3)
                {
                    return;
                }
                EnPassantTargetField = epdArray[3];
                EnPassantTargetFieldNumber = Fields.GetFieldNumber(EnPassantTargetField);

                if (epdArray.Length <= 4)
                {
                    return;
                }
                HalfMoveClock = Convert.ToInt32(epdArray[4]);

                if (epdArray.Length <= 5)
                {
                    return;
                }
                FullMoveNumber = Convert.ToInt32(epdArray[5]);
                _initialFenPosition = fenPosition;
            }
            catch
            {
                SetPosition(currentPosition);
                // throw new Exception("Invalid FEN line");
            }
        }

        /// <inheritdoc />
        public void SetPosition(int moveNumber, int color)
        {
            FullMoveNumber = moveNumber;
            SetPosition(_allPlayedMoves[moveNumber-1].GetFen(color));
        }


        private bool OneMoveBack()
        {
            //_fileLogger?.LogDebug($"Go one move back {_allPlayedMoves.Keys.Count}");
            if (_allPlayedMoves.Keys.Count > 1)
            {
                SetCurrentMove(_allPlayedMoves.Keys.Count, 0);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void SetCurrentMove(int moveNumber, int color)
        {
            List<Move> allMoves = new List<Move>();
            var playedMoveList = GetPlayedMoveList();
            for (int i = 0; i < moveNumber; i++)
            {
                if (playedMoveList.Length <= i)
                {
                    break;
                }
                allMoves.Add(playedMoveList[i]);
            }

            Init();
            NewGame();
            allMoves.ForEach(MakeMove);
        }

        /// <inheritdoc />
        public string GetFenPosition()
        {
            var fenLine = GenerateFenLine(Fields.FA8, Fields.FH8);
            fenLine += GenerateFenLine(Fields.FA7, Fields.FH7);
            fenLine += GenerateFenLine(Fields.FA6, Fields.FH6);
            fenLine += GenerateFenLine(Fields.FA5, Fields.FH5);
            fenLine += GenerateFenLine(Fields.FA4, Fields.FH4);
            fenLine += GenerateFenLine(Fields.FA3, Fields.FH3);
            fenLine += GenerateFenLine(Fields.FA2, Fields.FH2);
            fenLine += GenerateFenLine(Fields.FA1, Fields.FH1);

            fenLine += CurrentColor == Fields.COLOR_BLACK ? " b" : " w";

            var castling = string.Empty;
            if (_canCastling[Fields.COLOR_WHITE].ShortCastling)
            {
                castling += "K";
            }
            if (_canCastling[Fields.COLOR_WHITE].LongCastling)
            {
                castling += "Q";
            }
            if (_canCastling[Fields.COLOR_BLACK].ShortCastling)
            {
                castling += "k";
            }
            if (_canCastling[Fields.COLOR_BLACK].LongCastling)
            {
                castling += "q";
            }
            if (string.IsNullOrEmpty(castling))
            {
                castling = "-";
            }
            return fenLine +
                   $" {castling} {EnPassantTargetField} {HalfMoveClock} {FullMoveNumber}";
        }

        /// <inheritdoc />
        public List<Move> GenerateMoveList()
        {
            foreach (var chessFigure in _figures)
            {
                chessFigure.ClearAttacks();
            }
            CurrentMoveList = new List<Move>(50);
            EnemyMoveList = new List<Move>(50);
            if (CapturedFigure.GeneralFigureId == FigureId.KING)
            {
                return CurrentMoveList;
            }
            foreach (var i in CurrentFigureList)
            {
                var chessFigure = GetFigureOn(i);

                if (chessFigure.Color == EnemyColor)
                {
                    EnemyMoveList.AddRange(chessFigure.GetMoveList());
                }
            }
            foreach (var i in CurrentFigureList)
            {
                var chessFigure = GetFigureOn(i);

                if (chessFigure.Color == CurrentColor)
                {
                    CurrentMoveList.AddRange(chessFigure.GetMoveList());
                }
            }
            return CurrentMoveList;
        }

        /// <inheritdoc />
        public void Init()
        {
            _figures = new IChessFigure[Fields.MAX_FIELD];
            CurrentFigureList = new HashSet<int>();
            InitOutsideFields();
            CapturedFigure = _outsideFigure;
        }

        /// <inheritdoc />
        public void Init(IChessBoard chessboard)
        {
            _material[Fields.COLOR_WHITE] = chessboard.GetMaterialFor(Fields.COLOR_WHITE);
            _material[Fields.COLOR_BLACK] = chessboard.GetMaterialFor(Fields.COLOR_BLACK);
            _kingPosition[Fields.COLOR_WHITE] = chessboard.GetKingFigure(Fields.COLOR_WHITE).Field;
            _kingPosition[Fields.COLOR_BLACK] = chessboard.GetKingFigure(Fields.COLOR_BLACK).Field;
            CurrentFigureList = new HashSet<int>();
            _figures = new IChessFigure[Fields.MAX_FIELD];
            InitOutsideFields();
            CurrentColor = chessboard.CurrentColor;
            _canCastling[Fields.COLOR_WHITE].ShortCastling = chessboard.CanCastling(Fields.COLOR_WHITE,
                                                                                    CastlingEnum.Short);
            _canCastling[Fields.COLOR_WHITE].LongCastling = chessboard.CanCastling(Fields.COLOR_WHITE,
                                                                                   CastlingEnum.Long);
            _canCastling[Fields.COLOR_BLACK].ShortCastling = chessboard.CanCastling(Fields.COLOR_BLACK,
                                                                                    CastlingEnum.Short);
            _canCastling[Fields.COLOR_BLACK].LongCastling = chessboard.CanCastling(Fields.COLOR_BLACK,
                                                                                   CastlingEnum.Long);
            _castled[Fields.COLOR_WHITE] = chessboard.Castled(Fields.COLOR_WHITE);
            _castled[Fields.COLOR_BLACK] = chessboard.Castled(Fields.COLOR_BLACK);
            EnPassantTargetField = chessboard.EnPassantTargetField;
            EnPassantTargetFieldNumber = chessboard.EnPassantTargetFieldNumber;
            HalfMoveClock = chessboard.HalfMoveClock;
            FullMoveNumber = chessboard.FullMoveNumber;
            foreach (var boardField in Fields.BoardFields)
            {
                var chessFigure = chessboard.GetFigureOn(boardField);
                switch (chessFigure.FigureId)
                {
                    case FigureId.NO_PIECE:
                        _figures[boardField] = new NoFigure(this, boardField);
                        break;
                    case FigureId.WHITE_PAWN:
                        _figures[boardField] = new WhitePawnFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.WHITE_BISHOP:
                        _figures[boardField] = new WhiteBishopFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.WHITE_KNIGHT:
                        _figures[boardField] = new WhiteKnightFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.WHITE_ROOK:
                        _figures[boardField] = new WhiteRookFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.WHITE_QUEEN:
                        _figures[boardField] = new WhiteQueenFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.WHITE_KING:
                        _figures[boardField] = new WhiteKingFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.BLACK_PAWN:
                        _figures[boardField] = new BlackPawnFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.BLACK_BISHOP:
                        _figures[boardField] = new BlackBishopFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.BLACK_KNIGHT:
                        _figures[boardField] = new BlackKnightFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.BLACK_ROOK:
                        _figures[boardField] = new BlackRookFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.BLACK_QUEEN:
                        _figures[boardField] = new BlackQueenFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                    case FigureId.BLACK_KING:
                        _figures[boardField] = new BlackKingFigure(this, boardField);
                        CurrentFigureList.Add(boardField);
                        break;
                }
            }
            CapturedFigure = chessboard.CapturedFigure;
        }

        /// <inheritdoc />
        public int GetBoardHash()
        {
            var b = new HashBuilder();
            foreach (var i in CurrentFigureList)
            {
                b.AddItem(i);
                b.AddItem(GetFigureOn(i).FigureId);
            }
            b.AddItem(CurrentColor);
            return b.Result;
        }

        /// <inheritdoc />
        public bool IsInCheck(int color)
        {
            var enemyColor = color == Fields.COLOR_BLACK ? Fields.COLOR_WHITE : Fields.COLOR_BLACK;
            return GetKingFigure(color).IsAttackedByColor(enemyColor);
        }

        /// <inheritdoc />
        public string GetMove(string newFenPosition, bool ignoreRule)
        {
            try
            {
                bool kingMove = false;
                string fromField = string.Empty;
                string toField = string.Empty;
                string fromFenFigure = string.Empty;
                string toFenFigure = string.Empty;
                var internalChessBoard = new ChessBoard();
                internalChessBoard.Init(this);
                internalChessBoard.SetPosition(newFenPosition);
                for (int i = Fields.FA1; i <= Fields.FH8; i++)
                {
                    var figureOnField = internalChessBoard.GetFigureOn(i);
                    var currentFigurOnField = GetFigureOn(i);
                    if (currentFigurOnField.FenFigureCharacter.Equals(figureOnField.FenFigureCharacter))
                    {
                        continue;
                    }

                    if (figureOnField.Color == Fields.COLOR_OUTSIDE)
                    {
                        continue;
                    }

                    if (figureOnField.Color == Fields.COLOR_EMPTY)
                    {
                        if (currentFigurOnField.GeneralFigureId == FigureId.KING)
                        {
                            kingMove = true;
                            fromField = Fields.GetFieldName(i);
                            fromFenFigure = currentFigurOnField.FenFigureCharacter;
                            continue;
                        }

                        if (!kingMove)
                        {
                            fromFenFigure = currentFigurOnField.FenFigureCharacter;
                            fromField = Fields.GetFieldName(i);
                        }
                    }
                    else
                    {
                        if (figureOnField.GeneralFigureId == FigureId.KING)
                        {
                            kingMove = true;
                            toField = Fields.GetFieldName(i);
                            toFenFigure = figureOnField.FenFigureCharacter;
                            continue;
                        }

                        if (!kingMove)
                        {
                            toField = Fields.GetFieldName(i);
                            toFenFigure = figureOnField.FenFigureCharacter;
                        }
                    }
                }

                if (_analyzeMode)
                {

                }

                //if (_analyzeMode && !string.IsNullOrWhiteSpace(fromField) && !string.IsNullOrWhiteSpace(toField))
                //{
                //    _fileLogger?.LogDebug($"Check {newFenPosition}");
                //    _fileLogger?.LogDebug($"      {fromField}{toField}");
                //    string jj = GetPlayedMoveList().Aggregate(string.Empty, (current, move) => current + $" {move.FromFieldName}{move.ToFieldName}");
                //    _fileLogger?.LogDebug($"Movelist: {jj}");
                //}

                if (!ignoreRule && !MoveIsValid(Fields.GetFieldNumber(fromField), Fields.GetFieldNumber(toField)))
                {
                    
                    if (_analyzeMode && !string.IsNullOrWhiteSpace(fromField) && !string.IsNullOrWhiteSpace(toField))
                    {
                        //_fileLogger?.LogDebug($"Invalid move: {fromField}{toField}");
                        //_fileLogger?.LogDebug($"last move toField: {Fields.GetFieldName(_lastMoveToField)}");
                        if (_lastMoveToField == Fields.GetFieldNumber(fromField))
                        {
                            //_fileLogger?.LogDebug($"{fromField} is equal last toMoveField");
                            if (OneMoveBack())
                              return GetMove(newFenPosition, false);
                        }
                    }
                    return string.Empty;
                }

                string promote = fromFenFigure.Equals(toFenFigure) ? string.Empty : toFenFigure;
                return $"{fromField}{toField}".ToLower()+promote;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public string GetChangedFigure(string oldFenPosition, string newFenPosition)
        {
            if (string.IsNullOrWhiteSpace(oldFenPosition) || string.IsNullOrWhiteSpace(newFenPosition))
            {
                return " ";
            }

            var internalChessBoardOld = new ChessBoard();
            internalChessBoardOld.Init();
            internalChessBoardOld.NewGame();
            internalChessBoardOld.SetPosition(oldFenPosition);
            var internalChessBoardNew = new ChessBoard();
            internalChessBoardNew.Init();
            internalChessBoardNew.NewGame();
            internalChessBoardNew.SetPosition(newFenPosition);
            for (int i = Fields.FA1; i <= Fields.FH8; i++)
            {
                var oldFigureOnField = internalChessBoardNew.GetFigureOn(i);
                var newFigureOnField = internalChessBoardOld.GetFigureOn(i);
                if (newFigureOnField.FenFigureCharacter.Equals(oldFigureOnField.FenFigureCharacter))
                {
                    continue;
                }

                if (oldFigureOnField.Color == Fields.COLOR_OUTSIDE)
                {
                    continue;
                }

                if (oldFigureOnField.Color == Fields.COLOR_EMPTY && newFigureOnField.Color != Fields.COLOR_EMPTY)
                {
                    return newFigureOnField.FenFigureCharacter;
                }

                if (oldFigureOnField.Color != Fields.COLOR_EMPTY && newFigureOnField.Color == Fields.COLOR_EMPTY)
                {
                    return oldFigureOnField.FenFigureCharacter;

                }

            }

            return string.Empty;

        }

        public bool IsBasePosition(string fenPosition)
        {
            if (string.IsNullOrWhiteSpace(fenPosition))
            {
                return false;
            }
            return  FenCodes.BasePosition.StartsWith(fenPosition.Split(" ".ToCharArray())[0]);
        }

        #region private

        private void InitOutsideFields()
        {
            foreach (var outsideField in Fields.OutsideFields)
            {
                _figures[outsideField] = _outsideFigure;
            }
        }

        private string GenerateFenLine(int startField, int targetField)
        {
            var line = string.Empty;
            var noFigureCounter = 0;
            for (var f = startField; f <= targetField; f++)
            {
                var tmpFigure = _figures[f];
                if (tmpFigure.FigureId == FigureId.NO_PIECE)
                {
                    noFigureCounter++;
                }
                else
                {
                    if (noFigureCounter > 0)
                    {
                        line = line + noFigureCounter;
                    }
                    noFigureCounter = 0;
                    line = line + tmpFigure.FenFigureCharacter;
                }
            }
            if (noFigureCounter > 0)
            {
                line = line + noFigureCounter;
            }
            if (startField != Fields.FA1)
            {
                return line + "/";
            }
            return line;
        }

        private void CheckRochade()
        {
            if (_lastMoveFromFigure.FigureId == FigureId.WHITE_KING && _lastMoveFromField == Fields.FE1)
            {
                _canCastling[Fields.COLOR_WHITE].ShortCastling = false;
                _canCastling[Fields.COLOR_WHITE].LongCastling = false;
                switch (_lastMoveToField)
                {
                    case Fields.FG1:
                        _castled[Fields.COLOR_WHITE] = true;
                        _figures[Fields.FF1] = _figures[Fields.FH1];
                        _figures[Fields.FF1].Field = Fields.FF1;
                        _figures[Fields.FH1] = new NoFigure(this, Fields.FH1);
                        CurrentFigureList.Remove(Fields.FH1);
                        CurrentFigureList.Add(Fields.FF1);
                        break;
                    case Fields.FC1:
                        _castled[Fields.COLOR_WHITE] = true;
                        _figures[Fields.FD1] = _figures[Fields.FA1];
                        _figures[Fields.FD1].Field = Fields.FD1;
                        _figures[Fields.FA1] = new NoFigure(this, Fields.FA1);
                        CurrentFigureList.Remove(Fields.FA1);
                        CurrentFigureList.Add(Fields.FD1);
                        break;
                }
            }
            else if (_lastMoveFromFigure.FigureId == FigureId.BLACK_KING && _lastMoveFromField == Fields.FE8)
            {
                _canCastling[Fields.COLOR_BLACK].ShortCastling = false;
                _canCastling[Fields.COLOR_BLACK].LongCastling = false;
                switch (_lastMoveToField)
                {
                    case Fields.FG8:
                        _castled[Fields.COLOR_BLACK] = true;
                        _figures[Fields.FF8] = _figures[Fields.FH8];
                        _figures[Fields.FF8].Field = Fields.FF8;
                        _figures[Fields.FH8] = new NoFigure(this, Fields.FH8);
                        CurrentFigureList.Remove(Fields.FH8);
                        CurrentFigureList.Add(Fields.FF8);
                        break;
                    case Fields.FC8:
                        _castled[Fields.COLOR_BLACK] = true;
                        _figures[Fields.FD8] = _figures[Fields.FA8];
                        _figures[Fields.FD8].Field = Fields.FD8;
                        _figures[Fields.FA8] = new NoFigure(this, Fields.FA8);
                        CurrentFigureList.Remove(Fields.FA8);
                        CurrentFigureList.Add(Fields.FD8);
                        break;
                }
            }
            else
            {
                switch (_lastMoveFromFigure.FigureId)
                {
                    case FigureId.BLACK_ROOK:
                        switch (_lastMoveFromField)
                        {
                            case Fields.FA8:
                                _canCastling[Fields.COLOR_BLACK].LongCastling = false;
                                return;
                            case Fields.FH8:
                                _canCastling[Fields.COLOR_BLACK].ShortCastling = false;
                                return;
                        }
                        break;
                    case FigureId.WHITE_ROOK:
                        switch (_lastMoveFromField)
                        {
                            case Fields.FA1:
                                _canCastling[Fields.COLOR_WHITE].LongCastling = false;
                                return;
                            case Fields.FH1:
                                _canCastling[Fields.COLOR_WHITE].ShortCastling = false;
                                return;
                        }
                        break;
                }
            }
        }

        private void CheckPawnMove(int promotionFigure)
        {
            if (_lastMoveFromFigure.GeneralFigureId != FigureId.PAWN)
            {
                HalfMoveClock++;
                return;
            }

            HalfMoveClock=0;
            if (_lastMoveFromFigure.Color == Fields.COLOR_WHITE)
            {
                // Umwandlung?
                if (_lastMoveToField >= Fields.FA8)
                {
                    if (promotionFigure == FigureId.NO_PIECE || promotionFigure == FigureId.BLACK_QUEEN)
                    {
                        promotionFigure = FigureId.WHITE_QUEEN;
                    }

                    if (promotionFigure == FigureId.BLACK_BISHOP)
                    {
                        promotionFigure = FigureId.WHITE_BISHOP;
                    }
                    if (promotionFigure == FigureId.BLACK_ROOK)
                    {
                        promotionFigure = FigureId.WHITE_ROOK;
                    }
                    if (promotionFigure == FigureId.BLACK_KNIGHT)
                    {
                        promotionFigure = FigureId.WHITE_KNIGHT;
                    }
                    CurrentFigureList.Remove(_lastMoveToField);
                    if (_figures[_lastMoveToField].Color >= 0)
                    {
                        _material[_figures[_lastMoveToField].Color] -=
                            _figures[_lastMoveToField].Material;
                    }
                    _figures[_lastMoveToField] = CreatePromotionFigure(promotionFigure, _lastMoveToField);
                    CurrentFigureList.Add(_lastMoveToField);
                    _material[_figures[_lastMoveToField].Color] +=
                        _figures[_lastMoveToField].Material;
                    return;
                }
            }
            else
            {
                // Umwandlung
                if (_lastMoveToField <= Fields.FH1)
                {
                    if (promotionFigure == FigureId.NO_PIECE || promotionFigure == FigureId.WHITE_QUEEN)
                    {
                        promotionFigure = FigureId.BLACK_QUEEN;
                    }
                    if (promotionFigure == FigureId.WHITE_BISHOP)
                    {
                        promotionFigure = FigureId.BLACK_BISHOP;
                    }
                    if (promotionFigure == FigureId.WHITE_ROOK)
                    {
                        promotionFigure = FigureId.BLACK_ROOK;
                    }
                    if (promotionFigure == FigureId.WHITE_KNIGHT)
                    {
                        promotionFigure = FigureId.BLACK_KNIGHT;
                    }
                    CurrentFigureList.Remove(_lastMoveToField);
                    if (_figures[_lastMoveToField].Color >= 0)
                    {
                        _material[_figures[_lastMoveToField].Color] -=
                            _figures[_lastMoveToField].Material;
                    }
                    _figures[_lastMoveToField] = CreatePromotionFigure(promotionFigure, _lastMoveToField);
                    CurrentFigureList.Add(_lastMoveToField);
                    _material[_figures[_lastMoveToField].Color] +=
                        _figures[_lastMoveToField].Material;
                    return;
                }
            }
            if (_lastMoveToFigure.FigureId == FigureId.NO_PIECE)
            {
                if (_lastMoveFromFigure.Color == Fields.COLOR_WHITE)
                {
                    // e.p?
                    if (_lastMoveToField - _lastMoveFromField == 11 || _lastMoveToField - _lastMoveFromField == 9)
                    {
                        if (_figures[_lastMoveToField - 10].Color >= 0)
                        {
                            _material[_figures[_lastMoveToField - 10].Color] -=
                                _figures[_lastMoveToField - 10].Material;
                        }
                        CurrentFigureList.Remove(_lastMoveToField - 10);
                        _figures[_lastMoveToField - 10] = new NoFigure(this, _lastMoveToField - 10);
                    }
                }
                else
                {
                    // Umwandlung?
                    if (_lastMoveToField <= Fields.FH1)
                    {
                        if (_figures[_lastMoveToField].Color >= 0)
                        {
                            _material[_figures[_lastMoveToField].Color] -=
                                _figures[_lastMoveToField].Material;
                        }
                        _figures[_lastMoveToField] = CreatePromotionFigure(promotionFigure, _lastMoveToField);
                        _material[_figures[_lastMoveToField].Color] +=
                            _figures[_lastMoveToField].Material;
                        return;
                    }
                    // e.p.?
                    if (_lastMoveFromField - _lastMoveToField == 9 || _lastMoveFromField - _lastMoveToField == 11)
                    {
                        if (_figures[_lastMoveToField + 10].Color >= 0)
                        {
                            _material[_figures[_lastMoveToField + 10].Color] -=
                                _figures[_lastMoveToField + 10].Material;
                        }
                        CurrentFigureList.Remove(_lastMoveToField + 10);
                        _figures[_lastMoveToField + 10] = new NoFigure(this, _lastMoveToField + 10);
                    }
                }
            }
        }

        private IChessFigure CreatePromotionFigure(int figureId, int field)
        {
            switch (figureId)
            {
                case FigureId.BLACK_BISHOP: return new BlackBishopFigure(this, field);
                case FigureId.BLACK_KNIGHT: return new BlackKnightFigure(this, field);
                case FigureId.BLACK_QUEEN: return new BlackQueenFigure(this, field);
                case FigureId.BLACK_ROOK: return new BlackRookFigure(this, field);
                case FigureId.WHITE_BISHOP: return new WhiteBishopFigure(this, field);
                case FigureId.WHITE_KNIGHT: return new WhiteKnightFigure(this, field);
                case FigureId.WHITE_QUEEN: return new WhiteQueenFigure(this, field);
                case FigureId.WHITE_ROOK: return new WhiteRookFigure(this, field);
                default: return new NoFigure(this, field);
            }
        }

        private void ClearBoard()
        {

            //_allPlayedMoves.Clear();
            CurrentColor = Fields.COLOR_EMPTY;

            _canCastling[Fields.COLOR_WHITE].ShortCastling = false;
            _canCastling[Fields.COLOR_WHITE].LongCastling = false;
            _canCastling[Fields.COLOR_BLACK].ShortCastling = false;
            _canCastling[Fields.COLOR_BLACK].LongCastling = false;
            _castled[Fields.COLOR_WHITE] = false;
            _castled[Fields.COLOR_BLACK] = false;
            _material[Fields.COLOR_WHITE] = 0;
            _material[Fields.COLOR_BLACK] = 0;
            _kingPosition[Fields.COLOR_WHITE] = 0;
            _kingPosition[Fields.COLOR_BLACK] = 0;

            EnPassantTargetField = "-";
            EnPassantTargetFieldNumber = -1;
            HalfMoveClock = 0;
            FullMoveNumber = 1;

            foreach (var boardField in Fields.BoardFields)
            {
                _figures[boardField] = new NoFigure(this, boardField);
            }
            CurrentFigureList.Clear();
        }

        private void SetEnPassentTargetField()
        {
            EnPassantTargetField = "-";
            EnPassantTargetFieldNumber = -1;
            if (_lastMoveFromFigure.GeneralFigureId != FigureId.PAWN)
            {
                return;
            }
            if (_lastMoveFromFigure.Color == Fields.COLOR_WHITE)
            {
                if (_lastMoveToField - _lastMoveFromField == 20)
                {
                    //if (_figures[_lastMoveToField - 1].Color == Fields.COLOR_BLACK ||
                    //    _figures[_lastMoveToField + 1].Color == Fields.COLOR_BLACK)
                    //{
                        EnPassantTargetField = Fields.GetFieldName(_lastMoveToField - 10).ToLower();
                        EnPassantTargetFieldNumber = _lastMoveToField - 10;
                    //}
                }
            }
            else
            {
                if (_lastMoveFromField - _lastMoveToField == 20)
                {
                    //if (_figures[_lastMoveToField - 1].Color == Fields.COLOR_WHITE ||
                    //    _figures[_lastMoveToField + 1].Color == Fields.COLOR_WHITE)
                    //{
                        EnPassantTargetField = Fields.GetFieldName(_lastMoveToField + 10).ToLower();
                        EnPassantTargetFieldNumber = _lastMoveToField + 10;
                    //}
                }
            }
        }

        /// <inheritdoc />
        public AllMoveClass GetPrevMove()
        {
            var keysCount = _allPlayedMoves.Keys.Count;
            if (keysCount > 0)
            {
                var allPlayedMove = _allPlayedMoves[keysCount - 1];
                //_allPlayedMoves.Remove(keysCount - 1);
                return allPlayedMove;
            }

            return null;
        }

    

        private struct Castling
        {
            public bool ShortCastling;
            public bool LongCastling;
        }

        internal class HashBuilder
        {
            private const int Prime1 = 17;
            private const int Prime2 = 23;

            public HashBuilder()
            {
            }

            public HashBuilder(int startHash)
            {
                Result = startHash;
            }

            public int Result { get; private set; } = Prime1;

            public void AddItem<T>(T item)
            {
                unchecked
                {
                    Result = Result * Prime2 + item.GetHashCode();
                }
            }

            public void AddItems<T1, T2>(T1 item1, T2 item2)
            {
                AddItem(item1);
                AddItem(item2);
            }

            public void AddItems<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
            {
                AddItem(item1);
                AddItem(item2);
                AddItem(item3);
            }

            public void AddItems<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3,
                                                 T4 item4)
            {
                AddItem(item1);
                AddItem(item2);
                AddItem(item3);
                AddItem(item4);
            }

            public void AddItems<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3,
                                                     T4 item4, T5 item5)
            {
                AddItem(item1);
                AddItem(item2);
                AddItem(item3);
                AddItem(item4);
                AddItem(item5);
            }

            public void AddItems<T>(params T[] items)
            {
                foreach (var item in items)
                {
                    AddItem(item);
                }
            }
        }

        #endregion

    }
}

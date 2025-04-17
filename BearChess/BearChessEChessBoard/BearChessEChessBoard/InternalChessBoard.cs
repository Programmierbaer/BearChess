using System.Collections.Generic;
using System.Linq;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public class InternalChessBoard : IInternalChessBoard
    {
        private readonly ChessBoard _bearChessBoard = new ChessBoard();
        private List<Move> _moveList = new List<Move>();
        private List<Move> _moveListWithFen = new List<Move>();
        private List<Move> _enemyMoveList = new List<Move>();

        public const int FA1 = 21;
        public const int FH8 = 98;
        public int CurrentColor => _bearChessBoard.CurrentColor;
        public int EnemyColor => _bearChessBoard.EnemyColor;

        public Move[] CurrentMoveList => _moveList.ToArray();
        public Move[] EnemyMoveList => _enemyMoveList.ToArray();

        public InternalChessBoard()
        {
            _bearChessBoard.Init();
        }

        /// <inheritdoc />
        public string GetFigureOnField(int field)
        {
            return _bearChessBoard.GetFigureOn(field).FenFigureCharacter;

        }

        /// <inheritdoc />
        public void NewGame()
        {
            _bearChessBoard.Init();
            _bearChessBoard.NewGame();
            _moveList = _bearChessBoard.GenerateMoveList();
            _enemyMoveList = _bearChessBoard.EnemyMoveList;
        }

        /// <inheritdoc />
        public string GetBestMove()
        {
            var firstOrDefault = _moveList.FirstOrDefault();
            if (firstOrDefault == null)
            {
                return string.Empty;
            }

            return $"{Fields.GetFieldName(firstOrDefault.FromField)}{Fields.GetFieldName(firstOrDefault.ToField)}".ToLower();
        }

        /// <inheritdoc />
        public bool IsBasePosition(string fenPosition)
        {
            return _bearChessBoard.IsBasePosition(fenPosition);
        }

        public AllMoveClass GetPrevMove()
        {
            return _bearChessBoard.GetPrevMove();
        }

        public void TakeBack()
        {
            _bearChessBoard.TakeBack();
            _moveListWithFen = _bearChessBoard.GenerateFenPositionList();
        }

        public static string GetFieldName(int field)
        {
            string[] row = { "", "A", "B", "C", "D", "E", "F", "G", "H" };
            var zehner = field / 10;
            var einer = field - zehner * 10;
            zehner--;
            return row[einer] + zehner;
        }

        /// <inheritdoc />
        public void SetPosition(string fenPosition)
        {
            _bearChessBoard.SetPosition(fenPosition);
            _moveList = _bearChessBoard.GenerateMoveList().Where(m => m.FigureColor==_bearChessBoard.CurrentColor).ToList();
            _enemyMoveList = _bearChessBoard.EnemyMoveList;
        }

        /// <inheritdoc />
        public void MakeMove(string fromField, string toField, string promote)
        {
            if (Configuration.Instance.GetBoolValue("checkForAlternateMoves", false))
            {
                _moveListWithFen = _bearChessBoard.GenerateFenPositionList();
            }
            if (string.IsNullOrWhiteSpace(promote))
            {
                _bearChessBoard.MakeMove(fromField, toField);
            }
            else
            {
                _bearChessBoard.MakeMove(fromField, toField, promote);
            }

            _moveList = _bearChessBoard.GenerateMoveList();
            _enemyMoveList = _bearChessBoard.EnemyMoveList;
        }

        /// <inheritdoc />
        public string GetMove(string newFenPosition, bool ignoreRule)
        {
            return _bearChessBoard.GetMove(newFenPosition, ignoreRule);
        }

        public Move GetAlternateMove(string newFenPosition)
        {
            return _moveListWithFen.FirstOrDefault(m => m.Fen.StartsWith(newFenPosition));
        }

        public string GetChangedFigure(string oldFenPosition, string newFenPosition)
        {
            return _bearChessBoard.GetChangedFigure(oldFenPosition, newFenPosition);
        }

        /// <inheritdoc />
        public string GetPosition()
        {
            return _bearChessBoard.GetFenPosition();

        }
    }
}

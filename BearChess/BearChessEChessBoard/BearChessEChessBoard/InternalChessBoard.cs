using System.Linq;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public class InternalChessBoard : IInternalChessBoard
    {
        private readonly BearChessBase.Implementations.ChessBoard _bearChessBoard = new BearChessBase.Implementations.ChessBoard();

        public const int FA1 = 21;
        public const int FH8 = 98;
        public int CurrentColor => _bearChessBoard.CurrentColor;

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
        }

        /// <inheritdoc />
        public string GetBestMove()
        {
            var firstOrDefault = _bearChessBoard.GenerateMoveList().FirstOrDefault();
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

        }

        /// <inheritdoc />
        public void MakeMove(string fromField, string toField, string promote)
        {
            if (string.IsNullOrWhiteSpace(promote))
            {
                _bearChessBoard.MakeMove(fromField, toField);
            }
            else
            {
                _bearChessBoard.MakeMove(fromField, toField, promote);
            }
        }

        /// <inheritdoc />
        public string GetMove(string newFenPosition, bool ignoreRule)
        {
            return _bearChessBoard.GetMove(newFenPosition, ignoreRule);
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

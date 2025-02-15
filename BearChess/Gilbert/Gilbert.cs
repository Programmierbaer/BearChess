using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.Engine;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Gilbert
{
    public class Gilbert : IChessEngine
    {
        private readonly IGetImage _getImage;
        private readonly IChessBoard _chessBoard;
        private bool _stop;
        private Move _bestMove;
        private ILogger _logger;
        private bool _logging;
        private static readonly List<Move> CurrentMoveList = new List<Move>(5);
        private GamePhase _gamePhase = GamePhase.Opening;
        private BoardEvaluation[] _boardEvaluations;

        public string Identification => "Gilbert 0.0.2";
        public string Author => "Lars Nowak";
        public byte[] Logo => _getImage.Image;
        
        public event EventHandler<EngineFinishedCalculationEventArgs> EngineFinishedCalculationEvent;
        public event EventHandler<EngineInformationEventArgs> EngineInformationEvent;

        public bool Logging
        {
            get => _logging;
            set
            {
                _logging = value;
                if (!_logging)
                {
                    _logger.Close();
                }
            }
        }

        public Gilbert()
        {
            _getImage = new GetImage();
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _logger = new FileLogger($"gilbert_{DateTime.UtcNow.Ticks}.log", 10, 2);
            _logger.Log("========================================");
            _logger.Log($"{Identification} von {Author}");
            _logger.Log(" ");
            _logger.Pause();
            _bestMove = null;
        }



        public void NewGame()
        {
            _chessBoard.NewGame();
            _gamePhase = GamePhase.Opening;
            _logger.Continue();
            _logger.Log("=============== New Game ===============");            
            _logger.Pause();
        }


        /// <inheritdoc />
        public void MakeMove(string fromField, string toField)
        {
            _chessBoard.MakeMove(fromField, toField);
        }

        public void MakeMove(string fromField, string toField, int promotionFigureId)
        {
            _chessBoard.MakeMove(fromField, toField, promotionFigureId);
        }

        public void SetPosition(string fen)
        {
            _chessBoard.NewGame();
            _gamePhase = GamePhase.Opening;
            _chessBoard.SetPosition(fen, false);
        }

        public async Task<Move> Evaluate()
        {
            return await Evaluate(3);
        }

        public async Task<Move> Evaluate(int maxDeep)
        {
            _stop = false;
            _logger.Continue();
            CurrentMoveList.Clear();
            _boardEvaluations = new BoardEvaluation[2];
            return await Task.Run(() =>
            {
                var currentMoveList = _chessBoard.GenerateMoveList();
                _boardEvaluations = EvaluateBoard(_chessBoard);
                foreach (var move in currentMoveList)
                {
                    _logger.Log("*");
                    _logger.Log($"{move}");
                    var chessBoard = new ChessBoard();
                    chessBoard.Init(_chessBoard);
                    chessBoard.MakeMove(move);
                    chessBoard.GenerateMoveList();
                    move.Value = Evaluate(chessBoard,maxDeep);
                    _logger.Log($"{move}");

                }

                _bestMove = GetBestMove();
                return _bestMove;
            });
        }

    

        public void Stop()
        {
            _stop = true;
        }

        public Move GetBestMove()
        {
            return _chessBoard.CurrentMoveList.OrderByDescending(m => m.Value).FirstOrDefault();
        }

        public List<Move> GetBestMovesLine()
        {
            return new List<Move>();
        }

        public List<Move> GetMoves()
        {
            return _chessBoard.CurrentMoveList;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger GetLogger()
        {
            return _logger;
        }

        public string GetBoardEvaluation(int color)
        {
            return string.Empty;
        }

        #region private

        private int Evaluate(IChessBoard chessBoard, int maxDeep)
        {
            maxDeep--;
            if (maxDeep > 0)
            {
                int maxValue = int.MinValue;
                string filler = " ".PadLeft(3 - maxDeep);
                var currentMoveList = chessBoard.GenerateMoveList();
                foreach (var move in currentMoveList)
                {
                    _logger.Log($"{filler}{move}");
                    var localChessBoard = new ChessBoard();
                    localChessBoard.Init(chessBoard);
                    localChessBoard.MakeMove(move);
                    localChessBoard.GenerateMoveList();
                    move.Value = Evaluate(localChessBoard, maxDeep);
                    if (move.Value > maxValue)
                    {
                        maxValue = move.Value;
                    }
                    _logger.Log($"{filler}{move}");
                }

                return -maxValue;
            }

            return CompareBoard(chessBoard);

        }

        private BoardEvaluation[] EvaluateBoard(IChessBoard chessBoard)
        {
            BoardEvaluation[] boardEvaluations = new BoardEvaluation[2];
            boardEvaluations[chessBoard.CurrentColor] =
                new BoardEvaluation()
                {
                    Material = chessBoard.GetMaterialFor(chessBoard.CurrentColor) -
                               chessBoard.GetMaterialFor(chessBoard.EnemyColor),
                    FigurePositionValues = new Dictionary<int, List<int>>(),
                    Mobility = chessBoard.CurrentMoveList.Count
                };
            boardEvaluations[chessBoard.EnemyColor] =
                new BoardEvaluation()
                {
                    Material = chessBoard.GetMaterialFor(chessBoard.EnemyColor) -
                               chessBoard.GetMaterialFor(chessBoard.CurrentColor),
                    FigurePositionValues = new Dictionary<int, List<int>>(),
                    Mobility = chessBoard.EnemyMoveList.Count
                };
            foreach (int i in chessBoard.CurrentFigureList)
            {
                var chessFigure = chessBoard.GetFigureOn(i);
                var positionValue = chessFigure.PositionValue(chessBoard, _gamePhase);
                if (boardEvaluations[chessFigure.Color].FigurePositionValues.ContainsKey(chessFigure.GeneralFigureId))
                {
                    boardEvaluations[chessFigure.Color]
                        .FigurePositionValues[chessFigure.GeneralFigureId].Add(positionValue);
                }
                else
                {
                    boardEvaluations[chessFigure.Color]
                        .FigurePositionValues[chessFigure.GeneralFigureId] = new List<int>() {positionValue};
                }
            }

            return boardEvaluations;
        }

        private int CompareBoard(IChessBoard chessBoard)
        {
            _logger.Log($"Compare with {chessBoard.CurrentColor}");
            BoardEvaluation[] boardEvaluations = EvaluateBoard(chessBoard);
            var material = boardEvaluations[_chessBoard.CurrentColor].Material - _boardEvaluations[_chessBoard.CurrentColor].Material;
            var posValues = boardEvaluations[_chessBoard.CurrentColor].GetPositionSummary() - _boardEvaluations[_chessBoard.CurrentColor].GetPositionSummary();

            return material + posValues;
        }

        #endregion
    }
}

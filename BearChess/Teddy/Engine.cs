using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.Engine;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Teddy
{
    public class Engine
    {

        public class EvaluateResult
        {
            public Move Move { get; set; }
            public BoardEvaluation[] BoardEvaluationsInit { get; set; }
            public BoardEvaluation[] BoardEvaluationsMove { get; set; }
            public int MaterialDiff { get; }
            public int PositionsDiff { get; }
            public int MobilityDiff { get; }

            public EvaluateResult(Move move, BoardEvaluation[] boardEvaluationsInit, BoardEvaluation[] boardEvaluationsMove,int currentColor, int enemyColor)
            {
                Move = move;
                BoardEvaluationsInit = boardEvaluationsInit;
                BoardEvaluationsMove = boardEvaluationsMove;
                MaterialDiff = BoardEvaluationsInit[currentColor].Material - BoardEvaluationsMove[currentColor].Material;
                MobilityDiff = BoardEvaluationsInit[currentColor].Mobility - BoardEvaluationsMove[currentColor].Mobility;
                PositionsDiff = BoardEvaluationsInit[currentColor].GetPositionSummary() -
                                BoardEvaluationsMove[currentColor].GetPositionSummary();
            }
        }

        private readonly IChessBoard _chessBoard;
        private GamePhase _gamePhase;
        private FileLogger _logger;
        private BoardEvaluation[] _boardEvaluations;
        private Move _bestMove;

        public Engine(FileLogger logger)
        {
            _chessBoard = new ChessBoard();
            _chessBoard.Init();
            _logger = logger;
        }

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
            _chessBoard.SetPosition(fen);
        }

        public void Init(IChessBoard chessBoard)
        {
            _chessBoard.Init(chessBoard);
        }

        public Move Evaluate()
        {
            List<EvaluateResult> allResult = new List<EvaluateResult>();
            var currentMoveList = _chessBoard.GenerateMoveList();
            _boardEvaluations = new BoardEvaluation[2];
            _boardEvaluations = EvaluateBoard(_chessBoard);
            foreach (var move in currentMoveList)
            {
                _logger?.LogDebug("*");
                _logger?.LogDebug($"{move}");
                var chessBoard = new ChessBoard();
                chessBoard.Init(_chessBoard);
                chessBoard.MakeMove(move);
                chessBoard.GenerateMoveList();
                move.Value = Evaluate(chessBoard, 3, true);
                _logger?.LogDebug($"{move}");
            }

            _bestMove = GetBestMove();
            return _bestMove;
        }

        public Move[] GetMoveList()
        {
            return _chessBoard.CurrentMoveList.OrderByDescending(m => m.Value).ToArray();
        }

        public Move GetBestMove()
        {
            return _chessBoard.CurrentMoveList.OrderByDescending(m => m.Value).FirstOrDefault();
        }


       

        private int CompareBoard(IChessBoard chessBoard)
        {
            //_logger?.LogDebug($"   Compare with {chessBoard.CurrentColor}");
            BoardEvaluation[] boardEvaluations = EvaluateBoard(chessBoard);
            var material = boardEvaluations[_chessBoard.CurrentColor].Material - _boardEvaluations[_chessBoard.CurrentColor].Material;
            var posValues = boardEvaluations[_chessBoard.CurrentColor].GetPositionSummary() - _boardEvaluations[_chessBoard.CurrentColor].GetPositionSummary();
            _logger?.LogDebug($"    Material: {material}  Position: {posValues}");
            if (posValues > 0)
            {
        //        _logger?.LogDebug($"   Oops");
            }

            var compareBoard = material + posValues;
            if (compareBoard == 80)
            {
                _logger?.LogDebug($"Wait");
            }
            return compareBoard;
        }

        private int Evaluate(IChessBoard chessBoard, int maxDeep, bool mini)
        {
            maxDeep--;
            string filler = ".".PadLeft(3 - maxDeep,'.');
            //_logger?.LogDebug($"{filler}MaxDeep: {maxDeep} Mini: {mini}");
            if (maxDeep > 0)
            {
                int maxValue =  mini ? int.MaxValue : int.MinValue;
                var currentMoveList = chessBoard.GenerateMoveList();
                foreach (var move in currentMoveList)
                {
                  //  _logger?.LogDebug($"{filler}{move}");
                    var localChessBoard = new ChessBoard();
                    localChessBoard.Init(chessBoard);
                    localChessBoard.MakeMove(move);
                    localChessBoard.GenerateMoveList();
                    move.Value = Evaluate(localChessBoard, maxDeep, !mini);
                    if (mini)
                    {
                        if (move.Value < maxValue)
                        {
                            maxValue = move.Value;
                        }
                    }
                    else
                    {
                        if (move.Value > maxValue)
                        {
                            maxValue = move.Value;
                        }
                    }

                 
                    _logger?.LogDebug($"{filler}{move}");
                }
              //  _logger?.LogDebug($"{filler}return {maxValue}");
                return maxValue;
            }

            var compareBoard = CompareBoard(chessBoard);
           // _logger?.LogDebug($"{filler}MaxDeep: {maxDeep} Compare return: {compareBoard}");
            return compareBoard;

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
                        .FigurePositionValues[chessFigure.GeneralFigureId] = new List<int>() { positionValue };
                }
            }

            return boardEvaluations;
        }
    }
}

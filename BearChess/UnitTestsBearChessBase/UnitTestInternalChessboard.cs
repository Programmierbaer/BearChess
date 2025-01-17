using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestInternalChessboard
    {
        [TestMethod]
        public void FenCodeTests()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.SetPosition("8/8/8/kQ5p/7n/3B1Kb1/1R6/4q3 w - - 0 1");
            chessBoard.GenerateMoveList();
            var whiteIsInCheck = chessBoard.IsInCheck(Fields.COLOR_WHITE);
            var blackIsInCheck = chessBoard.IsInCheck(Fields.COLOR_BLACK);
            Assert.IsTrue(whiteIsInCheck, "White is not in check");    
            Assert.IsTrue(blackIsInCheck, "Black is not in check");    
            
        }
        [TestMethod]
        public void FastChessBoardTest()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("E2", "E4");
            chessBoard.MakeMove("E7", "E5");
            chessBoard.MakeMove("G1", "F3");
            chessBoard.MakeMove("B8", "C6");
            chessBoard.MakeMove("F1", "C4");
            chessBoard.MakeMove("F8", "C5");
            chessBoard.MakeMove("E1", "G1");
            chessBoard.GetFenPosition();
            var fastChessBoard = new FastChessBoard();
            var fastChessBoard2 = new FastChessBoard();
            fastChessBoard.SetMove("E2E4");
            fastChessBoard.SetMove("E7E5");
            fastChessBoard.SetMove("G1F3");
            fastChessBoard.SetMove("B8C6");
            fastChessBoard.SetMove("F1C4");
            fastChessBoard.SetMove("F8C5");
            fastChessBoard.SetMove("E1G1");
            var positionHashCode1 = fastChessBoard.GetPositionHashCode();
            fastChessBoard2.Init(chessBoard.GetFenPosition(), Array.Empty<string>());
            var positionHashCode2 = fastChessBoard2.GetPositionHashCode();
            Assert.AreEqual(positionHashCode1, positionHashCode2, $"Not equal {positionHashCode1} to {positionHashCode2}");
        }
    }
}
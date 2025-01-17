using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessTools;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestMaterial
    {
        [TestMethod]
        public void AfterMoves3()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("E2","E4");
            chessBoard.MakeMove("F7","F5");
            chessBoard.MakeMove("E4","F5");
            var geTopBottomLineForMoves = MaterialHelper.GeTopBottomLineForMoves(chessBoard.GetPlayedMoveList());
            Assert.AreEqual("p",geTopBottomLineForMoves.BlackLine);
            Assert.AreEqual("-",geTopBottomLineForMoves.WhiteLine);
        }

        [TestMethod]
        public void AfterMoves5()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("E2", "E4");
            chessBoard.MakeMove("F7", "F5");
            chessBoard.MakeMove("E4", "F5");
            chessBoard.MakeMove("G7", "G6");
            chessBoard.MakeMove("F5", "G6");
            var geTopBottomLineForMoves = MaterialHelper.GeTopBottomLineForMoves(chessBoard.GetPlayedMoveList());
            Assert.AreEqual("pp", geTopBottomLineForMoves.BlackLine);
            Assert.AreEqual("--", geTopBottomLineForMoves.WhiteLine);
        }
        [TestMethod]
        public void AfterMoves6()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("E2", "E4");
            chessBoard.MakeMove("F7", "F5");
            chessBoard.MakeMove("E4", "F5");
            chessBoard.MakeMove("G7", "G6");
            chessBoard.MakeMove("F5", "G6");
            chessBoard.MakeMove("H7", "G6");
            var geTopBottomLineForMoves = MaterialHelper.GeTopBottomLineForMoves(chessBoard.GetPlayedMoveList());
            Assert.AreEqual("pp", geTopBottomLineForMoves.BlackLine, "Invalid top line");
            Assert.AreEqual("P-", geTopBottomLineForMoves.WhiteLine,"Invalid bottom line");
        }

    }
}
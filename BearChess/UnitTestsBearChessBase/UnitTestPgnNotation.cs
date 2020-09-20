using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestPgnNotation
    {
        [TestMethod]
        public void TestMoves()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("e4");
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", chessBoard.GetFenPosition());
            chessBoard.MakeMove("e5");
            Assert.AreEqual("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Nf3");
            Assert.AreEqual("rnbqkbnr/pppp1ppp/8/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Nc6");
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 2 3", chessBoard.GetFenPosition());
            chessBoard.MakeMove("d4");
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/4p3/3PP3/5N2/PPP2PPP/RNBQKB1R b KQkq d3 0 3", chessBoard.GetFenPosition());
            chessBoard.MakeMove("exd4");
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/8/3pP3/5N2/PPP2PPP/RNBQKB1R w KQkq - 0 4", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Nxd4");
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/8/3NP3/8/PPP2PPP/RNBQKB1R b KQkq - 0 4", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Nxd4");
            Assert.AreEqual("r1bqkbnr/pppp1ppp/8/8/3nP3/8/PPP2PPP/RNBQKB1R w KQkq - 0 5", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Qxd4");
            Assert.AreEqual("r1bqkbnr/pppp1ppp/8/8/3QP3/8/PPP2PPP/RNB1KB1R b KQkq - 0 5", chessBoard.GetFenPosition());
            chessBoard.MakeMove("d6");
            Assert.AreEqual("r1bqkbnr/ppp2ppp/3p4/8/3QP3/8/PPP2PPP/RNB1KB1R w KQkq - 0 6", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Bc4");
            Assert.AreEqual("r1bqkbnr/ppp2ppp/3p4/8/2BQP3/8/PPP2PPP/RNB1K2R b KQkq - 1 6", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Nf6");
            Assert.AreEqual("r1bqkb1r/ppp2ppp/3p1n2/8/2BQP3/8/PPP2PPP/RNB1K2R w KQkq - 2 7", chessBoard.GetFenPosition());
            chessBoard.MakeMove("0-0");
            Assert.AreEqual("r1bqkb1r/ppp2ppp/3p1n2/8/2BQP3/8/PPP2PPP/RNB2RK1 b kq - 3 7", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Bg4");
            Assert.AreEqual("r2qkb1r/ppp2ppp/3p1n2/8/2BQP1b1/8/PPP2PPP/RNB2RK1 w kq - 4 8", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Nc3");
            Assert.AreEqual("r2qkb1r/ppp2ppp/3p1n2/8/2BQP1b1/2N5/PPP2PPP/R1B2RK1 b kq - 5 8", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Qd7");
            Assert.AreEqual("r3kb1r/pppq1ppp/3p1n2/8/2BQP1b1/2N5/PPP2PPP/R1B2RK1 w kq - 6 9", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Bg5");
            Assert.AreEqual("r3kb1r/pppq1ppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R4RK1 b kq - 7 9", chessBoard.GetFenPosition());
            chessBoard.MakeMove("0-0-0");
            Assert.AreEqual("2kr1b1r/pppq1ppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R4RK1 w - - 8 10", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Rfe1");
            Assert.AreEqual("2kr1b1r/pppq1ppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R3R1K1 b - - 9 10", chessBoard.GetFenPosition());
            chessBoard.MakeMove("Be7");
            Assert.AreEqual("2kr3r/pppqbppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R3R1K1 w - - 10 11", chessBoard.GetFenPosition());

            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("e4");
            chessBoard.MakeMove("c6");
            chessBoard.MakeMove("d4");
            chessBoard.MakeMove("d5");
            chessBoard.MakeMove("Nc3");
            chessBoard.MakeMove("dxe4");
            chessBoard.MakeMove("Ne4");
            chessBoard.MakeMove("Nd7");
            chessBoard.MakeMove("Ng5");
            chessBoard.MakeMove("Ngf6");
            chessBoard.MakeMove("Bd3");
            chessBoard.MakeMove("e6");
            chessBoard.MakeMove("N1f3");
            Assert.AreEqual("r1bqkb1r/pp1n1ppp/2p1pn2/6N1/3P4/3B1N2/PPP2PPP/R1BQK2R b KQkq - 1 7", chessBoard.GetFenPosition());

            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("e4");
            chessBoard.MakeMove("c5");
            chessBoard.MakeMove("e5");
            chessBoard.MakeMove("d5");
            Assert.AreEqual("rnbqkbnr/pp2pppp/8/2ppP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3", chessBoard.GetFenPosition());
            chessBoard.MakeMove("ed6");
            Assert.AreEqual("rnbqkbnr/pp2pppp/3P4/2p5/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 3", chessBoard.GetFenPosition());

            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakeMove("e4");
            chessBoard.MakeMove("e5");
            chessBoard.MakeMove("Nc3");
            chessBoard.MakeMove("f5");
            chessBoard.MakeMove("Qh5+");
            Assert.AreEqual("rnbqkbnr/pppp2pp/8/4pp1Q/4P3/2N5/PPPP1PPP/R1B1KBNR b KQkq - 1 3", chessBoard.GetFenPosition());


            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.SetPosition("8/P3k3/8/8/8/8/8/4K3 w - - 0 1 ");
            chessBoard.MakeMove("a8N");
            Assert.AreEqual("N7/4k3/8/8/8/8/8/4K3 b - - 0 1", chessBoard.GetFenPosition());
            chessBoard.SetPosition("4k3/P7/8/8/8/8/8/4K3 w - - 0 1 ");
            chessBoard.MakeMove("a8Q+");
            Assert.AreEqual("Q3k3/8/8/8/8/8/8/4K3 b - - 0 1", chessBoard.GetFenPosition());
        }

    }
}

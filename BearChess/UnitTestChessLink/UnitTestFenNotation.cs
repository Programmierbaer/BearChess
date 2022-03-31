using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace UnitTestChessLink
{
    [TestClass]
    public class UnitTestFenNotation
    {
        [TestMethod]
        public void TestFenToChessLink()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            string fen = chessBoard.GetFenPosition();
            var chessLinkFen = FenConversions.GetChessLinkFen(fen);
            Assert.AreEqual("rnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR330000000001", chessLinkFen);
            chessBoard.MakeMove("E2","E4");
            chessBoard.MakeMove("E7","E5");
            fen = chessBoard.GetFenPosition();
            chessLinkFen = FenConversions.GetChessLinkFen(fen);
            Assert.AreEqual("rnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR330000000001", chessLinkFen);
            // grnbqkbnrpppp.ppp............p.......P...........PPPP.PPPRNBQKBNR33
            chessBoard.NewGame();
            chessBoard.MakeMove("E2", "E4");
            chessBoard.MakeMove("C7", "C5");
            chessBoard.MakeMove("E4", "E5");
            chessBoard.MakeMove("D7", "D5");
            fen = chessBoard.GetFenPosition();
            chessLinkFen = FenConversions.GetChessLinkFen(fen);
         
            Assert.AreEqual("rnbqkbnrpp..pppp..........ppP...................PPPP.PPPRNBQKBNR335300000003", chessLinkFen);
            // grnbqkbnrpp..pppp..........ppP...................PPPP.PPPRNBQKBNR3353000000036

            chessBoard.NewGame();
            chessBoard.MakeMove("B2", "B4");
            chessBoard.MakeMove("E7", "E5");
            chessBoard.MakeMove("C2", "C4");
            chessBoard.MakeMove("E5", "E4");
            chessBoard.MakeMove("D2", "D4");
            fen = chessBoard.GetFenPosition();
            chessLinkFen = FenConversions.GetChessLinkFen(fen);
            Assert.AreEqual("rnbqkbnrpppp.ppp.................PPPp...........P...PPPPRNBQKBNR3B2300000003", chessLinkFen);
            //rnbqkbnrpppp.ppp.................PPPp...........P...PPPPRNBQKBNR3B2300000003

        }

        [TestMethod]
        public void TestChessLinkToFen()
        {
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();
            string fen = chessBoard.GetFenPosition();
            var strings = fen.Split(" ".ToCharArray());
            var chessLinkFen = FenConversions.GetPiecesFen("srnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR", false,true);
            Assert.AreEqual(strings[0], chessLinkFen);

        }
    }
}

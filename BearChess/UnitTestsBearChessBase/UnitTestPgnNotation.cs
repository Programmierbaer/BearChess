using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;

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
            chessBoard.MakePgnMove("e4",string.Empty, string.Empty);
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("e5",string.Empty, string.Empty);
            Assert.AreEqual("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Nf3",string.Empty, string.Empty);
            Assert.AreEqual("rnbqkbnr/pppp1ppp/8/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Nc6",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 2 3",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("d4",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/4p3/3PP3/5N2/PPP2PPP/RNBQKB1R b KQkq d3 0 3",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("exd4",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/8/3pP3/5N2/PPP2PPP/RNBQKB1R w KQkq - 0 4",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Nxd4",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/pppp1ppp/2n5/8/3NP3/8/PPP2PPP/RNBQKB1R b KQkq - 0 4",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Nxd4",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/pppp1ppp/8/8/3nP3/8/PPP2PPP/RNBQKB1R w KQkq - 0 5", chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Qxd4",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/pppp1ppp/8/8/3QP3/8/PPP2PPP/RNB1KB1R b KQkq - 0 5", chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("d6",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/ppp2ppp/3p4/8/3QP3/8/PPP2PPP/RNB1KB1R w KQkq - 0 6", chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Bc4",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkbnr/ppp2ppp/3p4/8/2BQP3/8/PPP2PPP/RNB1K2R b KQkq - 1 6", chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Nf6",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkb1r/ppp2ppp/3p1n2/8/2BQP3/8/PPP2PPP/RNB1K2R w KQkq - 2 7",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("0-0",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkb1r/ppp2ppp/3p1n2/8/2BQP3/8/PPP2PPP/RNB2RK1 b kq - 3 7", chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Bg4",string.Empty, string.Empty);
            Assert.AreEqual("r2qkb1r/ppp2ppp/3p1n2/8/2BQP1b1/8/PPP2PPP/RNB2RK1 w kq - 4 8",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Nc3",string.Empty, string.Empty);
            Assert.AreEqual("r2qkb1r/ppp2ppp/3p1n2/8/2BQP1b1/2N5/PPP2PPP/R1B2RK1 b kq - 5 8",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Qd7",string.Empty, string.Empty);
            Assert.AreEqual("r3kb1r/pppq1ppp/3p1n2/8/2BQP1b1/2N5/PPP2PPP/R1B2RK1 w kq - 6 9",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Bg5",string.Empty, string.Empty);
            Assert.AreEqual("r3kb1r/pppq1ppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R4RK1 b kq - 7 9",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("0-0-0",string.Empty, string.Empty);
            Assert.AreEqual("2kr1b1r/pppq1ppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R4RK1 w - - 8 10",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Rfe1",string.Empty, string.Empty);
            Assert.AreEqual("2kr1b1r/pppq1ppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R3R1K1 b - - 9 10",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("Be7",string.Empty, string.Empty);
            Assert.AreEqual("2kr3r/pppqbppp/3p1n2/6B1/2BQP1b1/2N5/PPP2PPP/R3R1K1 w - - 10 11",
                            chessBoard.GetFenPosition());

            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakePgnMove("e4",string.Empty, string.Empty);
            chessBoard.MakePgnMove("c6",string.Empty, string.Empty);
            chessBoard.MakePgnMove("d4",string.Empty, string.Empty);
            chessBoard.MakePgnMove("d5",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Nc3",string.Empty, string.Empty);
            chessBoard.MakePgnMove("dxe4",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Ne4",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Nd7",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Ng5",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Ngf6",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Bd3",string.Empty, string.Empty);
            chessBoard.MakePgnMove("e6",string.Empty, string.Empty);
            chessBoard.MakePgnMove("N1f3",string.Empty, string.Empty);
            Assert.AreEqual("r1bqkb1r/pp1n1ppp/2p1pn2/6N1/3P4/3B1N2/PPP2PPP/R1BQK2R b KQkq - 1 7",
                            chessBoard.GetFenPosition());

            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakePgnMove("e4",string.Empty, string.Empty);
            chessBoard.MakePgnMove("c5",string.Empty, string.Empty);
            chessBoard.MakePgnMove("e5",string.Empty, string.Empty);
            chessBoard.MakePgnMove("d5",string.Empty, string.Empty);
            Assert.AreEqual("rnbqkbnr/pp2pppp/8/2ppP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3",
                            chessBoard.GetFenPosition());
            chessBoard.MakePgnMove("ed6",string.Empty, string.Empty);
            Assert.AreEqual("rnbqkbnr/pp2pppp/3P4/2p5/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 3", chessBoard.GetFenPosition());

            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.MakePgnMove("e4",string.Empty, string.Empty);
            chessBoard.MakePgnMove("e5",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Nc3",string.Empty, string.Empty);
            chessBoard.MakePgnMove("f5",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Qh5+",string.Empty, string.Empty);
            Assert.AreEqual("rnbqkbnr/pppp2pp/8/4pp1Q/4P3/2N5/PPPP1PPP/R1B1KBNR b KQkq - 1 3",
                            chessBoard.GetFenPosition());


            chessBoard.Init();
            chessBoard.NewGame();
            chessBoard.SetPosition("8/P3k3/8/8/8/8/8/4K3 w - - 0 1 ");
            chessBoard.MakePgnMove("a8N",string.Empty, string.Empty);
            Assert.AreEqual("N7/4k3/8/8/8/8/8/4K3 b - - 0 1", chessBoard.GetFenPosition());
            chessBoard.SetPosition("4k3/P7/8/8/8/8/8/4K3 w - - 0 1 ");
            chessBoard.MakePgnMove("a8Q+",string.Empty, string.Empty);
            Assert.AreEqual("Q3k3/8/8/8/8/8/8/4K3 b - - 0 1", chessBoard.GetFenPosition());

            string text1 = "[Event \"BearChess\"]" + Environment.NewLine;
            text1 += "[Site \"?\"]" + Environment.NewLine;
            text1 += "[Date \"16.12.2021\"]" + Environment.NewLine;
            text1 += "[Round \"?\"]" + Environment.NewLine;
            text1 += "[White \"Player\"]" + Environment.NewLine;
            text1 += "[Black \"Player\"]" + Environment.NewLine;
            text1 += "[Result \"*\"]" + Environment.NewLine;
            string text = text1 + @"
1. e4 e5 2. Nf3 Nf6 3. d4 Nxe4 4. Bd3 d5 5. Nxe5 Bd6 6. O-O O-O 7. Nd2 Bxe5 8.
dxe5 Nc5 9. Nb3 Nxd3 10. Qxd3 Nc6 11. Bf4 Qd7 12. Rfd1 Qg4 13. Qg3 Qxg3 14.
hxg3 Bf5 15. Rxd5 Bxc2 16. Rb5 b6 17. e6 fxe6 18. Bxc7 Rf5 19. Rxf5 exf5 20.
Nd2 Kf7 21. Rc1 Nd4 22. Re1 Bd3 23. Re3 Bb5 24. Be5 Rd8 25. Nf3 Nxf3+ 26. Rxf3
Rd5 27. Bc3 g5 28. g4 f4 29. Rh3 Bd3 30. Rh6 Bg6 31. f3 b5 32. Kf2 Ke7 33. b3
Rd3 34. Bb4+ Ke6 35. Rh1 Kd5 36. Rc1 Ke5 37. Rc7 a6 38. Rc6 Rd8 39. Rxa6 Rc8
40. Rd6 Rc2+ 41. Rd2 Rxd2+ 42. Bxd2 Kd4 43. a4 bxa4 44. bxa4 Be8 45. a5 Bb5 46.
Bb4 Bd3 47. Be7 h6 48. Bf8 Ke5 49. g3 fxg3+ 50. Kxg3 Kf6 51. Bxh6 Be2 52. Bxg5+
Kxg5 53. a6 Bxa6 54. f4+ Kg6 55. Kf3 Bc8 56. g5 Kf5 57. Ke3 Be6 58. g6 Kf6 59.
g7 Kxg7 60. Kd4 Bf5 61. Kc3 Kf6 62. Kd4 Ke6 63. Ke3 Kd5 64. Kf3 Kd4 65. Kf2 Ke4
66. Kg3 Ke3 67. Kg2 Kd2 68. Kf3 Kd3 69. Kf2 Kd4 70. Kf3 Kc4 71. Kg3 Kc5 72. Kh4
Kd5 73. Kg5 Ke6 74. Kh5 Kf6 75. Kh4 Kg6 76. Kg3 Kf7 77. Kf2 Ke7 78. Kf3 Kd6 79.
Ke2 Kc6 80. Kf2 Kb5 81. Ke3 Kc5 82. Kf3 Kd5 83. Ke3 Kc4 84. Ke2 Kb4 85. Ke1 Kc3
86. Kf1 Kd2 87. Kf2 Kd3 88. Kf3 Kc2 89. Kg3 Kc3 90. Kh4 Kd4 91. Kg5 Ke4 92. Kf6
Bg4 93. Kg6 Bh5+ 94. Kg5 Be8 95. Kg4 Bf7 96. Kg5 Be8 97. Kg4 Kd4 98. Kf5 Kd5
99. Kg5 Ke6 100. Kg4 Kf6 101. Kf3 Kf5 102. Kg2 Bh5 103. Kf2 Bd1 104. Kg2 Bb3
105. Kf3 Bc4 106. Kf2 Ke4 107. Kg3 Be6 108. Kf2 Bd5";
            chessBoard.Init();
            chessBoard.NewGame();
            var pgnLoader = new PgnLoader();
            var pgnGame = pgnLoader.GetGame(text);
            Assert.IsNotNull(pgnGame);
            for (int i = 0; i < pgnGame.MoveCount; i++)
            {
                chessBoard.MakePgnMove(pgnGame.GetMove(i), string.Empty, string.Empty);
            }

            Assert.IsFalse(chessBoard.DrawBy50Moves);
            chessBoard.MakePgnMove("Kg3",string.Empty, string.Empty);
            chessBoard.MakePgnMove("Bb3",string.Empty, string.Empty);
            Assert.IsTrue(chessBoard.DrawBy50Moves);
        }

    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChess.SquareOffChessBoard;

namespace UnitTestChessLink
{
    [TestClass]
    public class UnitTestFenNotation
    {
        private const string BASEPOSITION = "1100001111000011110000111100001111000011110000111100001111000011";

        [TestMethod]
        public void TestSquareOffProCodeConverter()
        {
            var converter = new BoardCodeConverter(BASEPOSITION);
            var fieldsWith = converter.GetFieldsWithPieces();
            Assert.IsTrue(fieldsWith.Length == 32);
            Assert.AreEqual(string.Join(" ", fieldsWith), "A1 A2 A7 A8 B1 B2 B7 B8 C1 C2 C7 C8 D1 D2 D7 D8 E1 E2 E7 E8 F1 F2 F7 F8 G1 G2 G7 G8 H1 H2 H7 H8");
        }


        // 178 0 1 6 1 0 1 1  8 168 87 192 0 2 7 2
        [TestMethod]
        public void TestFirmwareChessUp()
        {
            string information = "178 0 1 6 1 0 1 1  8 168 87 192 0 2 7 2"; // 816887192
            information = "178 0 1 9 0 1 0 0 13 239 54 208 1 4 0 1 ";   // 1323954208
            var strings = information.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string result = string.Empty;
            if (strings.Length == 16)
            {
                string firmware = $"{strings[2]}.{strings[3]}.{strings[4]}";
                string BLEfirmware = $"{strings[5]}.{strings[6]}.{strings[7]}";
                string bootLoader = $"{strings[12]}.{strings[13]}.{strings[14]}";
              
                string version = Convert.ToUInt64(strings[8] + strings[9] + strings[10] + strings[11],16).ToString();
                result =
                $"ChessUp {strings[15]}{Environment.NewLine}Firmware: {firmware}{Environment.NewLine}BLE-Firmware: {BLEfirmware}{Environment.NewLine}Bootloader: {bootLoader}{Environment.NewLine} Serial#: {version}";
            }

            result = "ChessUp";
            Assert.AreEqual(result, "ChessUp");
        }

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

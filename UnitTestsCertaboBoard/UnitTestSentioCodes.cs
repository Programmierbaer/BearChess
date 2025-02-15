using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.ChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace UnitTestsCertaboBoard
{
    [TestClass]
    public class UnitTestSentioCodes
    {
        [TestMethod]
        public void TestBoardPositions()
        {
            var boardCodeConverter = new BoardCodeConverter(true);
            boardCodeConverter.SetFigureOn(Fields.FE2);
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE2));
            Assert.IsFalse(boardCodeConverter.SamePosition(new BoardCodeConverter(true)));
            boardCodeConverter.ClearFields();
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE2));
            Assert.IsTrue(boardCodeConverter.SamePosition(new BoardCodeConverter(true)));

            boardCodeConverter.SetFigureOn(Fields.FA1);
            boardCodeConverter.SetFigureOn(Fields.FB1);
            boardCodeConverter.SetFigureOn(Fields.FC1);
            boardCodeConverter.SetFigureOn(Fields.FD1);
            boardCodeConverter.SetFigureOn(Fields.FE1);
            boardCodeConverter.SetFigureOn(Fields.FF1);
            boardCodeConverter.SetFigureOn(Fields.FG1);
            boardCodeConverter.SetFigureOn(Fields.FH1);
            boardCodeConverter.SetFigureOn(Fields.FA2);
            boardCodeConverter.SetFigureOn(Fields.FB2);
            boardCodeConverter.SetFigureOn(Fields.FC2);
            boardCodeConverter.SetFigureOn(Fields.FD2);
            boardCodeConverter.SetFigureOn(Fields.FE2);
            boardCodeConverter.SetFigureOn(Fields.FF2);
            boardCodeConverter.SetFigureOn(Fields.FG2);
            boardCodeConverter.SetFigureOn(Fields.FH2);
            boardCodeConverter.SetFigureOn(Fields.FA8);
            boardCodeConverter.SetFigureOn(Fields.FB8);
            boardCodeConverter.SetFigureOn(Fields.FC8);
            boardCodeConverter.SetFigureOn(Fields.FD8);
            boardCodeConverter.SetFigureOn(Fields.FE8);
            boardCodeConverter.SetFigureOn(Fields.FF8);
            boardCodeConverter.SetFigureOn(Fields.FG8);
            boardCodeConverter.SetFigureOn(Fields.FH8);
            boardCodeConverter.SetFigureOn(Fields.FA7);
            boardCodeConverter.SetFigureOn(Fields.FB7);
            boardCodeConverter.SetFigureOn(Fields.FC7);
            boardCodeConverter.SetFigureOn(Fields.FD7);
            boardCodeConverter.SetFigureOn(Fields.FE7);
            boardCodeConverter.SetFigureOn(Fields.FF7);
            boardCodeConverter.SetFigureOn(Fields.FG7);
            boardCodeConverter.SetFigureOn(Fields.FH7);

            Assert.IsTrue(boardCodeConverter.SamePosition(new BoardCodeConverter(new[] { "255", "255", "0", "0", "0", "0", "255", "255" }, true)));

            // 255 255 0 0 0 0 247 255 
            boardCodeConverter = new BoardCodeConverter(new[] {"255","255","0","0","0","0","255","255"}, true);
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH7));

            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH6));

            boardCodeConverter = new BoardCodeConverter(new[] { "255", "255", "0", "0", "8", "0", "247", "255" }, true);
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH1));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD2));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH2));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH8));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FA7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FB7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FC7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FD7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FF7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FG7));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FH7));

            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH3));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD4));
            Assert.IsTrue(boardCodeConverter.IsFigureOn(Fields.FE4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH4));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH5));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FA6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FB6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FC6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FD6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FE6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FF6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FG6));
            Assert.IsFalse(boardCodeConverter.IsFigureOn(Fields.FH6));

        }
    }
}
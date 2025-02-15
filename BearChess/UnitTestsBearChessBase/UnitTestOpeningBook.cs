using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestOpeningBook
    {
        [TestMethod]
        public void VariationTest()
        {
            PolyglotReader polyglotReader = new PolyglotReader();
            polyglotReader.ReadFile(@"..\..\Perfect2017.bin");
            var openingBook = new OpeningBook();
            Assert.IsTrue(openingBook.LoadBook(@"..\..\Perfect2017.bin", true));
            openingBook.SetVariation("best move");
            var candidateMoveList = openingBook.GetCandidateMoveList();
            Assert.IsTrue(candidateMoveList.Length == 1);
            var bookMove = openingBook.GetMove(new string[0]);
            Assert.AreEqual("e2",bookMove.FromField);
            Assert.AreEqual("e4",bookMove.ToField);
            openingBook.SetVariation("flexible");
            candidateMoveList = openingBook.GetCandidateMoveList();
            Assert.IsTrue(candidateMoveList.Length==96);
            bool found = false;
            foreach (var move in candidateMoveList)
            {
                if (move.FromField.Equals("b2") && move.ToField.Equals("b3"))
                {
                    found = true;
                }
            }
            Assert.IsFalse(found);

            openingBook.SetVariation("wide");
            candidateMoveList = openingBook.GetCandidateMoveList();
            Assert.IsTrue(candidateMoveList.Length == 17);
            found = false;
            foreach (var move in candidateMoveList)
            {
                if (move.FromField.Equals("b2") && move.ToField.Equals("b3"))
                {
                    found = true;
                }
            }
            Assert.IsTrue(found);
        }
    }
}
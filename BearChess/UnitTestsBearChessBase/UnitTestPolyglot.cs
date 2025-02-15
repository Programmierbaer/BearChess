using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestPolyglot
    {
        
        [TestMethod]
        public void GetKeyTests()
        {
            PolyglotReader polyglotReader = new PolyglotReader();
      
            // starting position
            string key = polyglotReader.GetKey("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1").ToString("X").ToLower();
            Assert.AreEqual("463b96181691fc9c",key);
            var strings = polyglotReader.GetMoves("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            // position after e2e4
            key = polyglotReader.GetKey("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1").ToString("X").ToLower();
            Assert.AreEqual("823c9b50fd114196", key);

            // position after e2e4 d7d5
            key = polyglotReader.GetKey("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2").ToString("X").ToLower();
            Assert.AreEqual("756b94461c50fb0", key);

            // position after e2e4 d7d5 e4e5
            key = polyglotReader.GetKey("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2").ToString("X").ToLower();
            Assert.AreEqual("662fafb965db29d4", key);

            //position after e2e4 d7d5 e4e5 f7f5
            key = polyglotReader.GetKey("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3").ToString("X").ToLower();
            Assert.AreEqual("22a48b5a8e47ff78", key);

            // position after e2e4 d7d5 e4e5 f7f5 e1e2
            key = polyglotReader.GetKey("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR b kq - 0 3").ToString("X").ToLower();
            Assert.AreEqual("652a607ca3f242c1", key);
            
            // position after e2e4 d7d5 e4e5 f7f5 e1e2 e8f7
            key = polyglotReader.GetKey("rnbq1bnr/ppp1pkpp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR w - - 0 4").ToString("X").ToLower();
            Assert.AreEqual("fdd303c946bdd9", key);

            // position after a2a4 b7b5 h2h4 b5b4 c2c4
            key = polyglotReader.GetKey("rnbqkbnr/p1pppppp/8/8/PpP4P/8/1P1PPPP1/RNBQKBNR b KQkq c3 0 3").ToString("X").ToLower();
            Assert.AreEqual("3c8123ea7b067637", key);

            // position after a2a4 b7b5 h2h4 b5b4 c2c4 b4c3 a1a3
            key = polyglotReader.GetKey("rnbqkbnr/p1pppppp/8/8/P6P/R1p5/1P1PPPP1/1NBQKBNR b Kkq - 0 4").ToString("X").ToLower();
            Assert.AreEqual("5c3f9b829b279560", key);

        }
    }
}

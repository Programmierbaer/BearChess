using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestCTG
    {
        [TestMethod]
        public void GetMoveTests()
        {
            var ctgReader = new CTGReader();
            //ctgReader.ReadFile(@"c:\books\Perfect2023.ctg");
            ctgReader.ReadFile(@"c:\books\StrongBook 2022.ctg");
            var bookMoveBases = ctgReader.GetMoves("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            // var bookMoveBases = ctgReader.GetMoves("r1bqkbnr/pppp1ppp/2n5/4p3/3PP3/5N2/PPP2PPP/RNBQKB1R b KQkq - 0 3");
            // var bookMoveBases = ctgReader.GetMoves("r1bqkbnr/pppp1ppp/2n5/8/3pP3/5N2/PPP2PPP/RNBQKB1R w KQkq - 0 4");
            Assert.IsNotNull(bookMoveBases);
            Assert.IsTrue(bookMoveBases.Length>0);
        }
    }
}
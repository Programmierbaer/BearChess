using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChess.MChessLinkChessBoard;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestEncoding
    {
        [TestMethod]
        public void ReadFromChessLinkBTLE()
        {
            var addOddPar = CRCConversions.AddOddPar("V");
            Assert.AreEqual(214,addOddPar);
            addOddPar = CRCConversions.AddOddPar("v");
            Assert.AreEqual(118,addOddPar);
            var result = 214 & 127;
            Assert.AreEqual(86, result);
            result = 87 & 127;
            Assert.AreEqual(87, result);
        }
    }
}
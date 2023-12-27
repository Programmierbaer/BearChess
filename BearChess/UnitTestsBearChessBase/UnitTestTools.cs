using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestTools
    {
        [TestMethod]
        public void StringExtensions()
        {
            string manySpaces = "  asa    ödl  s      ww ws a ";
            string noSpaces = "asa ödl s ww ws a";
            Assert.AreEqual(noSpaces, manySpaces.RemoveSpaces());
            
        }
    }
}
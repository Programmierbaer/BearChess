using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

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

        [TestMethod]
        public void Excel()
        {
            var excelHelper = new ExcelHelper(@"d:\downloads\BearChessWin\lars.xlsx");
            excelHelper.AddMove("d4",1,(decimal)2.0);
            excelHelper.Close();
        }
    }
}
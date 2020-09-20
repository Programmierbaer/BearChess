using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestFields
    {
        [TestMethod]
        public void FirstTest()
        {
            Assert.AreEqual("A1",Fields.GetFieldName(Fields.FA1));

        }
    }
}
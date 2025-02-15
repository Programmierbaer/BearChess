using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace UnitTestFields
{
    [TestClass]
    public class UnitTestFields
    {
        [TestMethod]
        public void FirstTest()
        {
            Assert.That(Fields.GetFieldName(Fields.FA1).Equals("A1"));

        }
    }
}

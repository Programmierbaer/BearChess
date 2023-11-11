using Microsoft.VisualStudio.TestTools.UnitTesting;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace UnitTestsBearChessBase
{
    [TestClass]
    public class UnitTestFields
    {
        [TestMethod]
        public void TestFieldChangeHelper()
        {
            Assert.AreEqual("A1", Fields.GetFieldName(Fields.FA1));
            var fieldChanges = FieldChangeHelper.GetFieldChanges(string.Empty, string.Empty);
            Assert.IsTrue(fieldChanges.AddedFields.Length==0);
            Assert.IsTrue(fieldChanges.RemovedFields.Length==0);
            fieldChanges = FieldChangeHelper.GetFieldChanges("A1,A2,A7,A8,B1,B2,B7,B8,C1,C2,C7,C8,D1,D2,D7,D8,E1,E2,E7,E8,F1,F2,F7,F8,G1,G2,G7,G8,H1,H2,H7,H8",
                                                             "A1,A2,A7,A8,B1,B2,B7,B8,C1,C2,C7,C8,D1,D2,D7,D8,E1,E7,E8,F1,F2,F7,F8,G1,G2,G7,G8,H1,H2,H7,H8");
            Assert.IsTrue(fieldChanges.RemovedFields.Length == 1);
            Assert.IsTrue(fieldChanges.RemovedFields[0] == "E2");
            Assert.IsTrue(fieldChanges.AddedFields.Length == 0);

            fieldChanges = FieldChangeHelper.GetFieldChanges("E2,E4", string.Empty);
            Assert.IsTrue(fieldChanges.RemovedFields.Length == 2);
            Assert.IsTrue(fieldChanges.RemovedFields[0] == "E2");
            Assert.IsTrue(fieldChanges.RemovedFields[1] == "E4");
            Assert.IsTrue(fieldChanges.AddedFields.Length == 0);

            fieldChanges = FieldChangeHelper.GetFieldChanges(string.Empty,"E2,E4");
            Assert.IsTrue(fieldChanges.AddedFields.Length == 2);
            Assert.IsTrue(fieldChanges.AddedFields[0] == "E2");
            Assert.IsTrue(fieldChanges.AddedFields[1] == "E4");
            Assert.IsTrue(fieldChanges.RemovedFields.Length == 0);

            fieldChanges = FieldChangeHelper.GetFieldChanges("E2,E4", "D2,D4");
            Assert.IsTrue(fieldChanges.RemovedFields.Length == 2);
            Assert.IsTrue(fieldChanges.RemovedFields[0] == "E2");
            Assert.IsTrue(fieldChanges.RemovedFields[1] == "E4");
            Assert.IsTrue(fieldChanges.AddedFields.Length == 2);
            Assert.IsTrue(fieldChanges.AddedFields[0] == "D2");
            Assert.IsTrue(fieldChanges.AddedFields[1] == "D4");

            fieldChanges = FieldChangeHelper.GetFieldChanges("E2,E4", "D2,E4,D4");
            Assert.IsTrue(fieldChanges.RemovedFields.Length == 1);
            Assert.IsTrue(fieldChanges.RemovedFields[0] == "E2");
            Assert.IsTrue(fieldChanges.AddedFields.Length == 2);
            Assert.IsTrue(fieldChanges.AddedFields[0] == "D2");
            Assert.IsTrue(fieldChanges.AddedFields[1] == "D4");
        }

        [TestMethod]
        public void TestMoveLineHelper()
        {
            var moveLine = MoveLineHelper.GetMoveLine("E1", "E8");
            Assert.IsTrue(moveLine.Length==8);
            Assert.IsTrue(moveLine[0].Equals("E1"));
            Assert.IsTrue(moveLine[1].Equals("E8"));
            Assert.IsTrue(moveLine[2].Equals("E2"));
            Assert.IsTrue(moveLine[3].Equals("E3"));
            Assert.IsTrue(moveLine[4].Equals("E4"));
            Assert.IsTrue(moveLine[5].Equals("E5"));
            Assert.IsTrue(moveLine[6].Equals("E6"));
            Assert.IsTrue(moveLine[7].Equals("E7"));
            moveLine = MoveLineHelper.GetMoveLine("E8", "E1");
            Assert.IsTrue(moveLine.Length == 8);
            Assert.IsTrue(moveLine[0].Equals("E1"));
            Assert.IsTrue(moveLine[1].Equals("E8"));
            Assert.IsTrue(moveLine[2].Equals("E2"));
            Assert.IsTrue(moveLine[3].Equals("E3"));
            Assert.IsTrue(moveLine[4].Equals("E4"));
            Assert.IsTrue(moveLine[5].Equals("E5"));
            Assert.IsTrue(moveLine[6].Equals("E6"));
            Assert.IsTrue(moveLine[7].Equals("E7"));

            moveLine = MoveLineHelper.GetMoveLine("A1", "H1");
            Assert.IsTrue(moveLine.Length == 8);
            Assert.IsTrue(moveLine[0].Equals("A1"));
            Assert.IsTrue(moveLine[1].Equals("H1"));
            Assert.IsTrue(moveLine[2].Equals("B1"));
            Assert.IsTrue(moveLine[3].Equals("C1"));
            Assert.IsTrue(moveLine[4].Equals("D1"));
            Assert.IsTrue(moveLine[5].Equals("E1"));
            Assert.IsTrue(moveLine[6].Equals("F1"));
            Assert.IsTrue(moveLine[7].Equals("G1"));
            
            moveLine = MoveLineHelper.GetMoveLine("H1", "A1");
            Assert.IsTrue(moveLine.Length == 8);
            Assert.IsTrue(moveLine[0].Equals("A1"));
            Assert.IsTrue(moveLine[1].Equals("H1"));
            Assert.IsTrue(moveLine[2].Equals("B1"));
            Assert.IsTrue(moveLine[3].Equals("C1"));
            Assert.IsTrue(moveLine[4].Equals("D1"));
            Assert.IsTrue(moveLine[5].Equals("E1"));
            Assert.IsTrue(moveLine[6].Equals("F1"));
            Assert.IsTrue(moveLine[7].Equals("G1"));
            
            moveLine = MoveLineHelper.GetMoveLine("A1", "H8");
            Assert.IsTrue(moveLine.Length == 8);
            Assert.IsTrue(moveLine[0].Equals("A1"));
            Assert.IsTrue(moveLine[1].Equals("H8"));
            Assert.IsTrue(moveLine[2].Equals("B2"));
            Assert.IsTrue(moveLine[3].Equals("C3"));
            Assert.IsTrue(moveLine[4].Equals("D4"));
            Assert.IsTrue(moveLine[5].Equals("E5"));
            Assert.IsTrue(moveLine[6].Equals("F6"));
            Assert.IsTrue(moveLine[7].Equals("G7"));

            moveLine = MoveLineHelper.GetMoveLine("H8", "A1");
            Assert.IsTrue(moveLine.Length == 8);
            Assert.IsTrue(moveLine[0].Equals("A1"));
            Assert.IsTrue(moveLine[1].Equals("H8"));
            Assert.IsTrue(moveLine[2].Equals("B2"));
            Assert.IsTrue(moveLine[3].Equals("C3"));
            Assert.IsTrue(moveLine[4].Equals("D4"));
            Assert.IsTrue(moveLine[5].Equals("E5"));
            Assert.IsTrue(moveLine[6].Equals("F6"));
            Assert.IsTrue(moveLine[7].Equals("G7"));

            moveLine = MoveLineHelper.GetMoveLine("F1", "B5");
            Assert.IsTrue(moveLine.Length == 5);
            Assert.IsTrue(moveLine[0].Equals("B5"));
            Assert.IsTrue(moveLine[1].Equals("F1"));
            Assert.IsTrue(moveLine[2].Equals("C4"));
            Assert.IsTrue(moveLine[3].Equals("D3"));
            Assert.IsTrue(moveLine[4].Equals("E2"));

            moveLine = MoveLineHelper.GetMoveLine("B5", "F1");
            Assert.IsTrue(moveLine.Length == 5);
            Assert.IsTrue(moveLine[0].Equals("B5"));
            Assert.IsTrue(moveLine[1].Equals("F1"));
            Assert.IsTrue(moveLine[2].Equals("C4"));
            Assert.IsTrue(moveLine[3].Equals("D3"));
            Assert.IsTrue(moveLine[4].Equals("E2"));

            moveLine = MoveLineHelper.GetMoveLine("G1", "F3");
            Assert.IsTrue(moveLine.Length == 3);
            Assert.IsTrue(moveLine[0].Equals("F3"));
            Assert.IsTrue(moveLine[1].Equals("G1"));
            //   Assert.IsTrue(moveLine[2].Equals("F2"));

            moveLine = MoveLineHelper.GetMoveLine("F3", "G1");
            Assert.IsTrue(moveLine.Length == 3);
            Assert.IsTrue(moveLine[0].Equals("F3"));
            Assert.IsTrue(moveLine[1].Equals("G1"));

            moveLine = MoveLineHelper.GetMoveLine("F3", "H4");
            Assert.IsTrue(moveLine.Length == 3);
            Assert.IsTrue(moveLine[0].Equals("F3"));
            Assert.IsTrue(moveLine[1].Equals("H4"));

            moveLine = MoveLineHelper.GetMoveLine("E2", "D3");
            Assert.IsTrue(moveLine.Length == 2,$"Erwartet: 2, Ergebnis: {moveLine.Length}");
            Assert.IsTrue(moveLine[0].Equals("D3"));
            Assert.IsTrue(moveLine[1].Equals("E2"));

        }
    }
}
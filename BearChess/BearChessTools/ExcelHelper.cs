using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public class ExcelHelper
    {
        private readonly string _fileName;
        private XLWorkbook _workbook;

        public ExcelHelper()
        {
            
        }

        public ExcelHelper(string fileName)
        {
            _fileName = fileName;
            if (File.Exists(_fileName))
            {
                _workbook = new XLWorkbook(fileName);
                
            }
        }

        public void Load(string fileName)
        {
            if (File.Exists(_fileName))
            {
                _workbook = new XLWorkbook(fileName);
            }
        }

        public void AddMove(string move, int color, decimal evaluation)
        {
            var xlWorksheet = _workbook.Worksheets.Worksheet("Test");
            xlWorksheet.Cell(6, "B").Value = 2.0;
        }

        public void Close()
        {
            _workbook?.SaveAs(@"d:\downloads\BearChessWin\lars2.xlsx");
        }
    }
}

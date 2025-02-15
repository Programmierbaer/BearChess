using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.SquareOffChessBoard
{
    public class BoardCodeConverter
    {
        private readonly bool _playWithWhite;

        private Dictionary<string, bool> _chessFields = new Dictionary<string, bool>()
        {
            { "A1", false },
            { "A2", false },
            { "A3", false },
            { "A4", false },
            { "A5", false },
            { "A6", false },
            { "A7", false },
            { "A8", false },
            { "B1", false },
            { "B2", false },
            { "B3", false },
            { "B4", false },
            { "B5", false },
            { "B6", false },
            { "B7", false },
            { "B8", false },
            { "C1", false },
            { "C2", false },
            { "C3", false },
            { "C4", false },
            { "C5", false },
            { "C6", false },
            { "C7", false },
            { "C8", false },
            { "D1", false },
            { "D2", false },
            { "D3", false },
            { "D4", false },
            { "D5", false },
            { "D6", false },
            { "D7", false },
            { "D8", false },
            { "E1", false },
            { "E2", false },
            { "E3", false },
            { "E4", false },
            { "E5", false },
            { "E6", false },
            { "E7", false },
            { "E8", false },
            { "F1", false },
            { "F2", false },
            { "F3", false },
            { "F4", false },
            { "F5", false },
            { "F6", false },
            { "F7", false },
            { "F8", false },
            { "G1", false },
            { "G2", false },
            { "G3", false },
            { "G4", false },
            { "G5", false },
            { "G6", false },
            { "G7", false },
            { "G8", false },
            { "H1", false },
            { "H2", false },
            { "H3", false },
            { "H4", false },
            { "H5", false },
            { "H6", false },
            { "H7", false },
            { "H8", false },
        };
        private readonly Dictionary<byte, string> _fieldByte2FieldName = new Dictionary<byte, string>()
        { { 0, "A1" }, { 1, "A2" }, { 2, "A3" }, { 3, "A4" }, { 4, "A5" }, { 5, "A6" }, { 6, "A7" }, { 7, "A8" },
            { 8, "B1" }, { 9, "B2" }, {10, "B3" }, {11, "B4" }, {12, "B5" }, {13, "B6" }, {14, "B7" }, {15, "B8" },
            {16, "C1" }, {17, "C2" }, {18, "C3" }, {19, "C4" }, {20, "C5" }, {21, "C6" }, {22, "C7" }, {23, "C8" },
            {24, "D1" }, {25, "D2" }, {26, "D3" }, {27, "D4" }, {28, "D5" }, {29, "D6" }, {30, "D7" }, {31, "D8" },
            {32, "E1" }, {33, "E2" }, {34, "E3" }, {35, "E4" }, {36, "E5" }, {37, "E6" }, {38, "E7" }, {39, "E8" },
            {40, "F1" }, {41, "F2" }, {42, "F3" }, {43, "F4" }, {44, "F5" }, {45, "F6" }, {46, "F7" }, {47, "F8" },
            {48, "G1" }, {49, "G2" }, {50, "G3" }, {51, "G4" }, {52, "G5" }, {53, "G6" }, {54, "G7" }, {55, "G8" },
            {56, "H1" }, {57, "H2" }, {58, "H3" }, {59, "H4" }, {60, "H5" }, {61, "H6" }, {62, "H7" }, {63, "H8" }
        };

        public BoardCodeConverter(string boardCodes, bool playWithWhite)
        {
            _playWithWhite = playWithWhite;
            for (byte i = 0; i < 64; i++)
            {
                var key = boardCodes.Substring(i, 1);
                _chessFields[_fieldByte2FieldName[i]] = key == "1";
            }
        }

        public BoardCodeConverter(bool playWithWhite)
        {
            _playWithWhite = playWithWhite;
        }

        public bool SamePosition(BoardCodeConverter boardCodeConverter)
        {
            foreach (var chessFieldsKey in _chessFields.Keys)
            {
                if (_chessFields[chessFieldsKey] != boardCodeConverter.IsFigureOn(chessFieldsKey))
                {
                    return false;
                }
            }

            return true;
        }

        public void ClearFields()
        {
            foreach (var chessFieldsKey in _chessFields.Keys)
            {
                _chessFields[chessFieldsKey] = false;
            }
        }

        public void SetFigureOn(int fieldId)
        {
            var fieldName = Fields.GetFieldName(fieldId);
            _chessFields[fieldName] = true;
        }


        public bool IsFigureOn(int fieldId)
        {
            var fieldName = Fields.GetFieldName(fieldId);
            return _chessFields[fieldName];
        }

        public bool IsFigureOn(string fieldName)
        {
            return _chessFields[fieldName.ToUpper()];
        }

    }
}

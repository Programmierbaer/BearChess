using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.ChessBoard
{
    public class BoardCodeConverter
    {
        private readonly bool _playWithWhite;

        private readonly bool[,] _chessFields =
        {
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false }
            
        };

        public BoardCodeConverter(string[] boardCodes, bool playWithWhite)
        {
            _playWithWhite = playWithWhite;

            if (boardCodes.Length != 8)
            {
                return;
            }

            for (int i = 0; i < boardCodes.Length; i++)
            {
                if (byte.TryParse(boardCodes[i], out var code))
                {
                    _chessFields[i, 0] = IsBitSet(code, 0);
                    _chessFields[i, 1] = IsBitSet(code, 1);
                    _chessFields[i, 2] = IsBitSet(code, 2);
                    _chessFields[i, 3] = IsBitSet(code, 3);
                    _chessFields[i, 4] = IsBitSet(code, 4);
                    _chessFields[i, 5] = IsBitSet(code, 5);
                    _chessFields[i, 6] = IsBitSet(code, 6);
                    _chessFields[i, 7] = IsBitSet(code, 7);
                }
            }
        }

        public BoardCodeConverter(bool playWithWhite)
        {
            _playWithWhite = playWithWhite;
        }

        public bool SamePosition(BoardCodeConverter boardCodeConverter)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (_chessFields[i, j] != boardCodeConverter.IsFigureOn(i, j))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void ClearFields()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    _chessFields[i, j] = false;
                }
            }
        }

        public void SetFigureOn(int fieldId)
        {
            var lines = Fields.GetLine(fieldId);
            var row = Fields.GetRow(fieldId);
            var l = _playWithWhite ? 7 - (int)lines : (int)lines;
            var r = _playWithWhite ? 8 - row : row - 1;
            _chessFields[r, l] = true;
        }

        private bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public bool IsFigureOn(int fieldId)
        {
            var lines = Fields.GetLine(fieldId);
            var row = Fields.GetRow(fieldId);
            var l = _playWithWhite ? 7 - (int)lines : (int)lines;
            var r = _playWithWhite ? 8 - row : row - 1;
            return _chessFields[r, l];
        }

        public bool IsFigureOn(string fieldName)
        {
            return IsFigureOn(Fields.GetFieldNumber(fieldName));
        }

        public bool IsFigureOn(int i, int j)
        {
            return _chessFields[i, j];
        }
    }
}
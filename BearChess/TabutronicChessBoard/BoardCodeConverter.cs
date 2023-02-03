using System;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.ChessBoard
{
    public class BoardCodeConverter
    {
        private readonly bool _playWithWhite;

        private readonly bool[,] chessFields =
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
                    chessFields[i, 0] = IsBitSet(code, 0);
                    chessFields[i, 1] = IsBitSet(code, 1);
                    chessFields[i, 2] = IsBitSet(code, 2);
                    chessFields[i, 3] = IsBitSet(code, 3);
                    chessFields[i, 4] = IsBitSet(code, 4);
                    chessFields[i, 5] = IsBitSet(code, 5);
                    chessFields[i, 6] = IsBitSet(code, 6);
                    chessFields[i, 7] = IsBitSet(code, 7);
                }
            }

        }

        bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public bool IsFigureOn(int fieldId)
        {
            var lines = Fields.GetLine(fieldId);
            var row = Fields.GetRow(fieldId);
            var l = _playWithWhite ? 7 - (int)lines : (int)lines;
            var r = _playWithWhite ? 8 - row : row - 1;
            return chessFields[r, l];
        }

        public bool IsFigureOn(string fieldName)
        {
            return IsFigureOn( Fields.GetFieldNumber(fieldName));
        }
    }
}
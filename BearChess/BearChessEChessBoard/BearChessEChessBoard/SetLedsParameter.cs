using System;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public struct SetLedsParameter
    {

        public string[] FieldNames;
        public string[] InvalidFieldNames;
        public string Promote;
        public bool Thinking;
        public bool IsMove;
        public bool IsTakeBack;
        public bool IsError;
        public bool IsOwnMove;
        public string DisplayString;

    }
}
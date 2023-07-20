using System;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public class SetLEDsParameter
    {

        public string[] FieldNames;
        public string[] InvalidFieldNames;
        public string Promote;
        public bool IsThinking;
        public bool IsMove;
        public bool IsTakeBack;
        public bool IsError;
        public bool IsOwnMove;
        public bool IsCastle;
        public string DisplayString;

        public SetLEDsParameter()
        {
            FieldNames = Array.Empty<string>();
            InvalidFieldNames = Array.Empty<string>();
            Promote = string.Empty;
            DisplayString = string.Empty;
            IsThinking = false;
            IsMove = false;
            IsTakeBack = false;
            IsError = false;
            IsOwnMove = false;
            IsCastle = false;
        }

        public override string ToString()
        {
            return $"Fields: {string.Join(" ",FieldNames)} Invalid: {string.Join(" ", InvalidFieldNames)} IsMove: {IsMove} IsThinking: {IsThinking} IsError: {IsError} IsTakeBack: {IsTakeBack}";
        }
    }
}
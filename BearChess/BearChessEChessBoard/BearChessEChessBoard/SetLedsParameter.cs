using System;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{

    

    public class ProbingMove
    {
        public string FieldName;
        public decimal Score;

        public ProbingMove(string fieldName, decimal score)
        {
            FieldName = fieldName;
            Score = score;
        }
    }

    public class SetLEDsParameter
    {
        public string[] FieldNames;
        public string[] InvalidFieldNames;
        public string[] HintFieldNames;
        public ProbingMove[] ProbingMoves;
        public string Promote;
        public bool IsThinking;
        public bool IsMove;
        public bool IsEngineMove;
        public bool IsTakeBack;
        public bool IsError;
        public bool IsOwnMove;
        public bool IsProbing;
        public bool RepeatLastMove;
        public bool ForceShow;
        public string DisplayString;

        public SetLEDsParameter()
        {
            FieldNames = Array.Empty<string>();
            InvalidFieldNames = Array.Empty<string>();
            HintFieldNames = Array.Empty<string>();
            ProbingMoves = Array.Empty<ProbingMove>();
            Promote = string.Empty;
            DisplayString = string.Empty;
            IsThinking = false;
            IsMove = false;
            IsEngineMove = false;
            IsTakeBack = false;
            IsError = false;
            IsOwnMove = false;
            IsProbing = false;
            RepeatLastMove = false;
            ForceShow = false;
        }

        public override string ToString()
        {
            return $"Fields: {string.Join(" ",FieldNames)} Invalid: {string.Join(" ", InvalidFieldNames)} Hints: {string.Join(" ", HintFieldNames)} Probing; {string.Join(" ",ProbingMoves.Select(f => f.FieldName))} " +
                   $"IsMove: {IsMove} IsThinking: {IsThinking} IsError: {IsError}  IsProbing: {IsProbing} IsTakeBack: {IsTakeBack} Repeat: {RepeatLastMove}";
        }
    }
}
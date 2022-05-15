using System;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    [Serializable]
    public class Move : ICloneable
    {
        public static readonly int[] MoveOffsets = { -9, -11, 9, 11, -10, 10, 1, -1, 19, 21, 12, -8, -19, -21, -12, 8 };

        public int Figure { get; set; }
        public int FigureColor { get; set; }
        public int FromField { get; set; }
        public string FromFieldName { get; set; }
        public int ToField { get; set; }
        public string ToFieldName { get; set; }
        public int CapturedFigure { get; set; }
        public int PromotedFigure { get; set; }
        public int Value { get; set; }
        public decimal Score { get; set; }
        public string BestLine { get; set; }
        public int CapturedFigureMaterial { get; set; }
        public int Identifier { get; set; }
        public bool IsEngineMove { get; set; }
        public string CheckOrMateSign { get; set; }
        public string Comment { get; set; }
        public string EvaluationSymbol { get; set; }
        public string MoveSymbol { get; set; }
        public string OwnSymbol { get; set; }

        public string ShortMoveIdentifier { get; set; }

        [XmlIgnore] public BoardEvaluation[] BoardEvaluations { get; set; }

        public Move()
        {
        }

        public Move(int fromField, int toField, int color, int figureId)
        {
            Figure = figureId;
            FigureColor = color;
            FromField = fromField;
            ToField = toField;
            Value = int.MinValue;
            CapturedFigure = FigureId.NO_PIECE;
            PromotedFigure = FigureId.NO_PIECE;
            Identifier = fromField * 100 + toField;
            FromFieldName = Fields.GetFieldName(FromField);
            ToFieldName = Fields.GetFieldName(ToField);
            Score = 0;
            BestLine = string.Empty;
            IsEngineMove = false;
            Comment = string.Empty;
            EvaluationSymbol = string.Empty;
            MoveSymbol = string.Empty;
            OwnSymbol = string.Empty;
            ShortMoveIdentifier = string.Empty;
        }

        public Move(int fromField, int toField, int color, int figureId, IChessFigure capturedFigure) : this(fromField,
            toField, color, figureId)
        {
            CapturedFigure = capturedFigure.FigureId;
            CapturedFigureMaterial = capturedFigure.Material;
        }

        public Move(int fromField, int toField, int color, int figureId, IChessFigure capturedFigure,
            int promotedFigure) : this(fromField, toField, color, figureId)
        {
            CapturedFigure = capturedFigure.FigureId;
            CapturedFigureMaterial = capturedFigure.Material;
            PromotedFigure = promotedFigure;
        }

        public Move(int fromField, int toField, int color, int figureId, int promotedFigure) : this(fromField, toField,
            color, figureId)
        {
            PromotedFigure = promotedFigure;
        }

        public Move(int fromField, int toField, int color, int figureId, IChessFigure capturedFigure,
            int promotedFigure, decimal score, string bestLine) : this(fromField, toField, color, figureId,
            capturedFigure, promotedFigure)
        {
            Score = score;
            BestLine = bestLine;
            IsEngineMove = true;
        }

        public Move(int fromField, int toField, int color, int figureId, int promotedFigure, decimal score,
            string bestLine) :
            this(fromField, toField, color, figureId, promotedFigure)
        {
            Score = score;
            BestLine = bestLine;
            IsEngineMove = true;
        }

        public Move(int fromField, int toField, int color, int figureId, decimal score, string bestLine) :
            this(fromField, toField, color, figureId)
        {
            Score = score;
            BestLine = bestLine;
            IsEngineMove = true;
        }

        public Move(Move move)
        {
            FigureColor = move.FigureColor;
            FromField = move.FromField;
            ToField = move.ToField;
            Value = move.Value;
            CapturedFigure = move.CapturedFigure;
            CapturedFigureMaterial = move.CapturedFigureMaterial;
            PromotedFigure = move.PromotedFigure;
            Identifier = move.Identifier;
            FromFieldName = move.FromFieldName;
            ToFieldName = move.ToFieldName;
            Score = move.Score;
            BestLine = move.BestLine;
            IsEngineMove = move.IsEngineMove;
            Comment = move.Comment;
            EvaluationSymbol = move.EvaluationSymbol;
            MoveSymbol = move.MoveSymbol;
            OwnSymbol = move.OwnSymbol;
            ShortMoveIdentifier = move.ShortMoveIdentifier;
        }



        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return $"{FromFieldName}-{ToFieldName} ({Value})";
        }
    }

    public static class MoveExtentions
    {
        public static bool EqualMove(this Move move, Move move2)
        {
            return move.Identifier.Equals(move2.Identifier);
        }
    }
}
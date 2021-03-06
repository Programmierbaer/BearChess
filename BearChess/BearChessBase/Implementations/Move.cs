﻿using System;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    [Serializable]
    public class Move : ICloneable
    {
        public static readonly int[] MoveOffsets = { -9, -11, 9, 11, -10, 10, 1, -1, 19, 21, 12, -8, -19, -21, -12, 8 };

        /// <inheritdoc />
        public int Figure { get; set;  }

        /// <inheritdoc />
        public int FigureColor { get; set; }

        /// <inheritdoc />
        public int FromField { get; set; }

        /// <inheritdoc />
        public string FromFieldName { get; set; }

        /// <inheritdoc />
        public int ToField { get; set; }

        /// <inheritdoc />
        public string ToFieldName { get; set; }

        /// <inheritdoc />
        public int CapturedFigure { get; set; }

        /// <inheritdoc />
        public int PromotedFigure { get; set; }

        /// <inheritdoc />
        public int Value { get; set; }

        /// <inheritdoc />
        public decimal Score { get; set; }

        /// <inheritdoc />
        public string BestLine { get; set; }

        /// <inheritdoc />
        public int CapturedFigureMaterial { get; set; }

        /// <inheritdoc />
        public int Identifier { get; set; }

        /// <inheritdoc />
        public bool IsEngineMove { get; set;  }

        /// <inheritdoc />
        public string CheckOrMateSign { get; set; }

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
        }

        public Move(int fromField, int toField,  int color, int figureId, IChessFigure capturedFigure) : this(fromField,toField, color, figureId)
        {
            CapturedFigure = capturedFigure.FigureId;
            CapturedFigureMaterial = capturedFigure.Material;
        }

        public Move(int fromField, int toField,  int color, int figureId, IChessFigure capturedFigure, int promotedFigure) : this(fromField, toField, color, figureId)
        {
            CapturedFigure = capturedFigure.FigureId;
            CapturedFigureMaterial = capturedFigure.Material;
            PromotedFigure = promotedFigure;
        }

        public Move(int fromField, int toField,  int color, int figureId, int promotedFigure) : this(fromField, toField, color, figureId)
        {
            PromotedFigure = promotedFigure;
        }

        public Move(int fromField, int toField, int color, int figureId, IChessFigure capturedFigure, int promotedFigure, decimal score, string bestLine) : this(fromField, toField, color, figureId,capturedFigure, promotedFigure)
        {
            Score = score;
            BestLine = bestLine;
            IsEngineMove = true;
        }

        public Move(int fromField, int toField, int color, int figureId, int promotedFigure, decimal score, string bestLine) : 
            this(fromField, toField, color, figureId, promotedFigure)
        {
            Score = score;
            BestLine = bestLine;
            IsEngineMove = true;
        }

        public Move(int fromField, int toField, int color, int figureId,  decimal score, string bestLine) :
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
        }

        public override string ToString()
        {
            return $"{FromFieldName}-{ToFieldName} ({Value})";
        }

        public object Clone()
        {
            return this.MemberwiseClone();
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

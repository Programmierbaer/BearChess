using System;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    [Serializable]
    public class ExtendedEChessBoardConfiguration
    {
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
        public int DimLevel { get; set; }
        public string RGBCurrentColor { get; set; }
        public bool FlashCurrentColor { get; set; }
        public int DimCurrentColor { get; set; }
        public string RGBInvalid { get; set; }
        public bool FlashInvalid { get; set; }
        public int DimInvalid { get; set; }
        public string RGBTakeBack { get; set; }
        public bool FlashTakeBack { get; set; }
        public int DimTakeBack { get; set; }
        public string RGBHelp { get; set; }
        public bool FlashHelp { get; set; }
        public int DimHelp { get; set; }
        public int DimBook { get; set; }
        public string RGBMoveFrom { get; set; }
        public bool FlashMoveFrom { get; set; }
        public int DimMoveFrom { get; set; }
        public string RGBMoveTo { get; set; }
        public bool FlashMoveTo { get; set; }
        public int DimMoveTo { get; set; }
        public string RGBEvalAdvantage { get; set; }
        public bool FlashEvalAdvantage { get; set; }
        public int DimEvalAdvantage { get; set; }
        public string RGBEvalDisAdvantage { get; set; }
        public bool FlashEvalDisAdvantage { get; set; }
        public int DimEvalDisAdvantage { get; set; }
        public string RGBPossibleMoves { get; set; }
        public bool FlashPossibleMoves { get; set; }
        public int DimPossibleMoves { get; set; }
        public string RGBPossibleMovesGood { get; set; }
        public bool FlashPossibleMovesGood { get; set; }
        public int DimPossibleMovesGood { get; set; }
        public string RGBPossibleMovesBad { get; set; }
        public bool FlashPossibleMovesBad { get; set; }
        public int DimPossibleMovesBad { get; set; }
        public string RGBPossibleMovesPlayable { get; set; }
        public bool FlashPossibleMovesPlayable { get; set; }
        public int DimPossibleMovesPlayable { get; set; }
        public int ScanIntervall { get; set; }
        public bool InterruptMode { get; set; }
        public bool ShowPossibleMoves { get; set; }
        public bool ShowPossibleMovesEval { get; set; }
        public bool ShowOwnMoves { get; set; }
        public bool ShowInvalidMoves { get; set; }
        public bool ShowTakeBackMoves { get; set; }
        public bool ShowHintMoves { get; set; }
        public bool ShowBookMoves { get; set; }
        public bool ShowMoveLine { get; set; }
        public bool ShowCurrentColor { get; set; }
        public bool ShowEvaluationValue { get; set; }
        public string BuzzerConnected { get; set; }
        public string BuzzerInvalid { get; set; }
        public string BuzzerEngineMove { get; set; }
        public string BuzzerCheck { get; set; }
        public string BuzzerDraw { get; set; }
        public string BuzzerCheckMate { get; set; }
        public bool SendBuzzerConnected { get; set; }
        public bool SendBuzzerInvalid { get; set; }
        public bool SendBuzzerEngineMove { get; set; }
        public bool SendBuzzerCheck { get; set; }
        public bool SendBuzzerDraw { get; set; }
        public bool SendBuzzerCheckMate { get; set; }
        public string RGBBookMove { get; set; }
        public bool FlashBookMove { get; set; }


        public ExtendedEChessBoardConfiguration()
        {
            Name = "BearChess";
            DimLevel = 4;
            ShowMoveLine = false;
            ShowCurrentColor = true;
            ShowEvaluationValue = true;
            RGBCurrentColor = "099";
            RGBInvalid = "F00";
            RGBTakeBack = "0FF";
            RGBHelp = "00F";
            RGBBookMove = "00F";
            RGBMoveFrom = "0F0";
            RGBMoveTo = "0F0";
            RGBEvalAdvantage = "0F0";
            RGBEvalDisAdvantage = "F00";
            RGBPossibleMoves = "00F";
            RGBPossibleMovesGood = "0F0";
            RGBPossibleMovesBad = "F00";
            RGBPossibleMovesPlayable = "00F";
            BuzzerConnected = "000000";
            BuzzerInvalid = "000000";
            BuzzerEngineMove = "000000";
            BuzzerCheck = "000000";
            BuzzerDraw = "000000";
            BuzzerCheckMate = "000000";
            FlashCurrentColor = false;
            FlashEvalAdvantage = false;
            FlashEvalDisAdvantage  = false;
            FlashHelp = false;
            FlashMoveFrom = false;
            FlashMoveTo = false;
            FlashTakeBack = true;
            FlashInvalid = false;
            FlashBookMove = false;
            IsCurrent = false;
            ScanIntervall = 250;
            InterruptMode = true;
            DimCurrentColor = 4;
            DimEvalAdvantage = 4;
            DimEvalDisAdvantage = 4;
            DimHelp = 4;
            DimBook = 4;
            DimInvalid = 4;
            DimMoveFrom = 4;
            DimMoveTo = 4;
            DimTakeBack = 4;
            DimPossibleMoves = 4;
            DimPossibleMovesBad = 4;
            DimPossibleMovesGood = 4;
            DimPossibleMovesPlayable = 4;
            ShowPossibleMoves = false;
            ShowPossibleMovesEval = false;
            ShowOwnMoves = false;
            ShowInvalidMoves = true;
            ShowTakeBackMoves = true;
            ShowHintMoves = true;
            ShowBookMoves = false;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool SplitMoveFromMoveTo()
        {
            var s = (RGBMoveFrom.Equals(RGBMoveTo) && DimMoveFrom.Equals(DimMoveTo) &&
                    FlashMoveFrom.Equals(FlashMoveTo));
            return !s;
        }
    }
}
using System;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    [Serializable]
    public class ExtendedEChessBoardConfiguration
    {
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
        public int DimLevel { get; set; }
        public bool ShowMoveLine { get; set; }
        public bool ShowCurrentColor { get; set; }
        public bool ShowEvaluationValue { get; set; }
        public string RGBCurrentColor { get; set; }
        public bool FlashCurrentColor { get; set; }
        public string RGBInvalid { get; set; }
        public bool FlashInvalid { get; set; }
        public string RGBTakeBack { get; set; }
        public bool FlashTakeBack { get; set; }
        public string RGBHelp { get; set; }
        public bool FlashHelp { get; set; }
        public string RGBMoveFrom { get; set; }
        public bool FlashMoveFrom { get; set; } 
        public string RGBEvalAdvantage { get; set; }
        public bool FlashEvalAdvantage { get; set; }
        public string RGBEvalDisAdvantage { get; set; }
        public bool FlashEvalDisAdvantage { get; set; }

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
            RGBMoveFrom = "0F0";
            RGBEvalAdvantage = "0F0";
            RGBEvalDisAdvantage = "F00";
            FlashCurrentColor = false;
            FlashEvalAdvantage = false;
            FlashEvalDisAdvantage  = false;
            FlashHelp = false;
            FlashMoveFrom = false;
            FlashTakeBack = true;
            FlashInvalid = false;
            IsCurrent = false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
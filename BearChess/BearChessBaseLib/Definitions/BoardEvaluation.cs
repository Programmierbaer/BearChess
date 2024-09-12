using System;
using System.Collections.Generic;
using System.Linq;

namespace www.SoLaNoSoft.com.BearChessBase.Definitions
{
    [Serializable]
    public class BoardEvaluation
    {
        public int Material { get; set; }
        public Dictionary<int, List<int>> FigurePositionValues { get; set; }
        public int Mobility { get; set; }
        public int Safety { get; set; }
        public int Attacking { get; set; }

        public BoardEvaluation()
        {
            FigurePositionValues = new Dictionary<int, List<int>>();
        }

        public int GetPositionSummary()
        {
            int sum = 0;
            foreach (var key in FigurePositionValues.Keys)
            {
                sum += FigurePositionValues[key].Sum(s => s);
            }

            return sum;
        }

        public override string ToString()
        {
            return
                $"Material: {Material} Mobility: {Mobility} Safety: {Safety} Attacking: {Attacking} ";
        }
    }
}

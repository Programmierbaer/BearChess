using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.Engine
{
    public class BoardEvaluation
    {
        public int Material { get; set; }
        public Dictionary<int, List<int>> FigurePositionValues { get; set; }
        public int Mobility { get; set; }
        public int Safety { get; set; }
        public int Attacking { get; set; }

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
                $"Material: {Material} Mobility: {Mobility} Safety: {Safety} Attacking: {Attacking} Pawn:  Knight:  Bishop:  Rook:  Queen:  King: ";
        }
    }
}

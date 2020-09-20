using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www.SoLaNoSoft.com.BearChessWin
{
    [Serializable]
    public class TimeControl
    {
        public TimeControlEnum TimeControlType { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int HumanValue { get; set; }
    }
}

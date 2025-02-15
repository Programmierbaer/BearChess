using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBaseLib.Definitions
{
    public class TimeControlValue
    {
        private readonly string _description;
        public TimeControlEnum TimeControl { get; set; }

        public TimeControlValue(TimeControlEnum timeControlEnum, string description)
        {
            _description = description;
            TimeControl = timeControlEnum;
        }

        public override string ToString()
        {
            return _description;
        }
    }
}

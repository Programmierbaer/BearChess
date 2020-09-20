using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www.SoLaNoSoft.com.BearChess.CertaboChessBoard
{
    [Serializable]
    public class CalibrateData
    {
        public string BasePositionCodes { get; set; }
        public string WhiteQueenCodes { get; set; }
        public string BlackQueenCodes { get; set; }
    }
}

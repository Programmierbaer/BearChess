using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www.SoLaNoSoft.com.BearChessWin
{
    [Serializable]
    public class BoardFieldsSetup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string WhiteFileName { get; set; }
        public string BlackFileName { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

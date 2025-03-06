using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www.SoLaNoSoft.com.BearChessServerLib
{
    [Serializable]
    public class ServerChessBoardConfiguration
    {

        public string ServerBoardId { get; set; }
        public bool SameBoardForWhiteAndBlack { get; set; }

        public string BearChessClientNameWhite { get; set; }
        public string BearChessClientNameBlack { get; set; }
        public string EBoardNameWhite { get; set; }
        public string EBoardNameBlack { get; set; }
        public string ComPortWhite { get; set; }
        public string ComPortBlack { get; set; }


        public ServerChessBoardConfiguration() 
        { 
            ServerBoardId = string.Empty;
            SameBoardForWhiteAndBlack = false;
            BearChessClientNameWhite = string.Empty;
            BearChessClientNameBlack = string.Empty;
            EBoardNameWhite = string.Empty;
            EBoardNameBlack = string.Empty;
            ComPortWhite = string.Empty;
            ComPortBlack = string.Empty;
        }
    }
}

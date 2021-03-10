using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    [Serializable]
    public class CurrentGame
    {
        public UciInfo WhiteConfig { get; set; }
        public UciInfo BlackConfig { get; set; }
        public TimeControl TimeControl { get; set; }
        public string PlayerWhite { get; set; }
        public string PlayerBlack { get; set; }
        public bool StartFromBasePosition { get; set; }

        public CurrentGame(UciInfo whiteConfig, UciInfo blackConfig, TimeControl timeControl,
                           string playerWhite, string playerBlack, bool startFromBasePosition)
        {
            WhiteConfig = whiteConfig;
            BlackConfig = blackConfig;
            TimeControl = timeControl;
            PlayerWhite = playerWhite;
            PlayerBlack = playerBlack;
            StartFromBasePosition = startFromBasePosition;
        }

        public CurrentGame()
        {
            
        }
    }
}

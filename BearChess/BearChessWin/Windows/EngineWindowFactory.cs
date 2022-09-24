using System.IO;
using www.SoLaNoSoft.com.BearChess.FicsClient;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public static class EngineWindowFactory
    {
        public static IEngineWindow GetEngineWindow(Configuration configuration, string uciPath, IEngineWindow engineWindow)
        {
            if (engineWindow != null)
            {
                return engineWindow;
            }
            return new EngineWindow(configuration, uciPath);
        }
    }
}
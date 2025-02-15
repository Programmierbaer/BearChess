using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.MChessLinkUci;

namespace MChessLinkUciCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            var uciWrapper = new UciWrapper();
            uciWrapper.Init("MChessLinkUciCore");
            var thread = new Thread(uciWrapper.Run) { IsBackground = true };
            thread.Start();
            thread.Join();
        }
    }
}

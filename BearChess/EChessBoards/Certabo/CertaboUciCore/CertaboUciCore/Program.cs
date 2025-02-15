using System;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CertaboUci;

namespace CertaboUciCore
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var uciWrapper = new UciWrapper();
            uciWrapper.Init("CertaboUci");
            var thread = new Thread(uciWrapper.Run) { IsBackground = true };
            thread.Start();
            thread.Join();
        }
    }
}

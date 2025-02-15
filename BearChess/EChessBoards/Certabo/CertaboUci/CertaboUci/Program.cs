using System.Threading;

namespace www.SoLaNoSoft.com.BearChess.CertaboUci
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

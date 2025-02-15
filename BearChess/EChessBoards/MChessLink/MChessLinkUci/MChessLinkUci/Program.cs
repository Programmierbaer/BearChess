using System.Threading;

namespace www.SoLaNoSoft.com.BearChess.MChessLinkUci
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var uciWrapper = new UciWrapper();
            uciWrapper.Init("MChessLinkUci");
            var thread = new Thread(uciWrapper.Run) { IsBackground = true };
            thread.Start();
            thread.Join();
        }
    }
}

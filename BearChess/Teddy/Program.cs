using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChess.Teddy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var uciWrapper = new UciWrapper(args[0]);
            uciWrapper.Init(Constants.Teddy);
            var thread = new Thread(uciWrapper.Run) { IsBackground = true };
            thread.Start();
            thread.Join();
        }
    }
}

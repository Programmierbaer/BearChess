using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class NullLogger : ILogger
    {

        public void Clear()
        {
        }

        public void Log(string logInfo, bool appendNewLine)
        {
        }


        public void Log(IList<Move> logMoves, bool appendNewLine = true)
        {

        }

        public void Log(Move logMove, bool appendNewLine = true)
        {

        }

        public void Pause()
        {

        }

        public void Close()
        {

        }

        public void Continue()
        {

        }
    }
}

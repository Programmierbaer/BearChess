using System;
using System.Collections.Generic;
using System.Text;
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


        public void Log(IList<IMove> logMoves, bool appendNewLine = true)
        {

        }

        public void Log(IMove logMove, bool appendNewLine = true)
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

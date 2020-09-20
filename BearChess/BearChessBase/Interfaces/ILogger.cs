using System;
using System.Collections.Generic;
using System.Text;

namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface ILogger
    {
        void Clear();
        void Log(string logInfo, bool appendNewLine = true);
        void Log(IList<IMove> logMoves, bool appendNewLine = true);
        void Log(IMove logMove, bool appendNewLine = true);
        void Pause();
        void Close();
        void Continue();
    }
}

using System.Collections.Generic;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessBase.Interfaces
{
    public interface ILogger
    {
        void Clear();
        void Log(string logInfo, bool appendNewLine = true);
        void Log(IList<Move> logMoves, bool appendNewLine = true);
        void Log(Move logMove, bool appendNewLine = true);
        void Pause();
        void Close();
        void Continue();
    }
}

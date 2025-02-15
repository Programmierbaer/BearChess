using System;

namespace www.SoLaNoSoft.com.BearChessBase.Definitions
{
    public class EngineFinishedCalculationEventArgs : EventArgs
    {

        public string FromField { get; }

        public string ToField { get; }

        public bool Resigned { get; }

        /// <summary>
        /// The engine finished with the evaluated best move <paramref name="fromField"/> to <paramref name="toField"/>
        /// or set <paramref name="resigned"/> to true if resigned.
        /// </summary>
        public EngineFinishedCalculationEventArgs(string fromField, string toField, bool resigned)
        {
            FromField = fromField;
            ToField = toField;
            Resigned = resigned;
        }
    }
}

using System;

namespace www.SoLaNoSoft.com.BearChessBase.Definitions
{

    public class EngineInformationEventArgs : EventArgs
    {

        public string Information { get; }

        /// <summary>
        /// The engine informs about <paramref name="information"/>.
        /// </summary>
        public EngineInformationEventArgs(string information)
        {
            Information = information;
        }
    }
}

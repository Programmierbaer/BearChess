using System;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class UciLogger : FileLogger
    {
        private readonly string _name;

        public event EventHandler<UciEventArgs> UciCommunicationEvent;

        public UciLogger(string name, string fileName, int historyCount, long maxFileSizeInMb) : base(fileName, historyCount, maxFileSizeInMb)
        {
            _name = name;
        }


        /// <inheritdoc />
        public override void LogDebug(string logMessage)
        {
            base.LogDebug(logMessage);
            if (logMessage.StartsWith(">>"))
            {
                OnUciCommunicationEvent(logMessage, "to");
            }
            if (logMessage.StartsWith("<<"))
            {
                OnUciCommunicationEvent(logMessage, "from");
            }
        }

        public override void LogInfo(string logMessage)
        {
            base.LogInfo(logMessage);
            if (logMessage.StartsWith(">>"))
            {
                OnUciCommunicationEvent(logMessage, "to");
            }
            if (logMessage.StartsWith("<<"))
            {
                OnUciCommunicationEvent(logMessage, "from");
            }
        }


        protected virtual void OnUciCommunicationEvent(string command, string direction)
        {
            UciCommunicationEvent?.Invoke(this, new UciEventArgs(_name, command, direction));
        }
    }
}
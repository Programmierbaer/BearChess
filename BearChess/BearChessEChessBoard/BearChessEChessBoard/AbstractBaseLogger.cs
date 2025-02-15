using System;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public abstract class AbstractBaseLogger : ILogging
    {
        protected AbstractBaseLogger()
        {
            LogLevel = LogLevel.Debug | LogLevel.Error | LogLevel.Info | LogLevel.Warning;
        }

        /// <inheritdoc />
        public LogLevel LogLevel { get; set; }

        /// <inheritdoc />
        public bool Active { get; set; }

        /// <inheritdoc />
        public abstract void LogInfo(string logMessage);

        /// <inheritdoc />
        public abstract void LogDebug(string logMessage);

        /// <inheritdoc />
        public abstract void LogWarning(string logMessage);

        /// <inheritdoc />
        public abstract void LogWarning(string logMessage, Exception ex);

        /// <inheritdoc />
        public abstract void LogWarning(Exception ex);

        /// <inheritdoc />
        public abstract void LogError(Exception ex);

        /// <inheritdoc />
        public abstract void LogError(string logMessage);

        /// <inheritdoc />
        public abstract void LogError(string logMessage, Exception ex);

        /// <summary>
        /// Indicates if the current log level equals <paramref name="logLevel"/>.
        /// </summary>
        protected bool IsLogLevel(LogLevel logLevel)
        {
            return (LogLevel & logLevel) == logLevel;
        }
    }
}

using System;

namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    /// <summary>
    /// Log-Level as flag
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        Debug = 1,
        Info = 2,
        Warning = 4,
        Error = 8
    }

    /// <summary>
    /// Logging
    /// </summary>
    public interface ILogging
    {

        /// <summary>
        /// Set log level
        /// </summary>
        LogLevel LogLevel { get; set; }

        /// <summary>
        /// Activate or deactivate logging
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// Log an information
        /// </summary>
        void LogInfo(string logMessage);

        /// <summary>
        /// Log a debug output
        /// </summary>
        void LogDebug(string logMessage);

        /// <summary>
        /// Log a warning
        /// </summary>
        void LogWarning(string logMessage);

        /// <summary>
        /// Log a warning with a related exception <paramref name="ex"/>
        /// </summary>
        void LogWarning(string logMessage, Exception ex);

        /// <summary>
        /// Log an exception <paramref name="ex"/> as warning
        /// </summary>
        void LogWarning(Exception ex);

        /// <summary>
        /// Log an exception <paramref name="ex"/> as error
        /// </summary>
        void LogError(Exception ex);

        /// <summary>
        /// Log an error
        /// </summary>
        void LogError(string logMessage);

        /// <summary>
        /// Log an error with a related exception <paramref name="ex"/>
        /// </summary>
        void LogError(string logMessage, Exception ex);
    }
}

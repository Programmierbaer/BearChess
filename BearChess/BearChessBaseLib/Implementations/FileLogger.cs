using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{

    public class FileLogger : ILogger, ILogging
    {
        private readonly string _fileName;
        private readonly ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();
        private readonly int _historyCount;
        private readonly long _maxFileSizeInMb;
        private volatile bool _deleteFile;
        private bool _paused;
        private volatile bool _stopThread;
        private Thread _thread;
        public LogLevel LogLevel { get; set; }
        public bool Active { get; set; }


        public FileLogger(string fileName, int historyCount, long maxFileSizeInMb)
        {
            _fileName = fileName;
            _historyCount = historyCount;
            _maxFileSizeInMb = maxFileSizeInMb;
            ErrorOccurred = false;
            ErrorReason = string.Empty;
            Active = true;
            LogLevel = LogLevel.Debug | LogLevel.Info | LogLevel.Error | LogLevel.Warning;
            _thread = new Thread(DoWork) {IsBackground = true};
            _thread.Start();
        }

        public bool ErrorOccurred { get; private set; }

        public string ErrorReason { get; private set; }


        /// <inheritdoc />
        public void Clear()
        {
            _deleteFile = true;
        }

        /// <inheritdoc />
        public void Log(string logInfo, bool appendNewLine = true)
        {
            if (_paused || string.IsNullOrEmpty(logInfo))
            {
                return;
            }

            var append = appendNewLine ? Environment.NewLine : string.Empty;
            _fileQueue.Enqueue($"{logInfo}{append}");
        }


        /// <inheritdoc />
        public void Log(IList<Move> logMoves, bool appendNewLine)
        {
            if (_paused || logMoves == null)
            {
                return;
            }

            var output = string.Empty;
            foreach (var logMove in logMoves)
            {
                output += $" {Fields.GetFieldName(logMove.FromField)}-{Fields.GetFieldName(logMove.ToField)}";
            }

            var append = appendNewLine ? Environment.NewLine : string.Empty;
            _fileQueue.Enqueue($"{output}{append}");
        }

        /// <inheritdoc />
        public void Log(Move logMove, bool appendNewLine = true)
        {
            if (_paused || logMove == null)
            {
                return;
            }

            Log(new List<Move> {logMove}, appendNewLine);
        }

        /// <inheritdoc />
        public void Pause()
        {
            _paused = true;
        }

        /// <inheritdoc />
        public void Close()
        {
            Pause();
            _stopThread = true;
        }

        /// <inheritdoc />
        public void Continue()
        {
            if (_stopThread)
            {
                _stopThread = false;
                if (_fileQueue.IsEmpty)
                {
                    _thread = new Thread(DoWork) {IsBackground = true};
                    _thread.Start();
                }
            }

            _paused = false;
        }



        /// <inheritdoc />
        public virtual void LogInfo(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Info))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:o} Info:    {logMessage}");
            }
        }

        /// <inheritdoc />
        public virtual void LogDebug(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Debug))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:o} Debug:   {logMessage}");
            }
        }


        /// <inheritdoc />
        public void LogWarning(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Warning))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:o} Warning: {logMessage}");
            }
        }

        /// <inheritdoc />
        public void LogWarning(string logMessage, Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Warning))
            {
                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:o} Warning: {logMessage}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogWarning(ex.InnerException);
                }
            }
        }

        /// <inheritdoc />
        public void LogWarning(Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Warning))
            {
                LogError($"Art der Ausnahme: {ex.GetType()}");
                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:o} Warning: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogWarning(ex.InnerException);
                }
            }
        }

        /// <inheritdoc />
        public virtual void LogError(Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Error))
            {
                LogError($"Art der Ausnahme: {ex.GetType()}");
                if (ex is AggregateException aggregateException)
                {
                    var baseException = aggregateException.GetBaseException();
                    LogError(baseException);
                    foreach (var aggregateExceptionInnerException in aggregateException.InnerExceptions)
                    {
                        LogError(aggregateExceptionInnerException);
                    }

                    return;
                }

                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:o} Error:   {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException);
                }
            }
        }

        /// <inheritdoc />
        public void LogError(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Error))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:o} Error:   {logMessage}");
            }
        }

        /// <inheritdoc />
        public void LogError(string logMessage, Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Error))
            {
                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:o} Error:   {logMessage}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException);
                }
            }
        }

        private void DoWork()
        {
            while (!_stopThread || !_fileQueue.IsEmpty)
            {
                Thread.Sleep(10);
                if (_fileQueue.IsEmpty && _deleteFile)
                {
                    try
                    {
                        File.Delete(_fileName);
                    }
                    catch
                    {
                        //
                    }

                    _deleteFile = false;
                    continue;
                }

                var sb = new StringBuilder();
                while (_fileQueue.TryDequeue(out var result))
                {
                    sb.Append(result);
                }
                if (CheckActiveAndLogFile())
                {
                    try
                    {
                        File.AppendAllText(_fileName, sb.ToString());
                    }
                    catch (Exception lex)
                    {
                        ErrorReason = lex.Message;
                        ErrorOccurred = true;
                    }
                }
            }
        }

        protected bool IsLogLevel(LogLevel logLevel)
        {
            return (LogLevel & logLevel) == logLevel;
        }

        private bool CheckActiveAndLogFile()
        {
            if (ErrorOccurred || !Active)
            {
                return false;
            }

            try
            {
                if (!File.Exists(_fileName))
                {
                    return true;
                }

                var fileInfo = new FileInfo(_fileName);
                var lastWriteTime = fileInfo.LastWriteTime;
                var mbSize = _maxFileSizeInMb > 0 ? fileInfo.Length / 1024 / 1024 : 0;
                if (lastWriteTime.Date < DateTime.Now.Date || mbSize > _maxFileSizeInMb)
                {
                    // Delete old files
                    var directoryName = fileInfo.DirectoryName;
                    var baseName = fileInfo.Name;
                    if (directoryName != null)
                    {
                        var allFiles = Directory.GetFiles(directoryName, baseName + "_*")
                                                .OrderByDescending(f => f).ToArray();
                        for (var a = allFiles.Length - 1; a > _historyCount; a--)
                        {
                            File.Delete(allFiles[a]);
                        }
                    }

                    var count = 0;
                    var nameWithDate = $"{_fileName}_{lastWriteTime.Date:yyyyMMdd}";
                    var newName = nameWithDate;

                    while (File.Exists(newName) && count <= 10)
                    {
                        count++;
                        newName = nameWithDate + "_" + count;
                    }


                    if (!File.Exists(newName))
                    {
                        File.Move(_fileName, newName);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReason = ex.Message;
                ErrorOccurred = true;
            }


            return !ErrorOccurred;
        }
    }
}
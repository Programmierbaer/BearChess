using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public class FileLogger : AbstractBaseLogger
    {
        private readonly string _fileName;
        private readonly int _historyCount;
        private readonly long _maxFileSizeInMb;
        private readonly ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();

        public bool ErrorOccured { get; private set; }

        public string ErrorReason { get; private set; }

        public FileLogger(string fileName, int historyCount, long maxFileSizeInMb) : base()
        {
            _fileName = fileName;
            _historyCount = historyCount;
            _maxFileSizeInMb = maxFileSizeInMb;
            ErrorOccured = false;
            ErrorReason = string.Empty;
            Active = true;
            Thread thread = new Thread(DoWork) { IsBackground = true };
            thread.Start();
        }


        public FileLogger(string fileName, int historyCount) : this(fileName, historyCount, 0)
        {

        }


        public FileLogger(string fileName) : this(fileName, 0, 0)
        {

        }


        /// <inheritdoc />
        public override void LogInfo(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Info))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:s} Info:    {logMessage}");
            }

        }

        /// <inheritdoc />
        public override void LogDebug(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Debug))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:s} Debug:   {logMessage}");
            }
        }


        /// <inheritdoc />
        public override void LogWarning(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Warning))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:s} Warning: {logMessage}");
            }

        }

        /// <inheritdoc />
        public override void LogWarning(string logMessage, Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Warning))
            {
                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:s} Warning: {logMessage}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogWarning(ex.InnerException);
                }
            }
        }
        /// <inheritdoc />
        public override void LogWarning(Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Warning))
            {
                LogError($"Art der Ausnahme: {ex.GetType()}");
                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:s} Warning: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogWarning(ex.InnerException);
                }
            }
        }

        /// <inheritdoc />
        public override void LogError(Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Error))
            {
                LogError($"Art der Ausnahme: {ex.GetType()}");
                if (ex is AggregateException aggregateException)
                {
                    Exception baseException = aggregateException.GetBaseException();
                    LogError(baseException);
                    foreach (Exception aggregateExceptionInnerException in aggregateException.InnerExceptions)
                    {
                        LogError(aggregateExceptionInnerException);
                    }

                    return;
                }

                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:s} Error:   {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException);
                }
            }

        }

        /// <inheritdoc />
        public override void LogError(string logMessage)
        {
            if (Active && IsLogLevel(LogLevel.Error))
            {
                _fileQueue.Enqueue($"{Environment.NewLine}{DateTime.UtcNow:s} Error:   {logMessage}");
            }

        }

        /// <inheritdoc />
        public override void LogError(string logMessage, Exception ex)
        {
            if (Active && IsLogLevel(LogLevel.Error))
            {
                _fileQueue.Enqueue(
                    $"{Environment.NewLine}{DateTime.UtcNow:s} Error:   {logMessage}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException);
                }
            }
        }

        #region Privates


        private void DoWork()
        {
            while (true)
            {
                Thread.Sleep(10);
                if (_fileQueue.IsEmpty)
                {
                    continue;
                }

                StringBuilder sb = new StringBuilder();
                while (_fileQueue.TryDequeue(out string result))
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
                        ErrorOccured = true;
                    }
                }
            }
        }


        private bool CheckActiveAndLogFile()
        {
            if (ErrorOccured || !Active)
            {
                return false;
            }

            try
            {
                if (!File.Exists(_fileName))
                {
                    return true;
                }

                FileInfo fileInfo = new FileInfo(_fileName);
                DateTime lastWriteTime = fileInfo.LastWriteTime;
                long mbSize = _maxFileSizeInMb > 0 ? fileInfo.Length / 1024 / 1024 : 0;
                if (lastWriteTime.Date < DateTime.Now.Date || mbSize > _maxFileSizeInMb)
                {
                    // Ggf. die ältesten Dateien löschen
                    string directoryName = fileInfo.DirectoryName;
                    string baseName = fileInfo.Name;
                    if (directoryName != null)
                    {
                        string[] allFiles = Directory.GetFiles(directoryName, baseName + "_*")
                            .OrderByDescending(f => f).ToArray();
                        for (int a = allFiles.Length - 1; a > _historyCount; a--)
                        {
                            File.Delete(allFiles[a]);
                        }
                    }

                    int count = 0;
                    string nameWithDate = $"{_fileName}_{lastWriteTime.Date:yyyyMMdd}";
                    string newName = nameWithDate;

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
                ErrorOccured = true;
            }


            return !ErrorOccured;
        }

        #endregion
    }
}

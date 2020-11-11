using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace www.SoLaNoSoft.com.BearChessTools
{

    public class UciEventArgs : EventArgs
    {

        public string Command { get; }
        public string Direction { get; }
        public string Name { get; }


        public UciEventArgs(string name, string command, string direction)
        {
            Name = name;
            Command = command;
            Direction = direction;
        }
    }


    public class UciLogger : FileLogger
    {
        private readonly string _name;
        public event EventHandler<UciEventArgs> UciCommunicationEvent;

        public UciLogger(string name, string fileName, int historyCount, long maxFileSizeInMb) : base(fileName, historyCount, maxFileSizeInMb)
        {
            _name = name;
        }

        public UciLogger(string name, string fileName, int historyCount) : base(fileName, historyCount)
        {
        }

        public UciLogger(string name, string fileName) : base(fileName)
        {
        }

        /// <inheritdoc />
        public override void LogDebug(string logMessage)
        {
            base.LogDebug(logMessage);
            if (logMessage.StartsWith(">>"))
            {
                OnUciCommunicationEvent(logMessage,"to");
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
          UciCommunicationEvent?.Invoke(this, new UciEventArgs(_name, command,direction));
        }
    }


    public class FileLogger : AbstractBaseLogger
    {
        private readonly string _fileName;
        private readonly int _historyCount;
        private readonly long _maxFileSizeInMb;
        private readonly ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();

        public bool ErrorOccurred { get; private set; }

        public string ErrorReason { get; private set; }

        public FileLogger(string fileName, int historyCount, long maxFileSizeInMb) : base()
        {
            _fileName = fileName;
            try
            {
                var fileInfo = new FileInfo(_fileName);
                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                Active = true;
                ErrorOccurred = false;
                ErrorReason = string.Empty;
            }
            catch (Exception ex)
            {
                Active = false;
                ErrorOccurred = true;
                ErrorReason = ex.Message;
            }

            _historyCount = historyCount;
            _maxFileSizeInMb = maxFileSizeInMb;
            var thread = new Thread(DoWork) { IsBackground = true };
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
                LogError($"Exception: {ex.GetType()}");
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
                LogError($"Exception: {ex.GetType()}");
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
                        ErrorOccurred = true;
                    }
                }
            }
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

                FileInfo fileInfo = new FileInfo(_fileName);
                DateTime lastWriteTime = fileInfo.LastWriteTime;
                long mbSize = _maxFileSizeInMb > 0 ? fileInfo.Length / 1024 / 1024 : 0;
                if (lastWriteTime.Date < DateTime.Now.Date || mbSize > _maxFileSizeInMb)
                {
                    // Delete old files
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
                ErrorOccurred = true;
            }


            return !ErrorOccurred;
        }

        #endregion
    }
}
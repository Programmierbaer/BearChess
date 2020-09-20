using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class FileLogger : ILogger
    {
        private readonly string _fileName;
        private bool _paused;
        private readonly ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();
        private Thread _thread;
        private volatile bool _stopThread;
        private volatile bool _deleteFile;


        public FileLogger(string fileName)
        {
            _fileName = fileName;
            _thread = new Thread(DoWork) {IsBackground = true};
            _thread.Start();
        }

        private void DoWork()
        {
            while (!_stopThread || !_fileQueue.IsEmpty)
            {
                Thread.Sleep(10);
                if (_fileQueue.IsEmpty)
                {
                    if (_deleteFile )
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
                    }
                    continue;
                }
                StringBuilder sb = new StringBuilder();
                while (_fileQueue.TryDequeue(out var result))
                {
                    sb.Append(result);
                }
                File.AppendAllText(_fileName, sb.ToString());
            
             
            }
        }


        /// <inheritdoc />
        public void Clear()
        {
            _deleteFile = true;
        }

        /// <inheritdoc />
        public void Log(string logInfo, bool appendNewLine = true)
        {
            if (_paused || string.IsNullOrEmpty(logInfo)) return;
            string append = appendNewLine ? Environment.NewLine : string.Empty;
            _fileQueue.Enqueue($"{logInfo}{append}");

        }


        /// <inheritdoc />
        public void Log(IList<IMove> logMoves, bool appendNewLine)
        {
            if (_paused || logMoves == null) return;
            string output = string.Empty;
            foreach (IMove logMove in logMoves)
            {
                output += $" {Fields.GetFieldName(logMove.FromField)}-{Fields.GetFieldName(logMove.ToField)}";
            }

            string append = appendNewLine ? Environment.NewLine : string.Empty;
            _fileQueue.Enqueue($"{output}{append}");
        }

        /// <inheritdoc />
        public void Log(IMove logMove, bool appendNewLine = true)
        {
            if (_paused || logMove == null) return;
            Log(new List<IMove>() { logMove }, appendNewLine);
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
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessDatabase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class UciInstaller
    {
        private Process _engineProcess;
        private UciInfo _uciInfo;

        public UciInfo Install(string fileName)
        {
            _uciInfo = new UciInfo(fileName);
            _engineProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    FileName = fileName,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(fileName)
                }

            };
            _engineProcess.Start();
            Thread thread = new Thread(ReadFromEngine) { IsBackground = true };
            thread.Start();
            _uciInfo.Valid = thread.Join(5000);
            try
            {
                _engineProcess.Kill();
                _engineProcess.Dispose();
            }
            catch
            {
                //
            }

            return _uciInfo;
        }

        private void ReadFromEngine()
        {
            try
            {
                _engineProcess.StandardInput.WriteLine("uci");
                string waitingFor = "uciok";
                while (true)
                {
                    var readToEnd = _engineProcess.StandardOutput.ReadLine();

                    if (!string.IsNullOrWhiteSpace(readToEnd) && readToEnd.Equals(waitingFor))
                    {
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(readToEnd))
                    {
                        if (readToEnd.StartsWith("option"))
                        {
                         _uciInfo.AddOption(readToEnd);
                        }

                        if (readToEnd.StartsWith("id name"))
                        {
                            _uciInfo.OriginName = readToEnd.Substring("id name".Length).Trim();
                            _uciInfo.Name = _uciInfo.OriginName;
                        }
                        if (readToEnd.StartsWith("id author"))
                        {
                            _uciInfo.Author = readToEnd.Substring("id author".Length).Trim();
                        }
                    }

                }
                _engineProcess.StandardInput.WriteLine("quit");
                Thread.Sleep(100);
            }
            catch
            {
                //
            }
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Threading;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public class UciInstaller
    {
        private Process _engineProcess;
        private UciInfo _uciInfo;

        public UciInfo Install(string fileName, string parameters)
        {
            _uciInfo = new UciInfo(fileName);
            _uciInfo.CommandParameter = parameters;
            _engineProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    FileName = fileName,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(fileName),
                    Arguments = parameters
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
                string waitingFor = "uciok";
                _engineProcess.StandardInput.Write("uci");
                _engineProcess.StandardInput.Write("\n");
                
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
                _engineProcess.StandardInput.Write("quit");
                _engineProcess.StandardInput.Write("\n");
                Thread.Sleep(100);
            }
            catch
            {
                //
            }
        }
    }
}

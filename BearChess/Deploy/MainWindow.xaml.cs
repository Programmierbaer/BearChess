using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace Deploy
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string AssemblyFileName =
            @"C:\Users\larsn\source\github\BearChess\BearChess\BearChessWin\Properties\AssemblyInfo.cs";

        private readonly string AssemblyFileNameTesting =
            @"C:\Users\larsn\source\repos\BearChess\BearChess\BearChessWin\Properties\AssemblyInfo.cs";

        private readonly string BearChessFileNameTesting =
            @"C:\Users\larsn\source\repos\BearChess\BearChess\BearChessWin\bin\Debug\BearChessWin.exe";

        private readonly string BearChessFileName =
            @"C:\Users\larsn\source\github\BearChess\BearChess\BearChessWin\bin\Debug\BearChessWin.exe";

        private readonly string TeddyFileNameTesting =
            @"C:\Users\larsn\source\repos\BearChess\BearChess\Teddy\bin\Debug\Teddy.exe";

        private readonly string TeddyFileName =
            @"C:\Users\larsn\source\github\BearChess\BearChess\Teddy\bin\Debug\Teddy.exe";

        private string DocumentFileName = @"C:\Users\larsn\source\github\BearChess\Documentation\BearChess.pdf";

        private string EcoFileNameTesting  = @"C:\Users\larsn\source\github\BearChess\bearchess.eco";

        private string EcoFileName = @"C:\Users\larsn\source\github\BearChess\bearchess.eco";

        private readonly string SourcePathDebug =
            @"C:\Users\larsn\source\github\BearChess\BearChess\BearChessWin\bin\Debug";

        private readonly string SourceSqlitex64Testing =
            @"C:\Users\larsn\source\repos\BearChess\BearChess\BearChessWin\bin\Debug\x64";

        private readonly string SourceSqlitex64 =
            @"C:\Users\larsn\source\github\BearChess\BearChess\BearChessWin\bin\Debug\x64";

        private readonly string SourceSqlitex86Testing =
            @"C:\Users\larsn\source\repos\BearChess\BearChess\BearChessWin\bin\Debug\x86";

        private readonly string SourceSqlitex86 =
            @"C:\Users\larsn\source\github\BearChess\BearChess\BearChessWin\bin\Debug\x86";

        private readonly string SourcePathTesting =
            @"C:\Users\larsn\source\repos\BearChess\BearChess\BearChessWin\bin\debug";

        private readonly string TargetPath = @"G:\BearChess";

        public MainWindow()
        {
            InitializeComponent();
            textBlockSourcePathDebug.Text = SourcePathDebug;
            textBlockTargetPath.Text = TargetPath;
            textBlockVersion.Text = ReadVersion();
            var readDate = ReadDate();
            textBlockDate.Text = readDate.ToString("yyyy-MM-dd");
            textBlockFile.Text =  Path.Combine(TargetPath,$"Version {textBlockVersion.Text}", "BearChessWin.zip");
            CheckDeploy();
            if (File.Exists(textBlockFile.Text))
            {
                MessageBox.Show(this, $"{textBlockFile.Text} exists", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private DateTime ReadDate()
        {
         
            if (File.Exists(BearChessFileName))
            {
                var fileInfo = new FileInfo(BearChessFileName);
                return fileInfo.LastWriteTimeUtc;
            }
            return DateTime.MinValue;
        }

        private string ReadVersion()
        {
            if (File.Exists(AssemblyFileName))
            {
                var readAllLines = File.ReadAllLines(AssemblyFileName);
                foreach (var readAllLine in readAllLines)
                {
                    if (readAllLine.StartsWith("[assembly: AssemblyVersion"))
                    {
                        var strings = readAllLine.Split("\"".ToCharArray());
                        return strings[1];
                    }
                }
            }

            return string.Empty;
        }

        private void DoDeployTesting()
        {
            var tempDeployPath = Path.Combine(SourcePathTesting, "Deploy");
            var tempDeployPathx64 = Path.Combine(SourcePathTesting, "Deploy", "x64");
            var tempDeployPathx86 = Path.Combine(SourcePathTesting, "Deploy", "x86");
            Directory.CreateDirectory(tempDeployPath);
            Directory.CreateDirectory(Path.Combine(tempDeployPathx64));
            Directory.CreateDirectory(Path.Combine(tempDeployPathx86));
            File.Copy(Path.Combine(BearChessFileNameTesting), Path.Combine(tempDeployPath, "BearChessWin.exe"));
            File.Copy(Path.Combine(TeddyFileNameTesting), Path.Combine(tempDeployPath, "Teddy.exe"));
            if (File.Exists(EcoFileNameTesting))
            {
                File.Copy(Path.Combine(EcoFileNameTesting), Path.Combine(tempDeployPath, "bearchess.eco"));
            }

            if (File.Exists(DocumentFileName))
            {
                File.Copy(Path.Combine(DocumentFileName), Path.Combine(tempDeployPath, "BearChess.pdf"));
            }


            var tmpFiles = Directory.GetFiles(SourcePathTesting, "*.dll");

            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPath, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourcePathTesting, "*.pdb");

            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPath, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourcePathTesting, "*.config");

            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPath, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourcePathTesting, "*.xml");
            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPath, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourceSqlitex64Testing, "*.*");
            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPathx64, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourceSqlitex86Testing, "*.*");
            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPathx86, fileInfo.Name));
            }

            if (File.Exists(Path.Combine(SourcePathTesting, "BearChess.zip")))
            {
                File.Delete(Path.Combine(SourcePathTesting, "BearChess.zip"));
            }
            ZipFile.CreateFromDirectory(tempDeployPath, Path.Combine(SourcePathTesting, "BearChess.zip"),
                                        CompressionLevel.Optimal, false);
            tmpFiles = Directory.GetFiles(tempDeployPath, "*.*", SearchOption.AllDirectories);
            foreach (var s in tmpFiles)
            {
                File.Delete(s);
            }

            Directory.Delete(tempDeployPathx64);
            Directory.Delete(tempDeployPathx86);
            Directory.Delete(tempDeployPath);
            if (!Directory.Exists(Path.Combine(TargetPath, "Testing")))
            {
                Directory.CreateDirectory(Path.Combine(TargetPath, "Testing"));
            }

            
            File.Copy(Path.Combine(SourcePathTesting, "BearChess.zip"), Path.Combine(TargetPath, "Testing", "BearChessWin.zip"));
            if (File.Exists(DocumentFileName))
            {
                File.Copy(Path.Combine(DocumentFileName), Path.Combine(TargetPath, "Testing", "BearChess.pdf"));
            }
        }

        private void DoDeploy()
        {
            var tempDeployPath = Path.Combine(SourcePathDebug, "Deploy");
            var tempDeployPathx64 = Path.Combine(SourcePathDebug, "Deploy","x64");
            var tempDeployPathx86 = Path.Combine(SourcePathDebug, "Deploy","x86");
            Directory.CreateDirectory(tempDeployPath);
            Directory.CreateDirectory(Path.Combine(tempDeployPathx64));
            Directory.CreateDirectory(Path.Combine(tempDeployPathx86));
            File.Copy(Path.Combine(BearChessFileName), Path.Combine(tempDeployPath, "BearChessWin.exe"));
            File.Copy(Path.Combine(TeddyFileName), Path.Combine(tempDeployPath, "Teddy.exe"));
            if (File.Exists(EcoFileName))
            {
                File.Copy(Path.Combine(EcoFileName), Path.Combine(tempDeployPath, "bearchess.eco"));
            }

            if (File.Exists(DocumentFileName))
            {
                File.Copy(Path.Combine(DocumentFileName), Path.Combine(tempDeployPath, "BearChess.pdf"));
            }


            var tmpFiles = Directory.GetFiles(SourcePathDebug, "*.dll");

            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPath, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourcePathDebug, "*.config");

            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPath, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourcePathDebug, "*.xml");
            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPath, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourceSqlitex64, "*.*");
            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPathx64, fileInfo.Name));
            }

            tmpFiles = Directory.GetFiles(SourceSqlitex86, "*.*");
            foreach (var s in tmpFiles)
            {
                var fileInfo = new FileInfo(s);
                File.Copy(s, Path.Combine(tempDeployPathx86, fileInfo.Name));
            }

            if (File.Exists(Path.Combine(SourcePathDebug, "BearChess.zip")))
            {
                File.Delete(Path.Combine(SourcePathDebug, "BearChess.zip"));
            }
            ZipFile.CreateFromDirectory(tempDeployPath, Path.Combine(SourcePathDebug, "BearChess.zip"),
                                        CompressionLevel.Optimal, false);
            tmpFiles = Directory.GetFiles(tempDeployPath,"*.*",SearchOption.AllDirectories);
            foreach (var s in tmpFiles)
            {
                File.Delete(s);
            }

            Directory.Delete(tempDeployPathx64);
            Directory.Delete(tempDeployPathx86);
            Directory.Delete(tempDeployPath);
            if (!Directory.Exists(Path.Combine(TargetPath, $"Version {textBlockVersion.Text}")))
            {
                Directory.CreateDirectory(Path.Combine(TargetPath, $"Version {textBlockVersion.Text}"));
            }

            if (File.Exists(textBlockFile.Text))
            {
                File.Delete(textBlockFile.Text);
            }
            File.Copy(Path.Combine(SourcePathDebug, "BearChess.zip"), textBlockFile.Text);
            if (File.Exists(DocumentFileName))
            {
                File.Copy(Path.Combine(DocumentFileName), Path.Combine(TargetPath, $"Version {textBlockVersion.Text}","BearChess.pdf"));
            }
        }

        private bool CheckDeployTesting()
        {
            if (!Directory.Exists(TargetPath))
            {
                MessageBox.Show(this, "Target path not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Directory.Exists(SourcePathTesting))
            {
                MessageBox.Show(this, "Source path not exists", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }


            if (!File.Exists(AssemblyFileNameTesting))
            {
                MessageBox.Show(this, "Assembly file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!File.Exists(AssemblyFileNameTesting))
            {
                MessageBox.Show(this, "Assembly file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!File.Exists(BearChessFileNameTesting))
            {
                MessageBox.Show(this, "BearChessWin.exe file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!File.Exists(EcoFileNameTesting))
            {
                MessageBox.Show(this, "bearchess.eco file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }


            return true;
        }

        private bool CheckDeploy()
        {
            if (!Directory.Exists(TargetPath))
            {
                MessageBox.Show(this, "Target path not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Directory.Exists(SourcePathDebug))
            {
                MessageBox.Show(this, "Source path not exists", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }


            if (!File.Exists(AssemblyFileName))
            {
                MessageBox.Show(this, "Assembly file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!File.Exists(AssemblyFileName))
            {
                MessageBox.Show(this, "Assembly file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!File.Exists(BearChessFileName))
            {
                MessageBox.Show(this, "BearChessWin.exe file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!File.Exists(EcoFileName))
            {
                MessageBox.Show(this, "bearchess.eco file not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }


            return true;
        }

        private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDeploy())
                {
                    DoDeploy();
                    MessageBox.Show(this, "Done", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonGoTesting_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDeployTesting())
                {
                    DoDeployTesting();
                    MessageBox.Show(this, "Done", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
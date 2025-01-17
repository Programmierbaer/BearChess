using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ConfirmBoardImageWindow.xaml
    /// </summary>
    public partial class ConfirmBoardImageWindow : Window
    {
        private readonly string _path;
        public BoardFieldsSetup BoardFieldsSetup { get; private set; }

        private string _whiteFileName;
        private string _blackFileName;
        private readonly HashSet<string> _allNames;
        private readonly ResourceManager _rm;

        public ConfirmBoardImageWindow(string path, string[] allNames)
        {
            _path = path;
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            textBoxName.Text = string.Empty;
            textBoxName.ToolTip = _rm.GetString("GiveBoardAName");
            _allNames = new HashSet<string>(allNames);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show(_rm.GetString("BoardNameRequired"), _rm.GetString("MissingInformation"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_allNames.Contains(textBoxName.Text))
            {
                MessageBox.Show(_rm.GetString("BoardNameAlreadyTaken"), _rm.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Configuration.Instance.Standalone)
            {
                _whiteFileName = _whiteFileName.Replace(Configuration.Instance.BinPath, @".\");
                _blackFileName = _blackFileName.Replace(Configuration.Instance.BinPath, @".\");
            }
            BoardFieldsSetup = new BoardFieldsSetup()
            {
                Id = "board_" + Guid.NewGuid().ToString("N"),
                Name = textBoxName.Text,
                WhiteFileName = _whiteFileName,
                BlackFileName = _blackFileName
            };
           DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void FillGrid()
        {

           chessBoardUserControl.FillGrid(_whiteFileName,_blackFileName);
        }

        private void ConfirmBoardImageWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _whiteFileName = string.Empty;
                _blackFileName = string.Empty;
                var allFiles = Directory.GetFiles(_path, "*.png", SearchOption.TopDirectoryOnly);
                foreach (var allFile in allFiles)
                {
                    var fileInfo = new FileInfo(allFile);
                    if (fileInfo.Name.Equals("white.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("w.png", StringComparison.OrdinalIgnoreCase))
                    {
                        _whiteFileName = fileInfo.FullName;
                    }

                    if (fileInfo.Name.Equals("black.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("b.png", StringComparison.OrdinalIgnoreCase))
                    {
                        _blackFileName = fileInfo.FullName;
                    }
                }

                if (string.IsNullOrWhiteSpace(_whiteFileName) || string.IsNullOrWhiteSpace(_blackFileName))
                {
                    MessageBox.Show($"{_rm.GetString("NoPNGFilesFound")}{Environment.NewLine}{_rm.GetString("LookingForWhite")}{Environment.NewLine}{_rm.GetString("LookingForBlack")}.", _rm.GetString("Error"), MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    DialogResult = false;
                    return;
                }
                FillGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_rm.GetString("ErrorReadingFiles")}{Environment.NewLine}{ex.Message}", _rm.GetString("Error"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
                DialogResult = false;
            }
        }
    }
}

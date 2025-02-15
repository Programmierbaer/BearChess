using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using www.SoLaNoSoft.com.BearChess.ChessnutAirLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessBase;
using System.Resources;
using System.IO;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
{
    /// <summary>
    /// Interaktionslogik für ConfigureChessnutWindow.xaml
    /// </summary>
    public partial class ConfigureChessnutWindow : Window
    {
        private readonly string _fileName;
        private readonly Configuration _configuration;
        private readonly bool _useBluetooth;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private readonly ResourceManager _rm;

        public string SelectedPortName => _useBluetooth ? "BTLE" : "HID";
        public ConfigureChessnutWindow(string boardName, Configuration configuration, bool useBluetooth) : this(boardName, configuration, useBluetooth, configuration.FolderPath)
        { }

        public ConfigureChessnutWindow(string boardName, Configuration configuration, bool useBluetooth, string configPath)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _configuration = configuration;
            _useBluetooth = useBluetooth;
            if (boardName.Equals(Constants.ChessnutAir))
            {

                Title = $"{_rm.GetString("ConfigureTitle")} Chessnut Air/Air+/Pro";
                _fileName = Path.Combine(configPath, ChessnutAirLoader.EBoardName,
                    $"{ChessnutAirLoader.EBoardName}Cfg.xml");
            }
            else
            {
                Title = $"{_rm.GetString("ConfigureTitle")} {boardName}";
                _fileName = Path.Combine(configPath, ChessnutGoLoader.EBoardName,
                    $"{ChessnutGoLoader.EBoardName}Cfg.xml");
            }
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            checkBoxMoveLine.IsChecked = _eChessBoardConfiguration.ShowMoveLine;
            checkBoxOwnMoves.IsChecked = _eChessBoardConfiguration.ShowOwnMoves;
            checkBoxPossibleMoves.IsChecked = _eChessBoardConfiguration.ShowPossibleMoves;
            checkBoxBestMove.IsChecked = _eChessBoardConfiguration.ShowPossibleMovesEval;
        }
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;
            _eChessBoardConfiguration.ShowOwnMoves = checkBoxOwnMoves.IsChecked.HasValue && checkBoxOwnMoves.IsChecked.Value;
            _eChessBoardConfiguration.ShowPossibleMoves = checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            _eChessBoardConfiguration.ShowPossibleMovesEval = checkBoxBestMove.IsChecked.HasValue && checkBoxBestMove.IsChecked.Value;
            _eChessBoardConfiguration.UseBluetooth = _useBluetooth;
            _eChessBoardConfiguration.PortName = _useBluetooth ? "BTLE" : "HID";
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CheckBoxOwnMoves_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxBestMove.IsEnabled = true;
            checkBoxPossibleMoves.IsEnabled = true;
        }

        private void CheckBoxOwnMoves_OnChecked(object sender, RoutedEventArgs e)
        {
            checkBoxBestMove.IsEnabled = false;
            checkBoxPossibleMoves.IsEnabled = false;
        }

        private void CheckBoxPossibleMoves_OnChecked(object sender, RoutedEventArgs e)
        {
            checkBoxOwnMoves.IsEnabled = false;
            checkBoxOwnMoves.IsChecked = false;
        }

        private void CheckBoxPossibleMoves_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxOwnMoves.IsEnabled = !checkBoxBestMove.IsChecked.Value;
        }

        private void CheckBoxBesteMove_OnChecked(object sender, RoutedEventArgs e)
        {
            checkBoxOwnMoves.IsEnabled = false;
            checkBoxOwnMoves.IsChecked = false;
        }

        private void CheckBoxBesteMove_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxOwnMoves.IsEnabled = !checkBoxPossibleMoves.IsChecked.Value;
        }
    }
}

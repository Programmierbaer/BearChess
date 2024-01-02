using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.IO;
using www.SoLaNoSoft.com.BearChess.ChessnutAirLoader;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Implementations;


namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für WinConfigureChessnut.xaml
    /// </summary>
    public partial class WinConfigureChessnut : Window
    {
        private readonly string _fileName;
        private readonly Configuration _configuration;
        private readonly bool _useBluetooth;
        private readonly bool _showMoveLineChessnutAir;
        private readonly bool _showOwnMoveChessnutAir;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;

        public WinConfigureChessnut(Configuration configuration, bool useBluetooth)
        {
            InitializeComponent();
            _configuration = configuration;
            _useBluetooth = useBluetooth;
            _fileName = Path.Combine(_configuration.FolderPath, ChessnutAirLoader.EBoardName,
                         $"{ChessnutAirLoader.EBoardName}Cfg.xml");
            _showMoveLineChessnutAir = bool.Parse(_configuration.GetConfigValue("showmovelineChessnutAir", "false"));
            _showOwnMoveChessnutAir = bool.Parse(_configuration.GetConfigValue("showOwnMoveChessnutAir", "true"));
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

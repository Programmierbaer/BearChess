using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.HoSLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureHoS.xaml
    /// </summary>
    public partial class WinConfigureHoS : Window
    {
        
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;

        public string SelectedPortName => "HID";

        public WinConfigureHoS(Configuration configuration)
        {
            InitializeComponent();
        
            _fileName = Path.Combine(configuration.FolderPath, HoSLoader.EBoardName,
                $"{HoSLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            _eChessBoardConfiguration.UseBluetooth = false;
            checkBoxDefault.IsChecked = true;
            checkBoxOwnMoves.IsChecked = _eChessBoardConfiguration.ShowOwnMoves;
            checkBoxPossibleMoves.IsChecked = _eChessBoardConfiguration.ShowPossibleMoves;
            checkBoxBestMove.IsChecked = _eChessBoardConfiguration.ShowPossibleMovesEval;
        }

      
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
         
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

      
    }
}

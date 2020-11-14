using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für DatabaseWindow.xaml
    /// </summary>
    public partial class DatabaseWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly PgnLoader _pgnLoader;
        private BindingList<PgnGame> _pgnGames = new BindingList<PgnGame>();

        public event EventHandler<PgnGame> SelectedGameChanged;

        public DatabaseWindow(Configuration configuration, PgnLoader pgnLoader)
        {
            _configuration = configuration;
            InitializeComponent();
            _pgnLoader = pgnLoader;
            Top = _configuration.GetWinDoubleValue("DatabaseWindowTop", Configuration.WinScreenInfo.Top);
            Left = _configuration.GetWinDoubleValue("DatabaseWindowLeft", Configuration.WinScreenInfo.Left);
            var fileName = _configuration.GetConfigValue("DatabaseFile", string.Empty);
            if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
            {
                _pgnLoader.Load(fileName);
                _pgnGames = _pgnLoader.Games;
                dataGridGames.ItemsSource = _pgnGames;
                Title = $"Games on: {fileName}";
            }
        }

        private void ButtonFileManager_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Games|*.pgn;" };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {

                _pgnLoader.Load(openFileDialog.FileName);
                _pgnGames = _pgnLoader.Games;
                dataGridGames.ItemsSource = _pgnGames;
                _configuration.SetConfigValue("DatabaseFile", openFileDialog.FileName);
                Title = $"Games on: {openFileDialog.FileName}";
            }
        }

        private void DatabaseWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("DatabaseWindowTop", Top);
            _configuration.SetDoubleValue("DatabaseWindowLeft", Left);
        }


        private void ButtonNewFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog() { Filter = "Games|*.pgn;" };
            var showDialog = saveFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                if (File.Exists(saveFileDialog.FileName))
                {
                    File.Delete(saveFileDialog.FileName);
                }
                _pgnLoader.Load(saveFileDialog.FileName);
                _pgnGames = _pgnLoader.Games;
                dataGridGames.ItemsSource = _pgnGames;
                _configuration.SetConfigValue("DatabaseFile",saveFileDialog.FileName);
                Title = $"Games on: {saveFileDialog.FileName}";
            }
        }

        private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DataGridGames_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridGames.SelectedItem is PgnGame pgnGame)
            {
                OnSelectedGamedChanged(pgnGame);
            }
        }
        protected virtual void OnSelectedGamedChanged(PgnGame e)
        {
            SelectedGameChanged?.Invoke(this, e);
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridGames.SelectedItem is PgnGame pgnGame)
            {
                if (MessageBox.Show("Delete selected game?", "Delete game", MessageBoxButton.YesNo,
                    MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    _pgnLoader.DeleteGame(pgnGame);
                }
            }
        }
    }
}

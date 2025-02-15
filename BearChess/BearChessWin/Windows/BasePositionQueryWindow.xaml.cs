using System.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für BasePositionQueryWindow.xaml
    /// </summary>
    public partial class BasePositionQueryWindow : Window
    {
        public bool StartNewGame { get; private set; }
        public bool ReStartGame { get; private set; }
        public bool SaveGame { get; private set; }

        public BasePositionQueryWindow(bool showSaveGame)
        {
            InitializeComponent();
            buttonSave.Visibility = showSaveGame ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = true;
            ReStartGame = false;
            SaveGame = false;
            DialogResult = true;
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = false;
            ReStartGame = false;
            SaveGame = false;
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonReStart_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = false;
            ReStartGame = true;
            SaveGame = false;
            DialogResult = true;
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = false;
            ReStartGame = false;
            SaveGame = true;
            DialogResult = true;
        }
    }
}

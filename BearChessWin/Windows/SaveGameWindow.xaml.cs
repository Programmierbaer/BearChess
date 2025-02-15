using System;
using System.Resources;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für SaveGameWindow.xaml
    /// </summary>
    public partial class SaveGameWindow : Window
    {
        public string White => textBoxWhite.Text;
        public string Black => textBoxBlack.Text;
        public string Result => GetResult();
        public string GameEvent => textBoxEvent.Text;
        public string GameDate => textBoxDate.Text;

        public bool ReplaceGame { get; private set; }
        private readonly ResourceManager _rm;


        public SaveGameWindow(string moveList)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            textBoxDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
            textBoxEvent.Text = Constants.BearChess;
            textBlockMoves.Text = moveList;
            ReplaceGame = false;
        }

        public SaveGameWindow(string playerName, string white, string black, string result, string eventName, string date,
                              string moveList) : this(moveList)
        {
            textBoxWhite.Text = white.Equals(Constants.Player) ? playerName : white;
            textBoxBlack.Text = black.Equals(Constants.Player) ? playerName : black;
            textBoxEvent.Text = eventName;
            textBoxDate.Text = date;
            if (result.StartsWith("1/2"))
            {
                radioButton11.IsChecked = true;
            }
            else if (result.StartsWith("1"))
            {
                radioButton10.IsChecked = true;
            }
            else if (result.StartsWith("0"))
            {
                radioButton01.IsChecked = true;
            }
            else
            {
                radioButtonStar.IsChecked = true;
            }

        }

        private string GetResult()
        {
            if (radioButton10.IsChecked.HasValue && radioButton10.IsChecked.Value)
            {
                return "1-0";
            }
            if (radioButton01.IsChecked.HasValue && radioButton01.IsChecked.Value)
            {
                return "0-1";
            }
            if (radioButtonStar.IsChecked.HasValue && radioButtonStar.IsChecked.Value)
            {
                return "*";
            }

            return "1/2-1/2";
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            ReplaceGame = false;
            DialogResult = Check();
        }

        private void ButtonReplace_OnClick(object sender, RoutedEventArgs e)
        {
            ReplaceGame = true;
            DialogResult = Check();
        }

        private bool Check()
        {
            if (string.IsNullOrWhiteSpace(White))
            {
                MessageBox.Show(_rm.GetString("WhitePlayerMissing"), _rm.GetString("Required"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Black))
            {
                MessageBox.Show(_rm.GetString("BlackPlayerMissing"), _rm.GetString("Required"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(GameEvent))
            {
                MessageBox.Show(_rm.GetString("EventMissing"), _rm.GetString("Required"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}

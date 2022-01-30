using System;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für SaveGameWindow.xaml
    /// </summary>
    public partial class SaveGameWindow : Window
    {
        public string White => textBoxWhite.Text;
        public string Black => textBoxBlack.Text;
        public string Result => getResult();
        public string GameEvent => textBoxEvent.Text;
        public string GameDate => textBoxDate.Text;


        public SaveGameWindow(string moveList)
        {
            InitializeComponent();
            textBoxDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
            textBoxEvent.Text = Constants.BearChess;
            textBlockMoves.Text = moveList;
        }

        public SaveGameWindow(string playerName, string white, string black, string result, string eventName, string date,
                              string moveList) : this(moveList)
        {
            textBoxWhite.Text = white.Equals("Player") ? playerName : white;
            textBoxBlack.Text = black.Equals("Player") ? playerName : black;
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

        private string getResult()
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
            if (string.IsNullOrWhiteSpace(White))
            {
                MessageBox.Show("White player missing", "Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(Black))
            {
                MessageBox.Show("Black player missing", "Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(GameEvent))
            {
                MessageBox.Show("Event player missing", "Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }
    }
}

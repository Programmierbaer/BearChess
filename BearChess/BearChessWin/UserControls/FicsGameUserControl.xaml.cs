using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für FicsGameUserControl.xaml
    /// </summary>
    public partial class FicsGameUserControl : UserControl
    {

        public class SayEventArgs : EventArgs
        {
            public SayEventArgs(string message)
            {
                Message = message;
            }

            public string Message { get; }
        }

        public event EventHandler<SayEventArgs> SayEvent;

        public FicsGameUserControl()
        {
            InitializeComponent();
            textBoxOpponentMessage.IsEnabled = false;
            buttonCommonSend.IsEnabled = false;
        }

        public void SetGameInformation(FicsNewGameInfo newGameInfo)
        {
            textBlockPlayerWhite.Content = newGameInfo.PlayerWhite;
            textBlockPlayerBlack.Content = newGameInfo.PlayerBlack;
            textBlockPlayerWhiteElo.Content = newGameInfo.EloWhite;
            textBlockPlayerBlackElo.Content = newGameInfo.EloBlack;
            if (newGameInfo.Time2.Equals("0"))
            {
                textBlockTimeControl.Content = $"{newGameInfo.Time1} min.";
            }
            else
            {
                textBlockTimeControl.Content = $"{newGameInfo.Time1} min. with {newGameInfo.Time2} sec. increment";
            }

            textBlockInformation.Content = $"{newGameInfo.Rated} {newGameInfo.GameType}";
            Background = new SolidColorBrush(Colors.LightYellow);
            textBoxOpponentMessage.IsEnabled = true;
            buttonCommonSend.IsEnabled = true;
        }

        public void SetGameInformation(string information)
        {
            textBlockInformation.Content = information;
        }

        public void GameFinished()
        {
            Background = new SolidColorBrush(Colors.WhiteSmoke);
        }

        public void ClearInformation()
        {
            textBlockPlayerWhite.Content = string.Empty;
            textBlockPlayerBlack.Content = string.Empty;
            textBlockPlayerWhiteElo.Content = string.Empty;
            textBlockPlayerBlackElo.Content = string.Empty;
            textBlockTimeControl.Content = string.Empty;
            textBlockInformation.Content = "No game";
            textBoxOpponentMessage.IsEnabled = false;
            buttonCommonSend.IsEnabled = false;
        }

        private void ButtonSendCommand_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxOpponentMessage.Text))
            {
                OnSendEvent(new SayEventArgs($"say {textBoxOpponentMessage.Text}"));
            }
        }

        private void OnSendEvent(SayEventArgs e)
        {
            SayEvent?.Invoke(this, e);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrWhiteSpace(textBoxOpponentMessage.Text))
                {
                    OnSendEvent(new SayEventArgs($"say {textBoxOpponentMessage.Text}"));
                }
            }
        }
    }
}

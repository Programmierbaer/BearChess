using System.Windows;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für DefineSpeechWindow.xaml
    /// </summary>
    public partial class DefineSpeechWindow : Window
    {
        private readonly Configuration _config;

        public DefineSpeechWindow(Configuration config)
        {
            _config = config;
            InitializeComponent();
            InitConfig();
        }

        private void InitConfig()
        {
            textBoxKing.Text = _config.GetConfigValue("SpeechKing", string.Empty);
            textBoxQueen.Text = _config.GetConfigValue("SpeechQueen", string.Empty);
            textBoxRook.Text = _config.GetConfigValue("SpeechRook", string.Empty);
            textBoxKnight.Text = _config.GetConfigValue("SpeechKnight", string.Empty);
            textBoxBishop.Text = _config.GetConfigValue("SpeechBishop", string.Empty);
            textBoxPawn.Text = _config.GetConfigValue("SpeechPawn", string.Empty);
            textBoxAgainst.Text = _config.GetConfigValue("SpeechAgainst", string.Empty);
            textBoxCastleKingSide.Text = _config.GetConfigValue("SpeechCastleKing", string.Empty);
            textBoxCastleQueenSide.Text = _config.GetConfigValue("SpeechCastleQueen", string.Empty);
            textBoxCheck.Text = _config.GetConfigValue("SpeechCheck", string.Empty);
            textBoxCheckMate.Text = _config.GetConfigValue("SpeechCheckMate", string.Empty);
            textBoxDraw.Text = _config.GetConfigValue("SpeechDraw", string.Empty);
            textBoxFrom.Text = _config.GetConfigValue("SpeechFrom", string.Empty);
            textBoxTo.Text = _config.GetConfigValue("SpeechTo", string.Empty);
            textBoxTakes.Text = _config.GetConfigValue("SpeechTakes", string.Empty);
            textBoxGameFinished.Text = _config.GetConfigValue("SpeechGameFinished", string.Empty);
            textBoxWinsByMate.Text = _config.GetConfigValue("SpeechWinsByMate", string.Empty);
            textBoxWinsByScore.Text = _config.GetConfigValue("SpeechWinsByScore", string.Empty);
            textBoxNewGame.Text = _config.GetConfigValue("SpeechNewGame", string.Empty);
            textBoxWelcome.Text = _config.GetConfigValue("SpeechWelcome", string.Empty);
            textBoxFICSWelcome.Text = _config.GetConfigValue("SpeechFICSWelcome", string.Empty);
            textBoxFICSConnectedAs.Text = _config.GetConfigValue("SpeechFICSConnectedAs", string.Empty);
            textBoxFICSChallenge.Text = _config.GetConfigValue("SpeechFICSChallenge", string.Empty);
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
           _config.SetConfigValue("SpeechKing",textBoxKing.Text);
           _config.SetConfigValue("SpeechQueen",textBoxQueen.Text);
           _config.SetConfigValue("SpeechRook",textBoxRook.Text);
           _config.SetConfigValue("SpeechKnight",textBoxKnight.Text);
           _config.SetConfigValue("SpeechBishop",textBoxBishop.Text);
           _config.SetConfigValue("SpeechPawn",textBoxPawn.Text);
           _config.SetConfigValue("SpeechAgainst",textBoxAgainst.Text);
           _config.SetConfigValue("SpeechCastleKing",textBoxCastleKingSide.Text);
           _config.SetConfigValue("SpeechCastleQueen",textBoxCastleQueenSide.Text);
           _config.SetConfigValue("SpeechCastleCheck",textBoxCheck.Text);
           _config.SetConfigValue("SpeechCastleCheckMate",textBoxCheckMate.Text);
           _config.SetConfigValue("SpeechCheck",textBoxCheck.Text);
           _config.SetConfigValue("SpeechCheckMate",textBoxCheckMate.Text);
           _config.SetConfigValue("SpeechDraw",textBoxDraw.Text);
           _config.SetConfigValue("SpeechFrom",textBoxFrom.Text);
           _config.SetConfigValue("SpeechTo",textBoxTo.Text);
           _config.SetConfigValue("SpeechTakes",textBoxTakes.Text);
           _config.SetConfigValue("SpeechGameFinished", textBoxGameFinished.Text);
           _config.SetConfigValue("SpeechWinsByMate", textBoxWinsByMate.Text);
           _config.SetConfigValue("SpeechWinsByScore", textBoxWinsByScore.Text);
           _config.SetConfigValue("SpeechNewGame", textBoxNewGame.Text);
           _config.SetConfigValue("SpeechWelcome", textBoxWelcome.Text);
           _config.SetConfigValue("SpeechFICSWelcome", textBoxFICSWelcome.Text);
           _config.SetConfigValue("SpeechFICSConnectedAs", textBoxFICSConnectedAs.Text);
           _config.SetConfigValue("SpeechFICSChallenge", textBoxFICSChallenge.Text);
           DialogResult = true;
        }
    }
}

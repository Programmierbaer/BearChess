using System;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        private readonly ResourceManager _rm;
        private readonly string _currentHelpText;
        private readonly ISpeech _synthesizer = null;
        private readonly bool _blindUser;

        public string FirstName => textBlockFirstName.Text.Replace(",",string.Empty);
        public string LastName => textBlockLastName.Text.Replace(",", string.Empty);

        public PlayerWindow(string playerName)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _currentHelpText = _rm.GetString("DefinePlayerSpeech");
            _blindUser = bool.Parse(Configuration.Instance.GetConfigValue("blindUser", "false"));

            if (_blindUser || bool.Parse(Configuration.Instance.GetConfigValue("speechActive", "true")))
            {
                _synthesizer = BearChessSpeech.Instance;
            }
            var strings = playerName.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            textBlockFirstName.Text = strings.Length > 1 ? strings[1].Trim() : string.Empty;
            textBlockLastName.Text = strings.Length > 0 ? strings[0].Trim() : string.Empty;
            textBlockFirstName.Focus();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void PlayerWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    _synthesizer?.SpeakAsync(_currentHelpText);
                    return;
                }
                _synthesizer?.SpeakAsync(_rm.GetString("DefinePlayerSpeech"));
            }
        }
    }
}

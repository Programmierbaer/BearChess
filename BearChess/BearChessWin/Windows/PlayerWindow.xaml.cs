using System;
using System.ComponentModel;
using System.Resources;
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

        public string FirstName => textBlockFirstName.Text.Replace(",", string.Empty);
        public string LastName => textBlockLastName.Text.Replace(",", string.Empty);

        public PlayerWindow(string playerName)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _currentHelpText = _rm.GetString("DefinePlayerSpeech");
            _blindUser = Configuration.Instance.GetBoolValue("blindUser", false);

            if (_blindUser || Configuration.Instance.GetBoolValue("speechActive", true))
            {
                _synthesizer = BearChessSpeech.Instance;
            }

            _synthesizer?.Clear();
            _synthesizer?.SpeakAsync(_currentHelpText);
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
            if (_blindUser && e.Key == Key.F1)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    _synthesizer?.SpeakAsync(_currentHelpText);
                    return;
                }

                _synthesizer?.SpeakAsync(_rm.GetString("DefinePlayerSpeech"));
            }
        }

        private void TextBlockFirstName_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync($"{_rm.GetString("EditField")} {_rm.GetString("FirstName")}");
            _synthesizer?.SpeakAsync($"{_rm.GetString("CurrentInput")} {textBlockFirstName.Text}");
        }

        private void TextBlockLastName_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync($"{_rm.GetString("EditField")} {_rm.GetString("LastName")}");
            _synthesizer?.SpeakAsync($"{_rm.GetString("CurrentInput")} {textBlockLastName.Text}");
        }

        private void ButtonOk_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync($"{_rm.GetString("Button")} {_rm.GetString("Ok")}");
            _synthesizer?.SpeakAsync($"{_rm.GetString("CurrentInput")}");
            _synthesizer?.SpeakAsync($"{_rm.GetString("FirstName")} {textBlockFirstName.Text}");
            _synthesizer?.SpeakAsync($"{_rm.GetString("LastName")} {textBlockLastName.Text}");
        }

        private void ButtonCancel_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync($"{_rm.GetString("Button")} {_rm.GetString("Cancel")}");
        }

        private void PlayerWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _synthesizer?.Clear();
        }
    }
}
using System;
using System.Resources;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für BlindMainWindow.xaml
    /// </summary>
    public partial class BlindMainWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;
        public string SelectedMenuAction { get; private set; }

        public BlindMainWindow(Configuration configuration, bool isConnected)
        {
            _configuration = configuration;
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            SelectedMenuAction = string.Empty;
            ButtonConnect.Visibility = isConnected ? Visibility.Collapsed : Visibility.Visible;
            ButtonDisconnect.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            _synthesizer?.SpeakAsync(_rm.GetString("BearChessMainWindowsSpeech"));
        }


        private void Selector_OnSelected(object sender, RoutedEventArgs e)
        {
            var helpText = AutomationProperties.GetHelpText(sender as UIElement);
            if (!string.IsNullOrWhiteSpace(helpText))
            {
                _synthesizer.SpeakAsync(helpText);
            }

        }

        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (!button.Tag.Equals("NewGame"))
                {
                    _synthesizer.Clear();
                }

                _synthesizer.SpeakAsync($"{_rm.GetString("Button")} {button.Content}");
            }
        }

        private void ActionMenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag.Equals("Exit"))
                {
                    SelectedMenuAction = string.Empty;
                    _synthesizer?.Clear();
                    DialogResult = false;
                }
                else
                {
                    SelectedMenuAction = button.Tag.ToString();
                    DialogResult = true;
                }
            }
        }

        private void MainMenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag.Equals("Exit"))
                {
                    SelectedMenuAction = string.Empty;
                    _synthesizer?.Clear();
                    DialogResult = false;
                }
               
            }
        }

     

        private void BlindMainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ButtonNewGame.Focus();

        }

        private void BlindMainWindow_OnClosed(object sender, EventArgs e)
        {
            _synthesizer?.Clear();
        }
    }
}
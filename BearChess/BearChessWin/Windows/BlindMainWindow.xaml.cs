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
        private readonly bool _blindUserSaySelection;
        // public event EventHandler<IBookMoveBase> SelectedMoveChanged;
        public string SelectedMenuAction { get; private set; }

        public BlindMainWindow(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            _blindUserSaySelection = bool.Parse(_configuration.GetConfigValue("blindUserSaySelection", "false"));
            SelectedMenuAction = string.Empty;
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
            //var helpText = AutomationProperties.GetHelpText(sender as UIElement);
            //if (!string.IsNullOrWhiteSpace(helpText))
            //{
            //    _synthesizer.SpeakAsync(helpText);
            //    return;
            //}
            if (sender is Button button)
            {
                _synthesizer.SpeakAsync($"{_rm.GetString("Button")} {button.Content}");
            }

        }

        private void ActionMenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag.Equals("Main"))
                {
                    TabControlMain.SelectedIndex = 0;
                    return;
                }

                SelectedMenuAction = button.Tag.ToString();
                DialogResult = true;
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
                foreach (var item in TabControlMain.Items)
                {
                    if (item is TabItem tabItem)
                    {
                        if (tabItem.Tag!=null &&  tabItem.Tag.Equals(button.Tag))
                        {
                            _synthesizer?.Clear();
                            TabControlMain.SelectedItem = tabItem;
                            return;
                        }
                    }
                }
            }
        }

     
        private void TabControlMain_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                foreach (var item in TabControlMain.Items)
                {
                    if (item is TabItem tabItem)
                    {
                        if (tabItem.IsSelected)
                        {
                            var helpText = AutomationProperties.GetHelpText(tabItem);
                            if (!string.IsNullOrWhiteSpace(helpText))
                            {
                                _synthesizer.SpeakAsync(helpText);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void BlindMainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            TabControlMain.SelectedIndex = 0;
            TabItemMain.Focus();
        }
    }
}
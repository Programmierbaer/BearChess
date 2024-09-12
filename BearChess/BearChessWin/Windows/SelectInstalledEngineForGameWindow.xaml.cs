using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SelectInstalledEngineForGameWindow.xaml
    /// </summary>
    public partial class SelectInstalledEngineForGameWindow : Window
    {
        private readonly ObservableCollection<UciInfo> _uciInfos;
        private readonly bool _blindUser;
        private readonly ISpeech _synthesizer = null;
        private readonly ResourceManager _rm;
        private string _currentHelpText;
        public UciInfo SelectedEngine => (UciInfo)dataGridEngine.SelectedItem;

        public SelectInstalledEngineForGameWindow()
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            Top = Configuration.Instance.GetWinDoubleValue("InstalledEngineForGameWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth, "300");
            Left = Configuration.Instance.GetWinDoubleValue("InstalledEngineForGameWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth, "400");
            Height = Configuration.Instance.GetDoubleValue("InstalledEngineForGameWindowHeight", "390");
            Width = Configuration.Instance.GetDoubleValue("InstalledEngineForGameWindowWidth", "500");
            _blindUser = bool.Parse(Configuration.Instance.GetConfigValue("blindUser", "false"));
            
            if (_blindUser)
            {
                _synthesizer = BearChessSpeech.Instance;
            }
            _currentHelpText = _rm.GetString("SelectAnEngineSpeech");
            _synthesizer?.SpeakAsync(_currentHelpText);
        }

        public SelectInstalledEngineForGameWindow(IEnumerable<UciInfo> uciInfos, string lastEngineId) : this()
        {
            _uciInfos = new ObservableCollection<UciInfo>(uciInfos.Where(u => !u.IsPlayer && !u.IsChessServer).OrderBy(e => e.Name).ToList());
            var firstOrDefault = _uciInfos.FirstOrDefault(u => u.Name.Equals(lastEngineId));
            if (firstOrDefault == null)
            {
                firstOrDefault = _uciInfos.Count > 0 ? _uciInfos[0] : null;
            }

            dataGridEngine.ItemsSource = _uciInfos;
            dataGridEngine.Focus();
            if (firstOrDefault != null)
            {
                dataGridEngine.SelectedIndex = _uciInfos.IndexOf(firstOrDefault);
            }

            if (dataGridEngine.SelectedItem != null)
            {
                dataGridEngine.ScrollIntoView(dataGridEngine.SelectedItem);
              
            }
        }

        private void DataGridEngine_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonOk.IsEnabled = dataGridEngine.SelectedItems.Count == 1;
            if (dataGridEngine.SelectedItem!=null)
            {
                if (dataGridEngine.SelectedItem is UciInfo selectedItem)
                {
                   _currentHelpText =  selectedItem.Name;
                   _synthesizer?.Clear();
                   _synthesizer?.SpeakAsync(_currentHelpText);
                }
            }
        }

        private void DataGridEngine_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedEngine != null)
            {
                DialogResult = true;
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TextBoxFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var strings = textBoxFilter.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length > 0)
            {
                List<UciInfo> uciInfos = new List<UciInfo>(_uciInfos);
                foreach (var s in strings)
                {
                    uciInfos.RemoveAll(r => !r.Name.ContainsCaseInsensitive(s));
                }

                dataGridEngine.ItemsSource = uciInfos.Distinct().OrderBy(u => u.Name);
                return;
            }
            dataGridEngine.ItemsSource = _uciInfos.OrderBy(u => u.Name);
        }

        private void SelectInstalledEngineForGameWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Configuration.Instance.SetDoubleValue("InstalledEngineForGameWindowLeft", Left);
            Configuration.Instance.SetDoubleValue("InstalledEngineForGameWindowTop", Top);
            Configuration.Instance.SetDoubleValue("InstalledEngineForGameWindowWidth", Width);
            Configuration.Instance.SetDoubleValue("InstalledEngineForGameWindowHeight", Height);
        }

        private void DataGridEngine_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var u = e.OriginalSource as UIElement;
            if (e.Key == Key.Enter && u != null)
            {
                e.Handled = true;
                DialogResult = true;
            }

            if (e.Key == Key.Tab && u != null)
            {
                e.Handled = true;
                buttonOk.Focus();
            }
        }

       

        private void SelectInstalledEngineForGameWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                if (Keyboard.Modifiers==ModifierKeys.Control)
                {
                    _synthesizer?.SpeakAsync(_currentHelpText);
                    return;
                }
                _synthesizer?.SpeakAsync(_rm.GetString("SelectAnEngineSpeech"));
            }

        }
    }
}

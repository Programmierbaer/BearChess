using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für QueryEloDialogWindow.xaml
    /// </summary>
    public partial class QueryEloDialogWindow : Window
    {
        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;
        private bool _initialized = false;
        private bool _firstTime = true;

        public int Elo => numericUpDownUserControl.Value;
        public QueryDialogWindow.QueryDialogResult QueryResult { get; set; }

        public QueryEloDialogWindow(int minValue, int maxValue, int currentValue)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            TextBlockMinValue.Text = minValue.ToString();
            TextBlockMaxValue.Text = maxValue.ToString();
            numericUpDownUserControl.MinValue = minValue;
            numericUpDownUserControl.MaxValue = maxValue;
            numericUpDownUserControl.Value = currentValue;
            _synthesizer?.Clear();
            //_initialized = true;
        }

        private void numericUpDownUserControl_ValueChanged(object sender, int e)
        {
            if (_initialized)
            {
                _synthesizer?.Clear();
                _synthesizer?.Speak($"{e} ELO");
            }
        }
        private void YesButton_GotFocus(object sender, RoutedEventArgs e)
        {

            var helpText = AutomationProperties.GetHelpText(sender as UIElement);
            _synthesizer?.Speak(helpText);
        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogWindow.QueryDialogResult() { No = false, Yes = false, Previous = true, Cancel = false, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogWindow.QueryDialogResult() { No = false, Yes = false, Previous = false, Cancel = true, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
        }

        private void numericUpDownUserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_initialized)
            {
                //_synthesizer?.Clear();
                if (_firstTime)
                {
                    _synthesizer?.Speak($"{_rm.GetString("MinimumELO")} {numericUpDownUserControl.MinValue}");
                    _synthesizer?.Speak($"{_rm.GetString("MaximumELO")} {numericUpDownUserControl.MaxValue}");
                    _firstTime = false;
                }
                else
                {
                    _synthesizer?.Speak($"{numericUpDownUserControl.Value} ELO");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _initialized = true;
        }

        private void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogWindow.QueryDialogResult() { No = false, Yes = true, Previous = false, Cancel = false, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
        }
    }
}

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
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessTools;
using static www.SoLaNoSoft.com.BearChessWin.Windows.QueryDialogWindow;


namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für QueryTCValueDialogWindow.xaml
    /// </summary>
    public partial class QueryTCValueDialogWindow : Window
    {
        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;
        private readonly string _valueUnit;
        private readonly string _valueUnit2;
        private bool _initialzed = false;

        public int Value1 => numericUpDownUserControl.Value;
        public int Value2 => numericUpDownUserControl2.Value;

        public QueryDialogResult QueryResult { get; set; }

        public QueryTCValueDialogWindow(string tcTitle, int initialValue, string valueUnit)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            _valueUnit = valueUnit;
            _synthesizer.SpeakAsync(tcTitle,true);
            numericUpDownUserControl.Visibility = Visibility.Visible;
            numericUpDownUserControl.Value = initialValue;
            numericUpDownUserControl.Focus();

        }

        public QueryTCValueDialogWindow(string tcTitle, int initialValue, int initialValue2, string valueUnit, string valueUnit2)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            _valueUnit = valueUnit;
            _valueUnit2 = valueUnit2;
            _synthesizer.SpeakAsync(tcTitle, true);
            numericUpDownUserControl2.Visibility = Visibility.Visible;
            numericUpDownUserControl.Visibility = Visibility.Visible;
            numericUpDownUserControl2.Value = initialValue2;
            numericUpDownUserControl.Value = initialValue;
            numericUpDownUserControl.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            numericUpDownUserControl.Focus();
            _initialzed = true;
        }

        private void numericUpDownUserControl_ValueChanged(object sender, int e)
        {
            if (_initialzed)
            {
                _synthesizer.Speak($"{e} {_valueUnit}");
            }
        }

        private void numericUpDownUserControl2_ValueChanged(object sender, int e)
        {
            if (_initialzed)
            {
                _synthesizer.Speak($"{e} {_valueUnit2}");
            }
        }

        private void YesButton_GotFocus(object sender, RoutedEventArgs e)
        {

            var helpText = AutomationProperties.GetHelpText(sender as UIElement);
            _synthesizer?.Speak(helpText);
        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogResult() { No = false, Yes = false, Previous = true, Cancel = false, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogResult() { No = false, Yes = false, Previous = false, Cancel = true, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
        }

        private void numericUpDownUserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_initialzed)
                _synthesizer.Speak($"{numericUpDownUserControl.Value} {_valueUnit}");
        }

        private void numericUpDownUserControl2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_initialzed)
                _synthesizer.Speak($"{numericUpDownUserControl2.Value} {_valueUnit2}");
        }

        private void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogResult() { No = false, Yes = true, Previous = false, Cancel = false, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
        }

      
    }
}

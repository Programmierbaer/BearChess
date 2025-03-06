using System.Resources;
using System.Windows;
using System.Windows.Automation;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
{
    /// <summary>
    /// Interaktionslogik für QueryDialogWindow.xaml
    /// </summary>
    public partial class QueryDialogWindow : Window
    {
        public struct QueryDialogResult
        {
            public bool Yes { get; set; }
            public bool No { get; set; }
            public bool Previous { get; set; }
            public bool Cancel { get; set; }
            public bool Repeat { get; set; }
        }

        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;

        public QueryDialogResult QueryResult { get; private set; }

        public QueryDialogWindow(string question, string cancelQuestion, bool firstQuestion)
        {
            InitializeComponent();
            PrevButton.Visibility = firstQuestion ? Visibility.Collapsed : Visibility.Visible;
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            TextBlockQuestion.Text = question;
            CancelButton.Content = cancelQuestion;
            AutomationProperties.SetHelpText(CancelButton, cancelQuestion);
            _synthesizer?.Speak(question);
        }

        public QueryDialogWindow(string question)
        {
            InitializeComponent();
            PrevButton.Visibility =  Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            TextBlockQuestion.Text = question;
            _synthesizer?.Speak(question);
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            _synthesizer?.Speak(TextBlockQuestion.Text);
        }

        private void YesButton_GotFocus(object sender, RoutedEventArgs e)
        {

            var helpText = AutomationProperties.GetHelpText(sender as UIElement);
            _synthesizer?.Speak(helpText);
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogResult() { No = false, Yes = true, Previous = false, Cancel = false, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            QueryResult = new QueryDialogResult() { No = true, Yes = false, Previous = false, Cancel = false, Repeat = false };
            _synthesizer?.Clear();
            DialogResult = true;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            YesButton.Focus();
        }
    }
}

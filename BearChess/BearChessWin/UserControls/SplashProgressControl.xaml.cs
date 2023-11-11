using System.Windows;
using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SplashProgressControl.xaml
    /// </summary>
    public partial class SplashProgressControl : UserControl
    {
        private readonly bool _showCancel;

        private SplashProgressControlContent _currentContent;

        public delegate void SplashProgressControlEventHandler(object sender, SplashProgressControlEventArgs args);

        public event SplashProgressControlEventHandler OnCancelClick;

        public bool Cancel { get; private set; }

        public SplashProgressControl()
        {
            InitializeComponent();
            Cancel = false;
            CancelButton.Visibility = Visibility.Visible;
            SubCancelLabel.Visibility = Visibility.Hidden;
            TextLabel.Text = string.Empty;
            SubTextLabel.Text = string.Empty;
            _showCancel = false;
        }

        public SplashProgressControl(bool showCancel) : this()
        {
            _showCancel = showCancel;
            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;
            SubCancelLabel.Visibility = showCancel ? Visibility.Hidden : Visibility.Collapsed;

            // Breite der Progressbar anpassen
            Grid.SetColumnSpan(ProgressBar, _showCancel ? 1 : 2);
            ProgressBar.Width += _showCancel ? 0 : CancelButton.Width;
        }

        public void DoCancel()
        {
            Cancel = true;
            CancelButton.IsEnabled = false;
            ProgressBar.IsIndeterminate = true;
            SubCancelLabel.Visibility = _showCancel ? Visibility.Visible : Visibility.Collapsed;
        }


        public void SetValue(double value)
        {
            _currentContent.CurrentValue = value;
            SetContent(_currentContent);
        }


        public void SetValue(string titel, double value)
        {
            _currentContent.Label = titel;
            _currentContent.CurrentValue = value;
            SetContent(_currentContent);
        }


        public void SetContent(SplashProgressControlContent content)
        {
            _currentContent = content;
            Cancel = Cancel || _currentContent.Cancel;
            TextLabel.Text = _currentContent.Label;
            SubTextLabel.Text = _currentContent.SubLabel;
            ProgressBar.IsIndeterminate = Cancel || !_currentContent.ShowValues;
            ProgressBar.Maximum = _currentContent.MaxValue >= 0 ? _currentContent.MaxValue : 0;
            if (_currentContent.CurrentValue >= 0)
            {
                ProgressBar.Value = _currentContent.CurrentValue <= _currentContent.MaxValue ? _currentContent.CurrentValue : _currentContent.MaxValue;
            }
            else
            {
                _currentContent.CurrentValue = 0;
            }

            SubCancelLabel.Visibility = Cancel && _showCancel ? Visibility.Visible : Visibility.Collapsed;
            if (content.IsFinished)
            {
                ProgressBar.Visibility = Visibility.Hidden;
                SubCancelLabel.Text = "Canceled";
            }
        }


        #region private          

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            CancelButton.IsEnabled = false;
            SubCancelLabel.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            OnCancelClick?.Invoke(this, new SplashProgressControlEventArgs(_currentContent));
        }

        #endregion
    }
}

using System.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EditCommentWindow.xaml
    /// </summary>
    public partial class EditCommentWindow : Window
    {
        public string Comment => textBoxComment.Text;

        public EditCommentWindow(string header, string comment)
        {
            InitializeComponent();
            Title = header;
            textBoxComment.Text = comment;
            textBoxComment.Focus();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            textBoxComment.Text = string.Empty;
        }
    }
}

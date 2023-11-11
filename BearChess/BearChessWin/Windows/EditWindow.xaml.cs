using System.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public string Comment => textBoxComment.Text;

        public EditWindow()
        {
            InitializeComponent();
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        public void SetComment(string comment)
        {
            textBoxComment.Text = comment;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

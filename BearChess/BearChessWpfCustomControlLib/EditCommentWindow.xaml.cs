using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
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

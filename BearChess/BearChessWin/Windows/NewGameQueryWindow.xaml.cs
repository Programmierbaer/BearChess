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

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für NewGameQueryWindow.xaml
    /// </summary>
    public partial class NewGameQueryWindow : Window
    {
        public bool StartNewGame { get; private set; }
        public bool ContinueGame { get; private set; }

        public NewGameQueryWindow()
        {
            InitializeComponent();
        }


        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = true;
            ContinueGame = false;
            DialogResult = true;
        }

        private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = false;
            ContinueGame = true;
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

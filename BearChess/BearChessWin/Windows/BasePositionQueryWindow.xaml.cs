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
    /// Interaktionslogik für BasePositionQueryWindow.xaml
    /// </summary>
    public partial class BasePositionQueryWindow : Window
    {
        public bool StartNewGame { get; private set; }
        public bool ReStartGame { get; private set; }

        public BasePositionQueryWindow()
        {
            InitializeComponent();
        }

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = true;
            ReStartGame = false;
            DialogResult = true;
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = false;
            ReStartGame = false; 
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonReStart_OnClick(object sender, RoutedEventArgs e)
        {
            StartNewGame = false;
            ReStartGame = true;
            DialogResult = true;
        }
    }
}

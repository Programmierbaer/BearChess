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
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessServerWin.Windows
{
    /// <summary>
    /// Interaktionslogik für QueryTournamentWindow.xaml
    /// </summary>
    public partial class QueryTournamentWindow : Window
    {

        public string TournamentName => textBoxName.Text;
        public int BoardsCount => numericUpDownUserControBoards.Value;

        public QueryTournamentWindow()
        {
            InitializeComponent();           
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxName.Focus();
        }
    }
}

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
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für BearChessServerWindow.xaml
    /// </summary>
    public partial class BearChessServerWindow : Window
    {

        private IBearChessServer _server;

        public BearChessServerWindow()
        {
            InitializeComponent();
            _server = new BearChessServer(1111, null);
        }
    }
}

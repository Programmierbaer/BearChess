using System.Windows;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;

namespace CommunicationTestWin
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IElectronicChessBoard _eChessBoard = null;

        public MainWindow()
        {
            InitializeComponent();
            boardUserControl.SetInPositionMode(true,string.Empty);
            boardUserControl.ClearPosition();   
            
        }

        private void ButtonConnectCertabo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                _eChessBoard.FenEvent -= this._eChessBoard_FenEvent;
                _eChessBoard.MoveEvent -= this._eChessBoard_MoveEvent;
                _eChessBoard.SetAllLedsOff();
                _eChessBoard.Close();
                _eChessBoard = null;
                return;
            }
            _eChessBoard = new CertaboLoader();
            _eChessBoard.FenEvent += this._eChessBoard_FenEvent;
            _eChessBoard.MoveEvent += this._eChessBoard_MoveEvent;
            _eChessBoard.SetDemoMode(true);
        }

        private void ButtonConnectMChessLink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_eChessBoard != null)
            {
                _eChessBoard.FenEvent -= this._eChessBoard_FenEvent;
                _eChessBoard.MoveEvent -= this._eChessBoard_MoveEvent;
                _eChessBoard.SetAllLedsOff();
                _eChessBoard.Close();
                _eChessBoard = null;
                return;
            }
            _eChessBoard = new MChessLinkLoader();
            _eChessBoard.FenEvent += this._eChessBoard_FenEvent;
            _eChessBoard.MoveEvent += this._eChessBoard_MoveEvent;
            _eChessBoard.SetDemoMode(true);
        }

        private void _eChessBoard_MoveEvent(object sender, string e)
        {
            Dispatcher?.Invoke(() => { textBoxMove.Text = e; });
        }

        private void _eChessBoard_FenEvent(object sender, string e)
        {
            Dispatcher?.Invoke(() => { textBoxFen.Text = e; });
        }

        private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetLedsFor(textBoxFields.Text.Split(" ".ToCharArray()));
        }

        private void ButtonAllOff_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetAllLedsOff();
        }

        private void ButtonAllOn_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetAllLedsOn();
        }
    }
}

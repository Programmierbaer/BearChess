using System.Windows;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EBoardTestWindow.xaml
    /// </summary>
    public partial class EBoardTestWindow : Window
    {
        private readonly BearChessTools.Configuration _configuration;
        private IElectronicChessBoard _eChessBoard = null;

        public EBoardTestWindow(BearChessTools.Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            boardUserControl.SetInPositionMode(true, string.Empty,true);
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
            _eChessBoard = new CertaboLoader(_configuration.FolderPath);
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
            _eChessBoard.FlashInSync(true);
            _eChessBoard.SetLedCorner(upperLeft: true, upperRight: false, lowerLeft: false, lowerRight: false);
        }

        private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetLedsFor(new SetLedsParameter()
                                     {
                                         FieldNames = textBoxFields.Text.Split(" ".ToCharArray()),
                                         Thinking = true
                                     });
        }

        private void ButtonAllOff_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetAllLedsOff();
        }

        private void ButtonAllOn_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoard?.SetAllLedsOn();
        }

        private void _eChessBoard_MoveEvent(object sender, string e)
        {
            Dispatcher?.Invoke(() => { textBoxMove.Text = e; });
        }

        private void _eChessBoard_FenEvent(object sender, string e)
        {
            Dispatcher?.Invoke(() => { textBoxFen.Text = e; });
        }
    }
}

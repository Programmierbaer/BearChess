using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EBoardInfoWindow.xaml
    /// </summary>
    public partial class EBoardInfoWindow : Window
    {
        private readonly IElectronicChessBoard _eChessBoard;

        private readonly Thread _thread;
        private bool _stopRunning;
        private readonly ChessBoard _chessBoard;

        public EBoardInfoWindow()
        {
            InitializeComponent();
        }

        public EBoardInfoWindow(IElectronicChessBoard eChessBoard, string eBoardName) : this()
        {
            buttonReset.Visibility = Visibility.Collapsed;

            chessBoardUcGraphics.SetPiecesMaterial();
            chessBoardUcGraphics.SetInPositionMode(true, string.Empty, false);
            chessBoardUcGraphics.ClearPosition();
            chessBoardUcGraphics.ShowMultiButton(false);
            chessBoardUcGraphics.ShowRotateButton(false);
            _eChessBoard = eChessBoard;
            _thread = new Thread(ReadFromBoard)
                      {
                          IsBackground = true
                      };
            _thread.Start();
            _eChessBoard.RequestDump();
            _chessBoard = null;
            if (_eChessBoard.Information.Contains(Constants.MeOne))
            {
                _chessBoard = new ChessBoard();
                textBlockBoard.Text =
                    $"{eChessBoard.Information}{Environment.NewLine}{Environment.NewLine}Current position reported by the board.{Environment.NewLine}";

            }
            else
            {
                textBlockBoard.Text =
                    $"{eChessBoard.Information}{Environment.NewLine}{Environment.NewLine}Each pawn represents a piece on a field.{Environment.NewLine}{eBoardName} has no real piece recognition " +
                    "and can only give the information per field whether there is a piece or not.";
            }
        }

        private void ReadFromBoard()
        {
            int count = 0;
            while (!_stopRunning)
            {
                var dataFromBoard = _eChessBoard.GetDumpPiecesFen();
                if (!dataFromBoard.IsFieldDump)
                {
                    count++;
                }

                if (_chessBoard != null)
                {
                    _chessBoard.NewGame();
                    _chessBoard.SetPosition(dataFromBoard.FromBoard);
                    Dispatcher?.Invoke(() => { chessBoardUcGraphics.RepaintBoard(_chessBoard); });
                }
                else
                {
                    if (dataFromBoard.IsFieldDump && !string.IsNullOrWhiteSpace(dataFromBoard.FromBoard))
                    {
                        count = 0;
                        Dispatcher?.Invoke(() =>
                        {
                            chessBoardUcGraphics.ShowFiguresOnField(
                                dataFromBoard.FromBoard.Split(",".ToCharArray()), "P");
                        });

                        if (!dataFromBoard.BasePosition)
                        {
                            _eChessBoard.RequestDump();
                        }
                    }
                    else
                    {
                        if (count > 10)
                        {
                            _eChessBoard.RequestDump();
                            count = 0;
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void EBoardInfoWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _stopRunning = true;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _stopRunning = true;
            DialogResult = true;
        }

        private void ButtonReset_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Send a reset command to the board?", "Reset board", MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _eChessBoard.Reset();
            }
        }
    }
}

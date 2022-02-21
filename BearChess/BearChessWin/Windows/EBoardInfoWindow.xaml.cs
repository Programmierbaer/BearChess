using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für EBoardInfoWindow.xaml
    /// </summary>
    public partial class EBoardInfoWindow : Window
    {
        private readonly IElectronicChessBoard _eChessBoard;

        private readonly Thread _thread;
        private bool _stopRunning;

        public EBoardInfoWindow()
        {
            InitializeComponent();
        }

        public EBoardInfoWindow(IElectronicChessBoard eChessBoard) : this()
        {
            textBlockBoard.Text = $"{eChessBoard.Information}{Environment.NewLine}{Environment.NewLine}Each pawn represents a piece on a field.{Environment.NewLine}DGT Pegasus has no real piece recognition "+
                                  "and can only give the information per field whether there is a piece or not.";
            chessBoardUcGraphics.SetPiecesMaterial();
            chessBoardUcGraphics.SetInPositionMode(true,string.Empty,false);
            chessBoardUcGraphics.ClearPosition();
            _eChessBoard = eChessBoard;
            _thread = new Thread(ReadFromBoard)
                      {
                          IsBackground = true
                      };
            _thread.Start();
            _eChessBoard.RequestDump();
        }

        private void ReadFromBoard()
        {
            int count = 0;
            while (!_stopRunning)
            {
                var dataFromBoard = _eChessBoard.GetPiecesFen();
                if (!dataFromBoard.IsFieldDump)
                {
                    count++;
                }
                if (dataFromBoard.IsFieldDump && !string.IsNullOrWhiteSpace(dataFromBoard.FromBoard))
                {
                    count = 0;
                    Dispatcher?.Invoke(() =>
                    {
                        chessBoardUcGraphics.ShowFiguresOnField(dataFromBoard.FromBoard.Split(",".ToCharArray()), "P");
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
    }
}

using System.ComponentModel;
using System.Data.Entity;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für EBoardControlWindow.xaml
    /// </summary>
    public partial class EBoardControlWindow : Window, IEChessControlBoard
    {
        public EBoardControlWindow()
        {
            InitializeComponent();
            Top = Configuration.Instance.GetWinDoubleValue("EBoardControlWindowTop", Configuration.WinScreenInfo.Top, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = Configuration.Instance.GetWinDoubleValue("EBoardControlWindowLeft", Configuration.WinScreenInfo.Left, SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
        }

        public void SetFen(string fenPosition)
        {
            Dispatcher?.Invoke(() => { chessBoardUcGraphics.SetFen(fenPosition);  });
        }

        private void EBoardControlWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Configuration.Instance.SetDoubleValue("EBoardControlWindowTop", Top);
            Configuration.Instance.SetDoubleValue("EBoardControlWindowLeft", Left);
        }
    }
}

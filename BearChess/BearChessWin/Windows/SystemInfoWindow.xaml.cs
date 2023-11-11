using System.Diagnostics;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SystemInfoWindow.xaml
    /// </summary>
    public partial class SystemInfoWindow : Window
    {
        private readonly Configuration _configuration;

        public SystemInfoWindow(Configuration configuration, string chessBoardInfo)
        {
            InitializeComponent();
            _configuration = configuration;
            textBlockBoard.Text = chessBoardInfo;
            if (!string.IsNullOrWhiteSpace(chessBoardInfo))
            {

                if (chessBoardInfo.Equals("Millennium " + Constants.MeOne))
                {
                    imageeOne.Visibility = Visibility.Visible;
                }

                if (chessBoardInfo.Equals("Millennium " + Constants.Exclusive))
                {
                    imageExclusive.Visibility = Visibility.Visible;
                }
                
                if (chessBoardInfo.Equals("Millennium " + Constants.Supreme))
                {
                    imageSupreme.Visibility = Visibility.Visible;
                }

                if (chessBoardInfo.Equals("Millennium " + Constants.KingPerformance))
                {
                    imageKingPerformance.Visibility = Visibility.Visible;
                }

                if (chessBoardInfo.Equals(Constants.Certabo))
                {
                    imageCertabo.Visibility = Visibility.Visible;
                }
             
                if (chessBoardInfo.StartsWith(Constants.Pegasus))
                {
                    imagePegasus.Visibility = Visibility.Visible;
                }

                if (chessBoardInfo.Equals(Constants.DGT))
                {
                    imageDGT.Visibility = Visibility.Visible;
                }

                if (chessBoardInfo.Equals(Constants.ChessnutAir))
                {
                    imageChessnutAir.Visibility = Visibility.Visible;
                } 
                
                if (chessBoardInfo.StartsWith(Constants.IChessOne))
                {
                    imageIChessOne.Visibility = Visibility.Visible;
                }

                if (chessBoardInfo.Equals(Constants.SquareOffPro))
                {
                    imageSquareOffPro.Visibility = Visibility.Visible;
                }
                if (chessBoardInfo.Equals(Constants.Citrine))
                {
                    imageCitrine.Visibility = Visibility.Visible;
                }
                if (chessBoardInfo.Equals(Constants.UCB))
                {
                    imageUCB.Visibility = Visibility.Visible;
                }
                if (chessBoardInfo.Equals(Constants.TabutronicSentio))
                {
                    imageTabutronicSentio.Visibility = Visibility.Visible;
                }
                if (chessBoardInfo.Equals(Constants.TabutronicCerno))
                {
                    imageTabutronicCerno.Visibility = Visibility.Visible;
                }
                if (chessBoardInfo.Contains(Constants.Chesstimation))
                {
                    imageMephistoExclusive.Visibility = Visibility.Visible;
                }
                if (chessBoardInfo.Contains(Constants.Elfacun))
                {
                    imageMephistoExclusive.Visibility = Visibility.Visible;
                }
            }
        }

        private void SystemInfoWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            textBlockPath.Text = _configuration.FolderPath;
        }

        private void ButtonPath_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(_configuration.FolderPath);
            DialogResult = true;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

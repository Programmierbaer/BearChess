using System.Diagnostics;
using System.Resources;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase;
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
        private readonly ResourceManager _rm;

        public SystemInfoWindow(Configuration configuration, string chessBoardInfo)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _configuration = configuration;
            textBlockBit.Text = _configuration.RunOn64Bit ? "64 bit" : "32 bit";
            textBlockStandAlone.Visibility = _configuration.Standalone ? Visibility.Visible : Visibility.Collapsed;
            textBlockBoard.Text = _rm.GetString("NotConnected");
            if (!string.IsNullOrWhiteSpace(chessBoardInfo))
            {
                textBlockBoard.Text = chessBoardInfo;
                if (chessBoardInfo.StartsWith("Millennium"))
                {
                    if (chessBoardInfo.Equals("Millennium " + Constants.MeOne))
                    {
                        imageeOne.Visibility = Visibility.Visible;
                        return;
                    }

                    if (chessBoardInfo.Equals("Millennium " + Constants.Exclusive))
                    {
                        imageExclusive.Visibility = Visibility.Visible;
                        return;
                    }

                    if (chessBoardInfo.Equals("Millennium " + Constants.Supreme))
                    {
                        imageSupreme.Visibility = Visibility.Visible;
                        return;
                    }

                    if (chessBoardInfo.Equals("Millennium " + Constants.KingPerformance))
                    {
                        imageKingPerformance.Visibility = Visibility.Visible;
                        return;
                    }
                    imageExclusive.Visibility = Visibility.Visible;
                    return;
                }
                if (chessBoardInfo.Equals(Constants.Certabo))
                {
                    imageCertabo.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.StartsWith(Constants.Pegasus))
                {
                    imagePegasus.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.DGT))
                {
                    imageDGT.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.ChessnutAir))
                {
                    imageChessnutAir.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.ChessnutGo))
                {
                    imageChessnutGo.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.ChessnutAirPlus))
                {
                    imageChessnutAirPlus.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.ChessnutAirPlus2))
                {
                    imageChessnutAirPlus.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.ChessnutEvo))
                {
                    imageChessnutEvo.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.ChessnutPro))
                {
                    imageChessnutAirPro.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.StartsWith(Constants.IChessOne))
                {
                    imageIChessOne.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.SquareOffPro))
                {
                    imageSquareOffPro.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.Citrine))
                {
                    imageCitrine.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.UCB))
                {
                    imageUCB.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.TabutronicSentio))
                {
                    imageTabutronicSentio.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.TabutronicCerno))
                {
                    imageTabutronicCerno.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Equals(Constants.TabutronicTactum))
                {
                    imageTabutronicTactum.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Contains(Constants.Chesstimation))
                {
                    imageMephistoExclusive.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Contains(Constants.Elfacun))
                {
                    imageMephistoExclusive.Visibility = Visibility.Visible;
                    return;
                }

                if (chessBoardInfo.Contains(Constants.ChessUp))
                {
                    if (chessBoardInfo.Contains(Constants.ChessUp2))
                        imageChessUp2.Visibility = Visibility.Visible;
                    else
                        imageChessUp.Visibility = Visibility.Visible;
                    return;
                }                
                if (chessBoardInfo.Contains(Constants.Zmartfun))
                {
                    imageHoS.Visibility = Visibility.Visible;
                    return;
                }
            }           
        }

        private void SystemInfoWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            textBlockPath.Text = _configuration.FolderPath;
            textBlockBinPath.Text = _configuration.BinPath;
        }

        private void ButtonPath_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(_configuration.FolderPath);
           // DialogResult = true;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonPathBin_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(_configuration.BinPath);
           // DialogResult = true;
        }
    }
}
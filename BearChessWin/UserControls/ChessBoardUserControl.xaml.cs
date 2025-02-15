using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für ChessBoardUserControl.xaml
    /// </summary>
    public partial class ChessBoardUserControl : UserControl
    {
       
        public ChessBoardUserControl()
        {
            InitializeComponent();
        }


        public void FillGrid(string whiteFileName, string blackFileName)
        {
            imageA8.Source = new BitmapImage(new Uri(whiteFileName));
            imageA7.Source = new BitmapImage(new Uri(blackFileName));
            imageA6.Source = new BitmapImage(new Uri(whiteFileName));
            imageA5.Source = new BitmapImage(new Uri(blackFileName));
            imageA4.Source = new BitmapImage(new Uri(whiteFileName));
            imageA3.Source = new BitmapImage(new Uri(blackFileName));
            imageA2.Source = new BitmapImage(new Uri(whiteFileName));
            imageA1.Source = new BitmapImage(new Uri(blackFileName));
            imageB8.Source = new BitmapImage(new Uri(blackFileName));
            imageB7.Source = new BitmapImage(new Uri(whiteFileName));
            imageB6.Source = new BitmapImage(new Uri(blackFileName));
            imageB5.Source = new BitmapImage(new Uri(whiteFileName));
            imageB4.Source = new BitmapImage(new Uri(blackFileName));
            imageB3.Source = new BitmapImage(new Uri(whiteFileName));
            imageB2.Source = new BitmapImage(new Uri(blackFileName));
            imageB1.Source = new BitmapImage(new Uri(whiteFileName));
            imageC8.Source = new BitmapImage(new Uri(whiteFileName));
            imageC7.Source = new BitmapImage(new Uri(blackFileName));
            imageC6.Source = new BitmapImage(new Uri(whiteFileName));
            imageC5.Source = new BitmapImage(new Uri(blackFileName));
            imageC4.Source = new BitmapImage(new Uri(whiteFileName));
            imageC3.Source = new BitmapImage(new Uri(blackFileName));
            imageC2.Source = new BitmapImage(new Uri(whiteFileName));
            imageC1.Source = new BitmapImage(new Uri(blackFileName));
            imageD8.Source = new BitmapImage(new Uri(blackFileName));
            imageD7.Source = new BitmapImage(new Uri(whiteFileName));
            imageD6.Source = new BitmapImage(new Uri(blackFileName));
            imageD5.Source = new BitmapImage(new Uri(whiteFileName));
            imageD4.Source = new BitmapImage(new Uri(blackFileName));
            imageD3.Source = new BitmapImage(new Uri(whiteFileName));
            imageD2.Source = new BitmapImage(new Uri(blackFileName));
            imageD1.Source = new BitmapImage(new Uri(whiteFileName));
            imageE8.Source = new BitmapImage(new Uri(whiteFileName));
            imageE7.Source = new BitmapImage(new Uri(blackFileName));
            imageE6.Source = new BitmapImage(new Uri(whiteFileName));
            imageE5.Source = new BitmapImage(new Uri(blackFileName));
            imageE4.Source = new BitmapImage(new Uri(whiteFileName));
            imageE3.Source = new BitmapImage(new Uri(blackFileName));
            imageE2.Source = new BitmapImage(new Uri(whiteFileName));
            imageE1.Source = new BitmapImage(new Uri(blackFileName));
            imageF8.Source = new BitmapImage(new Uri(blackFileName));
            imageF7.Source = new BitmapImage(new Uri(whiteFileName));
            imageF6.Source = new BitmapImage(new Uri(blackFileName));
            imageF5.Source = new BitmapImage(new Uri(whiteFileName));
            imageF4.Source = new BitmapImage(new Uri(blackFileName));
            imageF3.Source = new BitmapImage(new Uri(whiteFileName));
            imageF2.Source = new BitmapImage(new Uri(blackFileName));
            imageF1.Source = new BitmapImage(new Uri(whiteFileName));
            imageG8.Source = new BitmapImage(new Uri(whiteFileName));
            imageG7.Source = new BitmapImage(new Uri(blackFileName));
            imageG6.Source = new BitmapImage(new Uri(whiteFileName));
            imageG5.Source = new BitmapImage(new Uri(blackFileName));
            imageG4.Source = new BitmapImage(new Uri(whiteFileName));
            imageG3.Source = new BitmapImage(new Uri(blackFileName));
            imageG2.Source = new BitmapImage(new Uri(whiteFileName));
            imageG1.Source = new BitmapImage(new Uri(blackFileName));
            imageH8.Source = new BitmapImage(new Uri(blackFileName));
            imageH7.Source = new BitmapImage(new Uri(whiteFileName));
            imageH6.Source = new BitmapImage(new Uri(blackFileName));
            imageH5.Source = new BitmapImage(new Uri(whiteFileName));
            imageH4.Source = new BitmapImage(new Uri(blackFileName));
            imageH3.Source = new BitmapImage(new Uri(whiteFileName));
            imageH2.Source = new BitmapImage(new Uri(blackFileName));
            imageH1.Source = new BitmapImage(new Uri(whiteFileName));
        }
    }
}
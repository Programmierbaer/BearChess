using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für ConfirmPiecesWindow.xaml
    /// </summary>
    public partial class ConfirmPiecesWindow : Window
    {
        private readonly HashSet<string> _allNames;
        private readonly string _path;
        private readonly string _fileName;

        public BoardPiecesSetup BoardPiecesSetup { get; private set; }

        public ConfirmPiecesWindow(string path, string[] allNames, string fileName)
        {
            InitializeComponent();
            _path = path;
            _fileName = fileName;
            textBoxName.Text = string.Empty;
            textBoxName.ToolTip = "Give the piece set a name";
            _allNames = new HashSet<string>(allNames);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("A piece set name is required", "Missing Information", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (_allNames.Contains(textBoxName.Text))
            {
                MessageBox.Show("The piece set name is already taken", "Duplicate Information", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            BoardPiecesSetup.Name = textBoxName.Text;
            DialogResult = true;
        }


        private void LoadBigBmpImage(string fileName)
        {
            var bitmapImage = new BitmapImage(new Uri(fileName));

            var bitmapImageWidth = bitmapImage.PixelWidth / 12;
            var bitmapImageHeight = bitmapImage.PixelHeight;
            var image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect(0, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            
            imagePawnW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)bitmapImageWidth, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKnightW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 2, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageBishopW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 3, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageRookW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 4, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageQueenW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 5, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKingW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 6, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imagePawnB.Source = image;


            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 7, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKnightB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 8, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageBishopB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 9, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageRookB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 10, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageQueenB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 11, 0, (int)bitmapImageWidth, (int)bitmapImageHeight - 1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKingB.Source = image;


            

        }

        private void LoadBigImage(string fileName)
        {
           

            var bitmapImage = new BitmapImage(new Uri(fileName));

            var bitmapImageWidth = bitmapImage.PixelWidth / 6;
            var bitmapImageHeight = bitmapImage.PixelHeight / 2;

            var image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect(0, 0, (int)bitmapImageWidth , (int)bitmapImageHeight-1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageRookW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)bitmapImageWidth , 0, (int)bitmapImageWidth , (int)bitmapImageHeight-1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKnightW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth)*2, 0, (int)bitmapImageWidth, (int)bitmapImageHeight-1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageBishopW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth)*3, 0, (int)bitmapImageWidth, (int)bitmapImageHeight-1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageQueenW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth)*4, 0, (int)bitmapImageWidth, (int)bitmapImageHeight-1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKingW.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)(bitmapImageWidth) * 5, 0, (int)bitmapImageWidth , (int)bitmapImageHeight-1);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imagePawnW.Source = image;


            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect(0, (int)bitmapImageHeight, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageRookB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)bitmapImageWidth, (int)bitmapImageHeight, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKnightB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 2, (int)bitmapImageHeight, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageBishopB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 3, (int)bitmapImageHeight, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageQueenB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 4, (int)bitmapImageHeight, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imageKingB.Source = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.SourceRect = new Int32Rect((int)bitmapImageWidth * 5, (int)bitmapImageHeight, (int)bitmapImageWidth, (int)bitmapImageHeight);
            image.UriSource = new Uri(fileName);
            image.EndInit();
            imagePawnB.Source = image;


        }

        private void ConfirmPiecesWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            BoardPiecesSetup = new BoardPiecesSetup();
            try
            {
                BoardPiecesSetup.Id = "pieces_"+Guid.NewGuid().ToString("N");
                if (!string.IsNullOrWhiteSpace(_fileName))
                {
                    BoardPiecesSetup.WhiteKingFileName = _fileName;
                    LoadBigImage(BoardPiecesSetup.WhiteKingFileName);
                    return;
                }
                var allFiles = Directory.GetFiles(_path, "*.png", SearchOption.TopDirectoryOnly);
                foreach (var allFile in allFiles)
                {
                    var fileInfo = new FileInfo(allFile);
                    if (fileInfo.Name.StartsWith("KingW.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("WhiteKing.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("wk.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.WhiteKingFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("QueenW.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("WhiteQueen.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("wq.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.WhiteQueenFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("RookW.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("WhiteRook.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("wr.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.WhiteRookFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("BishopW.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("WhiteBishop.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("wb.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.WhiteBishopFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("KnightW.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("WhiteKnight.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("wn.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.WhiteKnightFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("PawnW.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("WhitePawn.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("wp.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.WhitePawnFileName = fileInfo.FullName;

                    if (fileInfo.Name.StartsWith("KingB.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("BlackKing.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("bk.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.BlackKingFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("QueenB.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("BlackQueen.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("bq.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.BlackQueenFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("RookB.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("BlackRook.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("br.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.BlackRookFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("BishopB.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("BlackBishop.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("bb.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.BlackBishopFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("KnightB.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("BlackKnight.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("bn.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.BlackKnightFileName = fileInfo.FullName;
                    if (fileInfo.Name.StartsWith("PawnB.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("BlackPawn.png", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.Equals("bp.png", StringComparison.OrdinalIgnoreCase))
                        BoardPiecesSetup.BlackPawnFileName = fileInfo.FullName;
                }

                if (string.IsNullOrWhiteSpace(BoardPiecesSetup.WhiteKingFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.WhiteQueenFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.WhiteRookFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.WhiteBishopFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.WhiteKnightFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.WhitePawnFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.BlackKingFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.BlackQueenFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.BlackRookFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.BlackBishopFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.BlackKnightFileName)
                    || string.IsNullOrWhiteSpace(BoardPiecesSetup.BlackPawnFileName)
                )
                {
                    MessageBox.Show("No or not all png files found", "Missing Information", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    DialogResult = false;
                    return;
                }
                imageKingW.Source = new BitmapImage(new Uri(BoardPiecesSetup.WhiteKingFileName));
                imageQueenW.Source = new BitmapImage(new Uri(BoardPiecesSetup.WhiteQueenFileName));
                imageRookW.Source = new BitmapImage(new Uri(BoardPiecesSetup.WhiteRookFileName));
                imageBishopW.Source = new BitmapImage(new Uri(BoardPiecesSetup.WhiteBishopFileName));
                imageKnightW.Source = new BitmapImage(new Uri(BoardPiecesSetup.WhiteKnightFileName));
                imagePawnW.Source = new BitmapImage(new Uri(BoardPiecesSetup.WhitePawnFileName));
                imageKingB.Source = new BitmapImage(new Uri(BoardPiecesSetup.BlackKingFileName));
                imageQueenB.Source = new BitmapImage(new Uri(BoardPiecesSetup.BlackQueenFileName));
                imageRookB.Source = new BitmapImage(new Uri(BoardPiecesSetup.BlackRookFileName));
                imageBishopB.Source = new BitmapImage(new Uri(BoardPiecesSetup.BlackBishopFileName));
                imageKnightB.Source = new BitmapImage(new Uri(BoardPiecesSetup.BlackKnightFileName));
                imagePawnB.Source = new BitmapImage(new Uri(BoardPiecesSetup.BlackPawnFileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading files{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                DialogResult = false;
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
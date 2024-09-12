using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SelectInstalledBookWindow.xaml
    /// </summary>
    public partial class SelectInstalledBookWindow : Window
    {
        private readonly string _bookPath;
        private readonly ObservableCollection<BookInfo> _openingBooks;
        private readonly HashSet<string> _installedBooks;
        private readonly ResourceManager _rm;
        public BookInfo SelectedBook => (BookInfo)dataGridBook.SelectedItem;


        public SelectInstalledBookWindow(BookInfo[] openingBooks, string bookPath)
        {
            _bookPath = bookPath;
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _openingBooks = new ObservableCollection<BookInfo>(openingBooks.OrderBy(o => o.Name));
            dataGridBook.ItemsSource = _openingBooks;
            _installedBooks = new HashSet<string>(openingBooks.Select(b => b.Name));
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedBook == null)
            {
                return;
            }

            var bookName = SelectedBook.Name;
            var bookId = SelectedBook.Id;
            if (MessageBox.Show($"{_rm.GetString("UninstallBook")} '{bookName}'?", _rm.GetString("UninstallBook"),
                                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) !=
                MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                File.Delete(Path.Combine(_bookPath, bookId + ".book"));
                _installedBooks.Remove(bookName);
                _openingBooks.Remove(SelectedBook);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_rm.GetString("ErrorUninstallBook")} '{bookName}'{Environment.NewLine}{ex.Message}", _rm.GetString("UninstallBook"),
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGridBook_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedBook != null)
            {
                DialogResult = true;
            }
        }

        private void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = $"{_rm.GetString("AllBooks")}|*.abk;*.bin;*.ctg|Polyglot|*.bin|ChessBase|*.ctg|Arena|*.abk" };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                var openingBook = new OpeningBook();
                if (openingBook.LoadBook(openFileDialog.FileName, true))
                {
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    if (_installedBooks.Contains(fileInfo.Name))
                    {
                        MessageBox.Show(
                            this,
                            $"{_rm.GetString("OpeningBook")} '{fileInfo.Name}' {_rm.GetString("AlreadyInstalled")}!", _rm.GetString("OpeningBook"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;

                    }
                    var bookInfo = new BookInfo()
                                        {
                                            Id = "bk"+Guid.NewGuid().ToString("N"),
                                            Name = fileInfo.Name,
                                            FileName = openFileDialog.FileName,
                                            Size = fileInfo.Length,
                                            PositionsCount = openingBook.PositionsCount,
                                            MovesCount = openingBook.MovesCount,
                                            GamesCount = openingBook.GamesCount
                                        };
                    var serializer = new XmlSerializer(typeof(BookInfo));
                    TextWriter textWriter = new StreamWriter(Path.Combine(_bookPath,bookInfo.Id + ".book"), false);
                    serializer.Serialize(textWriter, bookInfo);
                    textWriter.Close();
                    _installedBooks.Add(bookInfo.Name);
                    _openingBooks.Add(bookInfo);
                }
            }
        }
    }
}

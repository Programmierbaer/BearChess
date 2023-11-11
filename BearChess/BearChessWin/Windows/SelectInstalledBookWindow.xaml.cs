using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

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
        public BookInfo SelectedBook => (BookInfo)dataGridBook.SelectedItem;


        public SelectInstalledBookWindow(BookInfo[] openingBooks, string bookPath)
        {
            _bookPath = bookPath;
            InitializeComponent();
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
            if (MessageBox.Show($"Uninstall opening book '{bookName}'?", "Uninstall Book",
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
                MessageBox.Show($"Error on uninstall opening book '{bookName}'{Environment.NewLine}{ex.Message}", "Uninstall Book",
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
            var openFileDialog = new OpenFileDialog { Filter = "All books|*.abk;*.bin|Polyglot book|*.bin|Arena book|*.abk" };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                OpeningBook openingBook = new OpeningBook();
                if (openingBook.LoadBook(openFileDialog.FileName, true))
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    if (_installedBooks.Contains(fileInfo.Name))
                    {
                        MessageBox.Show(
                            this,
                            $"Opening book '{fileInfo.Name}' already installed!", "Opening book", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;

                    }
                    BookInfo bookInfo = new BookInfo()
                                        {
                                            Id = "bk"+Guid.NewGuid().ToString("N"),
                                            Name = fileInfo.Name,
                                            FileName = openFileDialog.FileName,
                                            Size = fileInfo.Length,
                                            PositionsCount = openingBook.PositionsCount,
                                            MovesCount = openingBook.MovesCount
                                        };
                    XmlSerializer serializer = new XmlSerializer(typeof(BookInfo));
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

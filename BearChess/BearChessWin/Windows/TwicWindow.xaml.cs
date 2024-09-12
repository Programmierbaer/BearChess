using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;
using System.Resources;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für TwicWindow.xaml
    /// </summary>
    public partial class TwicWindow : Window
    {
        private readonly Database _database;
        private readonly ILogging _logger;
        
        private readonly string _twicUrl;
        private readonly bool _deleteAfterDownload;
        private int _initialTwicNumber;
        private ResourceManager _rm;

        public TwicWindow(Database database, ILogging logger, Configuration configuration)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _database = database;
            _logger = logger;
            _twicUrl = configuration.GetConfigValue("twicUrl", "https://theweekinchess.com/zips/");
            bool.TryParse(configuration.GetConfigValue("deleteAfterDownload", "true"), out _deleteAfterDownload);
            int.TryParse(configuration.GetConfigValue("initialTwicNumber", "1499"), out _initialTwicNumber);
            SetItemsSource();
        }

        private void SetItemsSource()
        {
            dataGridDownloads.ItemsSource = _database.GeTwicDownloads();
        }

        private void DataGridDownloads_OnSelected(object sender, SelectionChangedEventArgs e)
        {
            //
        }

        private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ButtonDownloadAll_OnClick(object sender, RoutedEventArgs e)
        {
            var maxNumber = _database.MaxTwicNumber();
            if (maxNumber == 0)
            {
                maxNumber = _initialTwicNumber;
            }
            if (MessageBox.Show($"{_rm.GetString("DownloadAllHigherThan")} {maxNumber}?", _rm.GetString("DownloadFromTWIC"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) != MessageBoxResult.Yes)
            {
                return;
            }
            buttonDownloadAll.IsEnabled = false;
            buttonExit.IsEnabled = false;
            buttonDownloadSingle.IsEnabled = false;
            var newDownloads = new List<string>();
            if (maxNumber > 0)
            {
                maxNumber++;
                while (await DownloadTWICFile(maxNumber))
                {
                    newDownloads.Add(maxNumber.ToString());
                    maxNumber++;
                }

                if (newDownloads.Count > 0)
                {
                    MessageBox.Show($"New downloads: {string.Join(", ", newDownloads.ToArray())}", "TWIC", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetItemsSource();
                }
                else
                {
                    maxNumber = _database.MaxTwicNumber();
                    var lastDownload = _database.TWICFileImported(maxNumber);
                    MessageBox.Show($"No new downloads available. Last download {maxNumber} at {lastDownload.Value}", "TWIC", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            buttonDownloadAll.IsEnabled = true;
            buttonExit.IsEnabled = true;
            buttonDownloadSingle.IsEnabled = true;
        }

        private async Task<bool> DownloadTWICFile(int twicNumber)
        {
            var url = $"{_twicUrl}/twic{twicNumber}g.zip";
            var imported = _database.TWICFileImported(url);
            if (imported != null)
            {
                return false;
            }
            var fi = new FileInfo(_database.FileName);
            var pathName = fi.DirectoryName;
            if (string.IsNullOrWhiteSpace(pathName))
            {
                return false;
            }
            var array = url.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (array.Length > 0)
            {
                var zipFileName = Path.Combine(pathName, array[array.Length - 1]);
                try
                {
                    ProgressWindow infoWindow = null;
                    Dispatcher.Invoke(() =>
                    {
                        infoWindow = new ProgressWindow
                        {
                            Owner = this
                        };

                        infoWindow.IsIndeterminate(true);
                        infoWindow.SetTitle($"{_rm.GetString("Download")} {twicNumber}");
                        infoWindow.SetWait(_rm.GetString("PleaseWait"));
                        infoWindow.Show();
                    });
                    FileDownload.Download(url, zipFileName, true);
                    Dispatcher.Invoke(() =>
                    {
                        infoWindow.Close();
                    });
                    if (!File.Exists(zipFileName))
                    {
                        return false;
                    }
                    var pgnFileName = FileDownload.UnzipFile(zipFileName, pathName, true);
                    twicNumber = 0;
                    if (int.TryParse(pgnFileName.Replace("twic", string.Empty).Replace(".pgn", string.Empty), out var result))
                    {
                        twicNumber = result;
                    }
                    var pgnFi = new FileInfo(Path.Combine(pathName, pgnFileName));
                    var twicId = _database.SaveTWICImport(twicNumber, url, pgnFileName, 0, pgnFi.LastWriteTime.ToFileTime());
                    bool isCanceled = await ImportFile(Path.Combine(pathName, pgnFileName), twicId);
                    if (_deleteAfterDownload)
                    {
                        File.Delete(zipFileName);
                        File.Delete(Path.Combine(pathName, pgnFileName));
                    }
                    if (isCanceled)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex);
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ImportFile(string fileName, int twicId)
        {
            _logger?.LogDebug($"Import file {fileName} started.");
            bool isCanceled = false;
            var count = 0;
            var fi = new FileInfo(fileName);

            ProgressWindow infoWindow = null;
            Dispatcher.Invoke(() =>
            {
                infoWindow = new ProgressWindow
                {
                    Owner = this
                };

                infoWindow.IsIndeterminate(true);
                infoWindow.SetTitle($"{_rm.GetString("Import")} {fi.Name}");
                infoWindow.SetWait(_rm.GetString("PleaseWait"));
                infoWindow.ShowCancelButton(true);
                infoWindow.Show();
            });
         
            var startFromBasePosition = true;
            CurrentGame currentGame;
            var pgnLoader = new PgnLoader();
            var chessBoard = new ChessBoard();
            chessBoard.Init();
            chessBoard.NewGame();

            await Task.Factory.StartNew(() =>
            {
                var startTime = DateTime.Now;
                _logger?.LogDebug("Start import...");

                foreach (var pgnGame in pgnLoader.Load(fileName))
                {
                    if (infoWindow.IsCanceled)
                    {
                        isCanceled = true;
                        break;
                    }
                    var fenValue = pgnGame.GetValue("FEN");
                    if (!string.IsNullOrWhiteSpace(fenValue))
                    {
                        chessBoard.SetPosition(fenValue, false);
                        startFromBasePosition = false;
                    }
                    for (var i = 0; i < pgnGame.MoveCount; i++)
                    {
                        chessBoard.MakePgnMove(pgnGame.GetMove(i), pgnGame.GetComment(i), pgnGame.GetEMT(i));
                    }
                    //    continue;
                    count++;
                    if (count % 100 == 0)
                    {
                        Dispatcher.Invoke(() => { infoWindow.SetInfo($"{count} {_rm.GetString("Games")}..."); });
                    }
                    var uciInfoWhite = new UciInfo()
                    {
                        IsPlayer = true,
                        Name = pgnGame.PlayerWhite
                    };
                    var uciInfoBlack = new UciInfo()
                    {
                        IsPlayer = true,
                        Name = pgnGame.PlayerBlack
                    };
                    currentGame = new CurrentGame(uciInfoWhite, uciInfoBlack, string.Empty,
                        new TimeControl(), pgnGame.PlayerWhite, pgnGame.PlayerBlack,
                        startFromBasePosition, true);
                    if (!startFromBasePosition)
                    {
                        currentGame.StartPosition = fenValue;
                    }


                    if (_database.Save(new DatabaseGame(pgnGame, chessBoard.GetPlayedMoveList(), currentGame) { TwicId = twicId }, false, count % 100 == 0, twicId) > 0)
                    {
                        chessBoard.Init();
                        chessBoard.NewGame();
                    }
                    else
                    {
                        break;
                    }
                }
                var diff = DateTime.Now - startTime;
                _database.CommitAndClose();
                //_database.Compress();
                _logger?.LogDebug($"... {count} games imported in {diff.TotalSeconds} sec.");

            }).ContinueWith(task =>
            {
                _database.SetNumberOfTWICGames(twicId, _database.NumberOfTWICInGames(twicId));
                Dispatcher.Invoke(() => { infoWindow.Close(); });
                //infoWindow.Close();
                SetItemsSource();
               
            }, System.Threading.CancellationToken.None, TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
            _logger?.LogDebug($"Import file {fileName} finished.");
            return isCanceled;
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridDownloads.SelectedItems.Count == 0)
            {
                return;
            }
           
            if (dataGridDownloads.SelectedItems.Count > 1)
            {
                if (MessageBox.Show($"Delete all {dataGridDownloads.SelectedItems.Count} selected games?", "Delete games", MessageBoxButton.YesNo,
                        MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show("Delete selected games?", "Delete games", MessageBoxButton.YesNo,
                        MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            foreach (var selectedItem in dataGridDownloads.SelectedItems)
            {
                if (selectedItem is TwicDownload twicDownload)
                {
                    _database.DeleteTWICGames(twicDownload.Id);
                }
            }

            SetItemsSource();
            
        }

        private async void ButtonDownloadSingle_OnClick(object sender, RoutedEventArgs e)
        {
            if (numericUpDownUserControlTwicNumberTo.Value < numericUpDownUserControlTwicNumberFrom.Value)
            {
                MessageBox.Show("The 'from' number must be less than or equal to the 'to' number", _rm.GetString("InvalidParameter"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            buttonDownloadSingle.IsEnabled = false;
            buttonDownloadAll.IsEnabled = false;
            buttonExit.IsEnabled = false;

            for (int i = numericUpDownUserControlTwicNumberFrom.Value; i <= numericUpDownUserControlTwicNumberTo.Value; i++)
            {
                if (!await DownloadTWICFile(i))
                {
                    break;
                }
            }

            SetItemsSource();
            buttonDownloadAll.IsEnabled = true;
            buttonExit.IsEnabled = true;
            buttonDownloadSingle.IsEnabled = true;
        }

        private void NumericUpDownUserControlTwicNumberFrom_OnValueChanged(object sender, int e)
        {
            if (numericUpDownUserControlTwicNumberTo!=null &&  e > numericUpDownUserControlTwicNumberTo.Value)
            {
                numericUpDownUserControlTwicNumberTo.Value = e;
            }

            SetToolTip();
        }

        private void SetToolTip()
        {
            if (buttonDownloadSingle!=null && numericUpDownUserControlTwicNumberFrom != null && numericUpDownUserControlTwicNumberTo != null)
            {
                if (numericUpDownUserControlTwicNumberFrom.Value.Equals(numericUpDownUserControlTwicNumberTo.Value))
                {
                    buttonDownloadSingle.ToolTip =
                        $"{_rm.GetString("Download")} {numericUpDownUserControlTwicNumberFrom.Value}";
                }
                else
                {
                    buttonDownloadSingle.ToolTip =
                        $"{_rm.GetString("Download")} {_rm.GetString("From")} {numericUpDownUserControlTwicNumberFrom.Value} {_rm.GetString("To")} {numericUpDownUserControlTwicNumberTo.Value}";
                }
            }
        }

        private void NumericUpDownUserControlTwicNumberTo_OnValueChanged(object sender, int e)
        {
            if (numericUpDownUserControlTwicNumberFrom!=null && e < numericUpDownUserControlTwicNumberFrom.Value)
            {
                numericUpDownUserControlTwicNumberFrom.Value = e;
            }
            SetToolTip();
        }
    }
}

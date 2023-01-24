using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für ChessBoardSetupWindow.xaml
    /// </summary>
    public partial class ChessBoardSetupWindow : Window
    {
        public class DeleteEventArgs : EventArgs
        {
            public string Id { get; }
            public DeleteEventArgs(string id)
            {
                Id = id;
            }

        }

        private readonly string _boardPath;
        private readonly string _piecesPath;
        private readonly Dictionary<string, BoardFieldsSetup> _installedFields;
        private readonly Dictionary<string, BoardPiecesSetup> _installedPieces;
        private readonly List<string> _unDeleteablePieces = new List<string>();
        private readonly List<string> _unDeleteableFields = new List<string>();

        public BoardFieldsSetup BoardFieldsSetup { get; private set; }
        public BoardPiecesSetup BoardPiecesSetup { get; private set; }
      
        public event EventHandler BoardSetupChangedEvent;
        public event EventHandler PiecesSetupChangedEvent;
      
        public bool ShowLastMove =>
            // ReSharper disable once PossibleInvalidOperationException
            checkBoxShowLastMove.IsChecked.Value;

        public bool ShowBestMove =>
            // ReSharper disable once PossibleInvalidOperationException
            checkBoxShowBestMove.IsChecked.Value;

        public bool ShowAsArrow =>
            // ReSharper disable once PossibleInvalidOperationException
            checkBoxAsArrow.IsChecked.Value;


        public ChessBoardSetupWindow(string boardPath, string piecesPath, Dictionary<string, BoardFieldsSetup> installedFields,
                                     Dictionary<string, BoardPiecesSetup> installedPieces,
                                     string currentBoardFieldsSetupId, string currentBoardPiecesSetupId, bool showLastMove, bool showBestMove, bool asArrow)
        {

            InitializeComponent();

            checkBoxShowBestMove.IsChecked = showBestMove;
            checkBoxShowLastMove.IsChecked = showLastMove;
            checkBoxAsArrow.IsChecked = asArrow;
            installedFields[Constants.BearChess] = new BoardFieldsSetup()
            {
                Name = Constants.BearChess,
                WhiteFileName = string.Empty,
                BlackFileName = string.Empty,
                Id = Constants.BearChess
            };

            installedFields[Constants.Certabo] = new BoardFieldsSetup()
            {
                Name = Constants.Certabo,
                WhiteFileName = string.Empty,
                BlackFileName = string.Empty,
                Id = Constants.Certabo
            };

            installedPieces[Constants.BearChess] = new BoardPiecesSetup() {Name = Constants.BearChess, Id = Constants.BearChess };
            installedPieces[Constants.Certabo] = new BoardPiecesSetup() {Name = Constants.Certabo, Id = Constants.Certabo };
            installedPieces[Constants.BryanWhitbyDali] = new BoardPiecesSetup() { Name = "Dali by Bryan Whitby", Id = Constants.BryanWhitbyDali };
            installedPieces[Constants.BryanWhitbyItalian] = new BoardPiecesSetup() { Name = "Italian by Bryan Whitby", Id = Constants.BryanWhitbyItalian };
            installedPieces[Constants.BryanWhitbyRoyalGold] = new BoardPiecesSetup() { Name = "Royal Gold by Bryan Whitby", Id = Constants.BryanWhitbyRoyalGold };
            installedPieces[Constants.BryanWhitbyRoyalBrown] = new BoardPiecesSetup() { Name = "Royal Brown by Bryan Whitby", Id = Constants.BryanWhitbyRoyalBrown };
            installedPieces[Constants.BryanWhitbyModernGold] = new BoardPiecesSetup() { Name = "Modern Gold by Bryan Whitby", Id = Constants.BryanWhitbyModernGold };
            installedPieces[Constants.BryanWhitbyModernBrown] = new BoardPiecesSetup() { Name = "Modern Brown by Bryan Whitby", Id = Constants.BryanWhitbyModernBrown };
            _unDeleteablePieces.Add(Constants.BearChess);
            _unDeleteablePieces.Add(Constants.Certabo);
            _unDeleteablePieces.Add(installedPieces[Constants.BryanWhitbyDali].Name);
            _unDeleteablePieces.Add(installedPieces[Constants.BryanWhitbyItalian].Name);
            _unDeleteablePieces.Add(installedPieces[Constants.BryanWhitbyRoyalGold].Name);
            _unDeleteablePieces.Add(installedPieces[Constants.BryanWhitbyRoyalBrown].Name);
            _unDeleteablePieces.Add(installedPieces[Constants.BryanWhitbyModernGold].Name);
            _unDeleteablePieces.Add(installedPieces[Constants.BryanWhitbyModernBrown].Name);
            _unDeleteableFields.Add(Constants.BearChess);
            _unDeleteableFields.Add(Constants.Certabo);

            _boardPath = boardPath;
            _piecesPath = piecesPath;
            _installedFields = installedFields;
            _installedPieces = installedPieces;

            comboBoxBoards.Items.Clear();
            comboBoxBoards.ItemsSource = installedFields.Values.OrderBy(f => f.Name);
            if (installedFields.ContainsKey(currentBoardFieldsSetupId))
            {
                comboBoxBoards.SelectedItem = installedFields[currentBoardFieldsSetupId];
                BoardFieldsSetup = installedFields[currentBoardFieldsSetupId];
            }
            comboBoxPieces.Items.Clear();
            comboBoxPieces.ItemsSource = installedPieces.Values.OrderBy(f => f.Name);
            if (installedPieces.ContainsKey(currentBoardPiecesSetupId))
            {
                comboBoxPieces.SelectedItem = installedPieces[currentBoardPiecesSetupId];
                BoardPiecesSetup = installedPieces[currentBoardPiecesSetupId];
            }
        }

        private void ButtonOpenBoard_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var confirmBoardImageWindow = new ConfirmBoardImageWindow(dialog.SelectedPath, _installedFields.Keys.ToArray()) {Owner = this};
                    var confirm = confirmBoardImageWindow.ShowDialog();
                    if (confirm.HasValue && confirm.Value)
                    {
                        var boardFieldsSetup = confirmBoardImageWindow.BoardFieldsSetup;
                        XmlSerializer serializer = new XmlSerializer(typeof(BoardFieldsSetup));
                        TextWriter textWriter = new StreamWriter(Path.Combine(_boardPath, boardFieldsSetup.Id + ".cfg"), false);
                        serializer.Serialize(textWriter, boardFieldsSetup);
                        textWriter.Close();
                        _installedFields[boardFieldsSetup.Id] = boardFieldsSetup;
                        comboBoxBoards.ItemsSource = null;
                        comboBoxBoards.ItemsSource = _installedFields.Values.OrderBy(f => f.Name);
                        comboBoxBoards.SelectedItem = _installedFields[boardFieldsSetup.Id];
                    }
                  
                }
            }
          
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOpenPieces_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var confirmPiecesWindow = new ConfirmPiecesWindow(dialog.SelectedPath, Array.Empty<string>(),string.Empty) { Owner = this };
                    var confirm = confirmPiecesWindow.ShowDialog();
                    if (confirm.HasValue && confirm.Value)
                    {
                        var boardPiecesSetup = confirmPiecesWindow.BoardPiecesSetup;
                        XmlSerializer serializer = new XmlSerializer(typeof(BoardPiecesSetup));
                        TextWriter textWriter = new StreamWriter(Path.Combine(_piecesPath, boardPiecesSetup.Id + ".cfg"), false);
                        serializer.Serialize(textWriter, boardPiecesSetup);
                        textWriter.Close();
                        _installedPieces[boardPiecesSetup.Id] = boardPiecesSetup;
                        comboBoxPieces.ItemsSource = null;
                        comboBoxPieces.ItemsSource = _installedPieces.Values.OrderBy(f => f.Name);
                        comboBoxPieces.SelectedItem = _installedPieces[boardPiecesSetup.Id];                      
                    }
                }
            }
        }

        private void ComboBoxBoards_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                BoardFieldsSetup = _installedFields[((BoardFieldsSetup)e.AddedItems[0]).Id];
                BoardSetupChangedEvent?.Invoke(this, new EventArgs());
            }
            else
            {
                BoardFieldsSetup = _installedFields.First(f => f.Value.Name.Equals(Constants.BearChess, StringComparison.OrdinalIgnoreCase)).Value;
                BoardSetupChangedEvent?.Invoke(this, new EventArgs());
                File.Delete(Path.Combine(_boardPath, ((BoardFieldsSetup)e.RemovedItems[0]).Id+".cfg"));                
            }
        }

        private void ComboBoxPieces_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                BoardPiecesSetup = _installedPieces[((BoardPiecesSetup)e.AddedItems[0]).Id];
                PiecesSetupChangedEvent?.Invoke(this, new EventArgs());
            }
            else
            {
                BoardPiecesSetup = _installedPieces.First(f => f.Value.Name.Equals(Constants.BearChess, StringComparison.OrdinalIgnoreCase)).Value;
                PiecesSetupChangedEvent?.Invoke(this, new EventArgs());
                File.Delete(Path.Combine(_piecesPath, ((BoardPiecesSetup)e.RemovedItems[0]).Id + ".cfg"));
            }
        }

        private void ButtonDeleteBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (_unDeleteableFields.Contains(BoardFieldsSetup.Name))
            {
                MessageBox.Show($"You cannot delete pre-installed board '{BoardFieldsSetup.Name}'", "Information", MessageBoxButton.OK,
                          MessageBoxImage.Hand);
                return;
            }
            
            if (MessageBox.Show($"Delete board '{BoardFieldsSetup.Name}'?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _installedFields.Remove(BoardFieldsSetup.Id);
                comboBoxBoards.ItemsSource = null;
                comboBoxBoards.ItemsSource = _installedFields.Values.OrderBy(f => f.Name);
                comboBoxBoards.SelectedItem = _installedFields.First(f => f.Value.Name.Equals(Constants.BearChess, StringComparison.OrdinalIgnoreCase)).Value;
            }
        }

        private void ButtonDeletePieces_OnClick(object sender, RoutedEventArgs e)
        {
            if (_unDeleteablePieces.Contains(BoardPiecesSetup.Name))
            {
                MessageBox.Show($"You cannot delete pre-installed pieces '{BoardPiecesSetup.Name}'", "Information", MessageBoxButton.OK,
                            MessageBoxImage.Hand);
                return;
            }

            if (MessageBox.Show($"Delete pieces '{BoardPiecesSetup.Name}'?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                _installedPieces.Remove(BoardPiecesSetup.Id);
                comboBoxPieces.ItemsSource = null;
                comboBoxPieces.ItemsSource = _installedPieces.Values.OrderBy(f => f.Name);
                comboBoxPieces.SelectedItem = _installedPieces.First(f => f.Value.Name.Equals(Constants.BearChess, StringComparison.OrdinalIgnoreCase)).Value;
            }
        }


        private void ComboBoxPieces_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                {
                    var fileInfo = new FileInfo(files[0]);
                    var confirmPiecesWindow = new ConfirmPiecesWindow(fileInfo.DirectoryName, new string[0], fileInfo.FullName) { Owner = this };
                    var confirm = confirmPiecesWindow.ShowDialog();
                    if (confirm.HasValue && confirm.Value)
                    {
                        var boardPiecesSetup = confirmPiecesWindow.BoardPiecesSetup;
                        XmlSerializer serializer = new XmlSerializer(typeof(BoardPiecesSetup));
                        TextWriter textWriter = new StreamWriter(Path.Combine(_piecesPath, boardPiecesSetup.Id + ".cfg"), false);
                        serializer.Serialize(textWriter, boardPiecesSetup);
                        textWriter.Close();
                        _installedPieces[boardPiecesSetup.Id] = boardPiecesSetup;
                        comboBoxPieces.ItemsSource = null;
                        comboBoxPieces.ItemsSource = _installedPieces.Values.OrderBy(f => f.Name);
                        comboBoxPieces.SelectedItem = _installedPieces[boardPiecesSetup.Id];
                    }
                }
            }
        }


        private void GroupBoxPieces_OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length !=1 || !files[0].EndsWith(".png",StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
    }
}

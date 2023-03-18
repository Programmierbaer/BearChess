using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessDatabase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public static class ExportGames
    {
        public static void Export(IList selectedItems, Database database,bool purePGN, Window owner)
        {
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("No games for export", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var saveFileDialog = new SaveFileDialog { Filter = "Games|*.pgn;" };
                var showDialog = saveFileDialog.ShowDialog(owner);
                var fileName = saveFileDialog.FileName;
                if (showDialog.Value && !string.IsNullOrWhiteSpace(fileName))
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    var infoWindow = new ProgressWindow
                                     {
                                         Owner = owner
                                     };

                    var sb = new StringBuilder();

                    infoWindow.SetMaxValue(selectedItems.Count);
                    infoWindow.Show();
                    var i = 0;
                    foreach (var selectedItem in selectedItems)
                    {
                        if (selectedItem is DatabaseGameSimple pgnGame)
                        {
                            sb.AppendLine(database.LoadGame(pgnGame.Id, purePGN).PgnGame.GetGame());
                            sb.AppendLine(string.Empty);
                            i++;
                            infoWindow.SetCurrentValue(i);
                        }
                    }

                    File.WriteAllText(fileName, sb.ToString());
                    infoWindow.Close();
                    MessageBox.Show(
                        $"{selectedItems.Count} games exported into{Environment.NewLine}{fileName} ",
                        "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
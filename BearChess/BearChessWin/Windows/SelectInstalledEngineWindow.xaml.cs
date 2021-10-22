using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessDatabase;
using www.SoLaNoSoft.com.BearChessWin.Windows;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SelectInstalledEngineWindow.xaml
    /// </summary>
    public partial class SelectInstalledEngineWindow : Window
    {
        private  ObservableCollection<UciInfo> _uciInfos;
        private  HashSet<string> _installedEngines;
        private readonly string[] _installedBooks;
        private readonly string _uciPath;
        public UciInfo SelectedEngine => (UciInfo) dataGridEngine.SelectedItem;

        public SelectInstalledEngineWindow()
        {
            InitializeComponent();
        }

        public SelectInstalledEngineWindow(IEnumerable<UciInfo> uciInfos, string[] installedBooks, string lastEngineId, string uciPath) : this()
        {
            _uciInfos =  new ObservableCollection<UciInfo>(uciInfos.OrderBy(e => e.Name).ToList());
            var firstOrDefault = _uciInfos.FirstOrDefault(u => u.Id.Equals(lastEngineId));
            if (firstOrDefault == null)
            {
                firstOrDefault = _uciInfos.Count > 0 ? _uciInfos[0] : null;
            }
            _installedBooks = installedBooks;
            _uciPath = uciPath;
            dataGridEngine.ItemsSource = _uciInfos;
            if (firstOrDefault != null)
            {
                dataGridEngine.SelectedIndex = _uciInfos.IndexOf(firstOrDefault);
            }
            dataGridEngine.ScrollIntoView(dataGridEngine.SelectedItem);
            _installedEngines = new HashSet<string>(_uciInfos.Select(u => u.Name));
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DataGridEngine_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedEngine != null)
            {
                DialogResult = true;
            }
        }

        private void ButtonConfigure_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedEngine == null)
            {
                return;
            }

            var uciConfigWindow = new UciConfigWindow(SelectedEngine, true, false, true) { Owner = this };
            var showDialog = uciConfigWindow.ShowDialog();
            if (showDialog.HasValue && showDialog.Value)
            {
                var uciInfo = uciConfigWindow.GetUciInfo();
                if (uciConfigWindow.SaveAsNew)
                {
                    if (SelectedEngine.Name.Equals(uciInfo.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        uciInfo.Name = SelectedEngine.Name + " (Copy)";
                    }
                    var uciPath = Path.Combine(_uciPath, uciInfo.Id);
                    if (!Directory.Exists(uciPath))
                    {
                        Directory.CreateDirectory(uciPath);
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(UciInfo));
                    TextWriter textWriter = new StreamWriter(Path.Combine(uciPath, uciInfo.Id + ".uci"), false);
                    serializer.Serialize(textWriter, uciInfo);
                    textWriter.Close();
                    _uciInfos.Add(uciInfo);
                }
                else
                {
                    SelectedEngine.Name = uciInfo.Name;
                    SelectedEngine.ClearOptionValues();
                    SelectedEngine.OpeningBook = uciInfo.OpeningBook;
                    SelectedEngine.OpeningBookVariation = uciInfo.OpeningBookVariation;
                    SelectedEngine.AdjustStrength = uciInfo.AdjustStrength;
                    SelectedEngine.CommandParameter = uciInfo.CommandParameter;
                    SelectedEngine.LogoFileName = uciInfo.LogoFileName;
                    foreach (var uciInfoOptionValue in uciInfo.OptionValues)
                    {
                        SelectedEngine.AddOptionValue(uciInfoOptionValue);
                    }
                    var uciPath = Path.Combine(_uciPath, SelectedEngine.Id);
                    XmlSerializer serializer = new XmlSerializer(typeof(UciInfo));
                    TextWriter textWriter = new StreamWriter(Path.Combine(uciPath, SelectedEngine.Id + ".uci"), false);
                    serializer.Serialize(textWriter, SelectedEngine);
                    textWriter.Close();
                }

                NameChanged();
            }
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedEngine == null)
            {
                return;
            }

            var engineName = SelectedEngine.Name;
            var engineId = SelectedEngine.Id;
            if (MessageBox.Show($"Uninstall engine '{engineName}'?", "Uninstall Engine",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) !=
                MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var uciPath = Path.Combine(_uciPath, engineId);
                var fileNames = Directory.GetFiles(uciPath);
                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
                Directory.Delete(uciPath);
                _installedEngines.Remove(engineName);
                _uciInfos.Remove(SelectedEngine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on uninstall engine '{engineName}'{Environment.NewLine}{ex.Message}", "Uninstall UCI Engine",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ButtonInstall_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "UCI Engine|*.exe|UCI Engine|*.cmd|All Files|*.*" };
            var showDialog = openFileDialog.ShowDialog(this);
            if (showDialog.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                LoadNewEngine(openFileDialog.FileName);
            }
        }

        private void NameChanged()
        {
            _uciInfos = new ObservableCollection<UciInfo>(_uciInfos.OrderBy(e => e.Name).ToList());
            var firstOrDefault = _uciInfos.FirstOrDefault(u => u.Id.Equals(SelectedEngine.Id));
            if (firstOrDefault == null)
            {
                firstOrDefault = _uciInfos.Count > 0 ? _uciInfos[0] : null;
            }
          
            dataGridEngine.ItemsSource = _uciInfos;
            if (firstOrDefault != null)
            {
                dataGridEngine.SelectedIndex = _uciInfos.IndexOf(firstOrDefault);
            }

            _installedEngines = new HashSet<string>(_uciInfos.Select(u => u.Name));
        }


        private void LoadNewEngine(string fileName)
        {
            try
            {
                string parameters = string.Empty;
                string avatarName = string.Empty;
                if (fileName.EndsWith("MessChess.exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    var fileInfo = new FileInfo(fileName);
                    string enginesList = Path.Combine(fileInfo.DirectoryName, "Engines.lst");
                    var parameterSelectionWindow = new ParameterSelectionWindow() { Owner = this };
                    if (File.Exists(enginesList))
                    {
                        var readAllLines = File.ReadAllLines(enginesList);
                        parameterSelectionWindow.ShowList(readAllLines);
                    }
                    parameterSelectionWindow.SetLabel("Engine:");
                    var showDialog = parameterSelectionWindow.ShowDialog();
                    if (showDialog.HasValue && showDialog.Value)
                    {
                        parameters = parameterSelectionWindow.SelectedEngine;
                    }
                    else
                    {
                        return;
                    }
                }
                if (fileName.EndsWith("avatar.exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    var fileInfo = new FileInfo(fileName);
                    string avatars = Path.Combine(fileInfo.DirectoryName, "avatar_weights");
                    var parameterSelectionWindow = new ParameterSelectionWindow() { Owner = this };
                    if (Directory.Exists(avatars))
                    {
                        var avatarList = Directory.GetFiles(avatars,"*.zip",SearchOption.TopDirectoryOnly);
                        List<string> avList = new List<string>();
                        foreach (var s in avatarList)
                        {
                            if (s.Contains(@"\"))
                            {
                                avList.Add(s.Substring(s.LastIndexOf(@"\")+1));
                            }
                        }

                        parameterSelectionWindow.ShowList(avList.ToArray());
                        parameterSelectionWindow.SetLabel("Avatar:");
                        parameterSelectionWindow.ShowParameterButton(true);
                        var showDialog = parameterSelectionWindow.ShowDialog();
                        if (showDialog.HasValue && showDialog.Value)
                        {
                            avatarName = parameterSelectionWindow.SelectedEngine;
                            parameters = $"--weights \"{Path.Combine(avatars, avatarName)}\"";
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        var openFileDialog = new OpenFileDialog { Filter = "Avatar Engine|*.zip|All Files|*.*" };
                        var showDialogAvatar = openFileDialog.ShowDialog(this);
                        if (showDialogAvatar.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                        {
                            var info = new FileInfo(openFileDialog.FileName);
                            avatarName = info.Name;
                            parameters = $"--weights \"{info.FullName}\"";
                        }
                    }
                }
                UciInstaller uciInstaller = new UciInstaller();
                UciInfo uciInfo = uciInstaller.Install(fileName, parameters);
                if (!uciInfo.Valid)
                {
                    throw new Exception($"{uciInfo.FileName} is not a valid UCI engine");
                }

                if (uciInfo.Name.Equals("Avatar", StringComparison.OrdinalIgnoreCase))
                {
                    uciInfo.Name = "Avatar " + avatarName.Replace("_"," ").Replace(".zip",string.Empty);
                }
                if (_installedEngines.Contains(uciInfo.Name))
                {
                    MessageBox.Show(
                        this,
                        $"Engine '{uciInfo.Name}' already installed!", "UCI Engine", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool isAdded = false;
                uciInfo.Id = "uci" + Guid.NewGuid().ToString("N");
                if (MessageBox.Show(this, $"Install UCI engine{Environment.NewLine}{uciInfo.Name}{Environment.NewLine}Author: {uciInfo.Author}",
                        "UCI Engine", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _installedEngines.Add(uciInfo.Name);
                    var uciPath = Path.Combine(_uciPath, uciInfo.Id);
                    if (!Directory.Exists(uciPath))
                    {
                        Directory.CreateDirectory(uciPath);
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(UciInfo));
                    TextWriter textWriter = new StreamWriter(Path.Combine(uciPath, uciInfo.Id + ".uci"), false);
                    serializer.Serialize(textWriter, uciInfo);
                    textWriter.Close();
                    UciConfigWindow uciConfigWindow = new UciConfigWindow(uciInfo, true, false, false) { Owner = this };
                    var dialog = uciConfigWindow.ShowDialog();

                    if (dialog.HasValue && dialog.Value)
                    {
                        var info = uciConfigWindow.GetUciInfo();
                        for (int i = 0; i < _uciInfos.Count; i++)
                        {
                            if (_uciInfos[i].Name.CompareTo(info.Name) < 0)
                            {
                                continue;

                            }

                            isAdded = true;
                            _uciInfos.Insert(i, info);
                            break;
                        }

                        if (!isAdded)
                        {
                            _uciInfos.Add(info);
                        }
                        //_uciInfos.Add(info);
                        serializer = new XmlSerializer(typeof(UciInfo));
                        textWriter = new StreamWriter(Path.Combine(uciPath, uciInfo.Id + ".uci"), false);
                        serializer.Serialize(textWriter, info);
                        textWriter.Close();
                    }
                    else
                    {
                        for (int i = 0; i < _uciInfos.Count; i++)
                        {
                            if (_uciInfos[i].Name.CompareTo(uciInfo.Name) < 0)
                            {
                                continue;

                            }
                            isAdded = true;
                            _uciInfos.Insert(i, uciInfo);
                            break;
                        }
                        if (!isAdded)
                        {
                            _uciInfos.Add(uciInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error on install chess engine", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DataGridEngine_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                {
                    var fileInfo = new FileInfo(files[0]);
                    LoadNewEngine(fileInfo.FullName);
                }
            }
        }

        private void DataGridEngine_OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length != 1 || !files[0].EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
    }
}

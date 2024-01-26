using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChess.DGTLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureDGT.xaml
    /// </summary>
    public partial class WinConfigureDGT : Window
    {
        private readonly Configuration _configuration;
        private readonly bool _useBluetooth;
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private List<string> _portNames;
        private List<string> _allPortNames;
        private readonly ILogging _fileLogger;

        public string SelectedPortName => (string)comboBoxComPorts.SelectedItem;

        public WinConfigureDGT(Configuration configuration, bool useBluetooth)
        {
            InitializeComponent();
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Chess Miscel");
            textBlockClock.FontFamily = fontFamily;
            textBlockClock.Text = "c";
            _configuration = configuration;
            _useBluetooth = useBluetooth;
            _fileName = Path.Combine(_configuration.FolderPath, DGTLoader.EBoardName, $"{DGTLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "DGTCfg.log"), 10, 10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }

            _allPortNames = new List<string> { "<auto>" };
            if (_useBluetooth)
            {
                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                _portNames = SerialCommunicationTools
                            .GetBTComPort(DGTLoader.EBoardName, configuration, _fileLogger, true, false, false).ToList();
                comPortSearchWindow.Close();
            }
            else
            {
                _portNames = SerialCommunicationTools.GetPortNames(string.Empty).ToList();
            }

            _portNames.ForEach(f => _allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            _eChessBoardConfiguration.UseBluetooth = useBluetooth;
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
            if (_portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
            }

            checkBoxUseClock.IsChecked = _eChessBoardConfiguration.UseClock;
            checkBoxShowOnlyMoves.IsChecked = _eChessBoardConfiguration.ClockShowOnlyMoves;
            checkBoxSwitchSide.IsChecked = _eChessBoardConfiguration.ClockSwitchSide;
            checkBoxUpperCase.IsChecked = _eChessBoardConfiguration.ClockUpperCase;
            checkBoxBeep.IsChecked = _eChessBoardConfiguration.ClockBeep;
            numericUpDownUserControlDuration.Value = _eChessBoardConfiguration.BeepDuration;
            textBlockClock.Text = _eChessBoardConfiguration.ClockSwitchSide ? "C" : "c";
            if (_eChessBoardConfiguration.LongMoveFormat)
            {
                radioButtonLongFormat.IsChecked = true;
            }
            else
            {
                radioButtonShortFormat.IsChecked = true;
            }
        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                {
                    Owner = this
                };
                var dgtLoader = new DGTLoader(true, DGTLoader.EBoardName);
                var portName = comboBoxComPorts.SelectionBoxItem.ToString();
                if (portName.Contains("auto"))
                {
                    infoWindow.SetMaxValue(_portNames.Count);
                    infoWindow.Show();
                    var i = 0;
                    foreach (var name in _portNames)
                    {
                        infoWindow.SetCurrentValue(i, name);
                        infoWindow.SetCurrentValue(i);
                        i++;
                        if (dgtLoader.CheckComPort(name))
                        {
                            infoWindow.Close();
                            MessageBox.Show($"Check successful for {name}", "Check", MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                            comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                            return;
                        }
                    }

                    infoWindow.Close();
                    MessageBox.Show("Check failed for all COM ports", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;

                }

                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (dgtLoader.CheckComPort(portName))
                {
                    infoWindow.Close();
                    MessageBox.Show($"Check successful for {portName}", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                else
                {
                    infoWindow.Close();
                    MessageBox.Show($"Check failed for {portName}", "Check", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }

        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            _eChessBoardConfiguration.UseClock = checkBoxUseClock.IsChecked.HasValue && checkBoxUseClock.IsChecked.Value;
            _eChessBoardConfiguration.ClockShowOnlyMoves = checkBoxShowOnlyMoves.IsChecked.HasValue && checkBoxShowOnlyMoves.IsChecked.Value;
            _eChessBoardConfiguration.ClockSwitchSide = checkBoxSwitchSide.IsChecked.HasValue && checkBoxSwitchSide.IsChecked.Value;
            _eChessBoardConfiguration.ClockUpperCase = checkBoxUpperCase.IsChecked.HasValue && checkBoxUpperCase.IsChecked.Value;
            _eChessBoardConfiguration.LongMoveFormat = radioButtonLongFormat.IsChecked.HasValue && radioButtonLongFormat.IsChecked.Value;
            _eChessBoardConfiguration.ClockBeep = checkBoxBeep.IsChecked.HasValue && checkBoxBeep.IsChecked.Value;
            _eChessBoardConfiguration.BeepDuration = numericUpDownUserControlDuration.Value;
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CheckBoxUseClock_OnChecked(object sender, RoutedEventArgs e)
        {
            checkBoxShowOnlyMoves.IsEnabled = true;
            checkBoxSwitchSide.IsEnabled = true;
            checkBoxUpperCase.IsEnabled = true;
            stackPanelStyle.IsEnabled = true;
            checkBoxBeep.IsEnabled = true;
            numericUpDownUserControlDuration.IsEnabled = true;
        }

        private void CheckBoxUseClock_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxShowOnlyMoves.IsEnabled = false;
            checkBoxSwitchSide.IsEnabled = false;
            checkBoxUpperCase.IsEnabled = false;
            stackPanelStyle.IsEnabled = false;
            checkBoxBeep.IsEnabled = false;
            numericUpDownUserControlDuration.IsEnabled = false;
        }

        private void CheckBoxSwitchSide_OnChecked(object sender, RoutedEventArgs e)
        {
            textBlockClock.Text = "C";
            checkBoxSwitchSide.ToolTip = "Left side shows the time for black";
        }

        private void CheckBoxSwitchSide_OnUnchecked(object sender, RoutedEventArgs e)
        {
            textBlockClock.Text = "c";
            checkBoxSwitchSide.ToolTip = "Left side shows the time for white";
        }
    }
}

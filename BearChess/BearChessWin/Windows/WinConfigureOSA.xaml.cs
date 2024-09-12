using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using www.SoLaNoSoft.com.BearChess.CitrineLoader;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.OSALoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;
using www.SoLaNoSoft.com.BearChessTools;
using System.Resources;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureOSA.xaml
    /// </summary>
    public partial class WinConfigureOSA : Window
    {
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private readonly List<string> _portNames;
        private readonly List<string> _baudRates;
        private readonly List<string> _allPortNames;
        private readonly List<string> _allBaudRates;
        private readonly ILogging _fileLogger;
        private readonly ResourceManager _rm;

        public string SelectedPortName => (string)comboBoxComPorts.SelectedItem;

        public WinConfigureOSA(Configuration configuration)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _fileName = Path.Combine(configuration.FolderPath, OSALoader.EBoardName, $"{OSALoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "OSACfg.log"), 10, 10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }

            _allPortNames = new List<string> { "<auto>" };
            _allBaudRates = new List<string> { "<auto>" };

            _portNames = SerialCommunicationTools.GetPortNames(string.Empty).ToList();
            _portNames.ForEach(f => _allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            _baudRates = SerialCommunicationTools.GetBaudRates().ToList();
            _baudRates.ForEach(f => _allBaudRates.Add(f));
            comboBoxBaud.ItemsSource = _allBaudRates;

            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;
            textBlockCurrentBaud.Text = _eChessBoardConfiguration.Baud;
            if (_portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
            }
            comboBoxBaud.SelectedIndex = _allBaudRates.IndexOf(_eChessBoardConfiguration.Baud);
        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                                 {
                                     Owner = this
                                 };
                var osaLoader = new OSALoader(true, OSALoader.EBoardName);
                var portName = comboBoxComPorts.SelectionBoxItem.ToString();
                var baud = comboBoxBaud.SelectionBoxItem.ToString();
                if (portName.Contains("auto"))
                {
                    infoWindow.SetMaxValue(_portNames.Count);
                    infoWindow.Show();
                    var i = 0;
                    foreach (var name in _portNames)
                    {
                        i++;
                        if (baud.Contains("auto"))
                        {
                            foreach (var baudRate in _baudRates)
                            {
                                infoWindow.SetCurrentValue(i, $"{name} {_rm.GetString("And")} {baudRate} {_rm.GetString("BaudValue")}");
                                infoWindow.SetCurrentValue(i);
                                if (osaLoader.CheckComPort(name, baudRate))
                                {
                                    infoWindow.Close();
                                    MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {name} {_rm.GetString("And")} {baudRate} {_rm.GetString("BaudValue")}", _rm.GetString("Check"),
                                                    MessageBoxButton.OK,
                                                    MessageBoxImage.Information);
                                    comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                                    comboBoxBaud.SelectedIndex = _allBaudRates.IndexOf(baudRate);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            infoWindow.SetCurrentValue(i, $"{name} {baud} baud");
                            if (osaLoader.CheckComPort(name, baud))
                            {
                                infoWindow.Close();
                                MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {name} {_rm.GetString("And")} {baud} {_rm.GetString("BaudValue")}", _rm.GetString("Check"), MessageBoxButton.OK,
                                                MessageBoxImage.Information);
                                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                                return;
                            }
                        }
                    }

                    infoWindow.Close();
                    MessageBox.Show($"{_rm.GetString("CheckConnectionFailedForAll")} {_rm.GetString("And")} {_rm.GetString("BaudRates")}", _rm.GetString("Check"), MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;

                }


                if (baud.Contains("auto"))
                {
                    infoWindow.SetMaxValue(_baudRates.Count);
                    infoWindow.Show();
                    var i = 0;
                    foreach (var baudRate in _baudRates)
                    {
                        i++;
                        infoWindow.SetCurrentValue(i, $"{portName} {_rm.GetString("And")} {baudRate} {_rm.GetString("BaudValue")}");
                        if (osaLoader.CheckComPort(portName, baudRate))
                        {
                            infoWindow.Close();
                            MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {portName} {_rm.GetString("And")} {baudRate} {_rm.GetString("BaudValue")}", _rm.GetString("Check"),
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                            comboBoxBaud.SelectedIndex = _allBaudRates.IndexOf(baudRate);
                            return;
                        }

                    }

                    infoWindow.Close();
                    MessageBox.Show($"{_rm.GetString("CheckConnectionFailed")} {portName} {_rm.GetString("AndAllBaudRates")}", _rm.GetString("Check"), MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;

                }

                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, $"{portName} {_rm.GetString("And")} {baud} {_rm.GetString("BaudValue")}");
                if (osaLoader.CheckComPort(portName, baud))
                {
                    infoWindow.Close();
                    MessageBox.Show($"{_rm.GetString("CheckConnectionSuccess")} {portName} {_rm.GetString("And")} {baud} {_rm.GetString("BaudValue")}", _rm.GetString("Check"), MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }


                infoWindow.Close();
                MessageBox.Show($"{_rm.GetString("CheckConnectionFailed")} {portName} {_rm.GetString("And")} {baud} {_rm.GetString("BaudValue")}", _rm.GetString("Check"), MessageBoxButton.OK,
                                MessageBoxImage.Error);

            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }

        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            _eChessBoardConfiguration.Baud = comboBoxBaud.SelectionBoxItem.ToString();
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

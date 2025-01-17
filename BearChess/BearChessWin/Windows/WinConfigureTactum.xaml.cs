using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.TabuTronic.Tactum.Loader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessBTLETools;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWin.Windows;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureTactum.xaml
    /// </summary>
    public partial class WinConfigureTactum : Window
    {
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private readonly List<string> _portNames;
        private readonly List<string> _allPortNames;
        private readonly ILogging _fileLogger;
        private string _currentHelpText;
        private readonly ISpeech _synthesizer;
        private readonly ResourceManager _rm;

        public WinConfigureTactum(Configuration configuration, bool useBluetoothLE)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _synthesizer = BearChessSpeech.Instance;
            _currentHelpText = string.Empty;

            _fileName = Path.Combine(configuration.FolderPath, TabuTronicTactumLoader.EBoardName,
                $"{TabuTronicTactumLoader.EBoardName}Cfg.xml");
            try
            {
                var fileInfo = new FileInfo(_fileName);
                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                    Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
                }

                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "TabuTronicTactumCfg.log"), 10,
                    10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }

            _allPortNames = new List<string> { "<auto>" };
            if (useBluetoothLE)
            {
                var comPortSearchWindow = new COMPortSearchWindow();
                comPortSearchWindow.Show();
                _portNames = SerialCommunicationTools
                    .GetBTComPort(TabuTronicTactumLoader.EBoardName, configuration, _fileLogger, false, true, false)
                    .ToList();
                var btComPort = SerialBTLECommunicationTools.GetBTComPort(Constants.TabutronicTactum);
                comPortSearchWindow.Close();
                if (btComPort.Length > 0)
                {
                    var firstOrDefault = SerialBTLECommunicationTools.DeviceIdList.FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        var btleComPort = new BTLEComPort(firstOrDefault.ID, firstOrDefault.Name, _fileLogger);
                        btleComPort.Open();
                    }
                }
            }
            else
            {
                _portNames = SerialCommunicationTools.GetPortNames("Silicon Labs").ToList();
            }

            _portNames.ForEach(f => _allPortNames.Add(f));
            comboBoxComPorts.ItemsSource = _allPortNames;
            comboBoxComPorts.SelectedIndex = 0;

            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            _eChessBoardConfiguration.UseBluetooth = useBluetoothLE;
            checkBoxSayLiftUpDown.IsChecked = _eChessBoardConfiguration.SayLiftUpDownFigure;
            checkBoxPossibleMoves.IsChecked = _eChessBoardConfiguration.ShowPossibleMoves;
            checkBoxBestMove.IsChecked = _eChessBoardConfiguration.ShowPossibleMovesEval;
            textBlockCurrentPort.Text = _eChessBoardConfiguration.PortName;

            _synthesizer?.SpeakAsync(_rm.GetString("ConfigureTactumSpeech"));

            if (_portNames.Count == 0)
            {
                textBlockInformation.Visibility = Visibility.Visible;
                _synthesizer?.SpeakAsync(textBlockInformation.Text);
            }
            else
            {
                comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(_eChessBoardConfiguration.PortName);
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
                var tactumLoader = new TabuTronicTactumLoader(true, TabuTronicTactumLoader.EBoardName);
                var portName = comboBoxComPorts.SelectionBoxItem.ToString();
                if (portName.Contains("auto"))
                {
                    infoWindow.SetWait(_rm.GetString("PleaseWait"));
                    infoWindow.SetMaxValue(_portNames.Count);
                    infoWindow.Show();
                    var i = 0;
                    foreach (var name in _portNames)
                    {
                        infoWindow.SetCurrentValue(i, name);
                        infoWindow.SetCurrentValue(i);
                        i++;
                        if (tactumLoader.CheckComPort(name))
                        {
                            infoWindow.Close();
                            _synthesizer.SpeakAsync($"{_rm.GetString("CheckConnectionSuccess")} {name}");

                            comboBoxComPorts.SelectedIndex = _allPortNames.IndexOf(name);
                            return;
                        }
                    }

                    infoWindow.Close();

                    _synthesizer.SpeakAsync(_rm.GetString("CheckConnectionFailedForAll"));

                    return;
                }

                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (tactumLoader.CheckComPort(portName))
                {
                    infoWindow.Close();
                    _synthesizer.SpeakAsync($"{_rm.GetString("CheckConnectionSuccess")} {portName}");
                }
                else
                {
                    infoWindow.Close();
                    _synthesizer.SpeakAsync($"{_rm.GetString("CheckConnectionFailed")} {portName}");
                }
            }
            catch (Exception ex)
            {
                _fileLogger?.LogError(ex);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _synthesizer.Clear();
            _eChessBoardConfiguration.PortName = comboBoxComPorts.SelectionBoxItem.ToString();
            _eChessBoardConfiguration.SayLiftUpDownFigure =
                checkBoxSayLiftUpDown.IsChecked.HasValue && checkBoxSayLiftUpDown.IsChecked.Value;
            _eChessBoardConfiguration.ShowPossibleMoves =
                checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            _eChessBoardConfiguration.ShowPossibleMovesEval =
                checkBoxBestMove.IsChecked.HasValue && checkBoxBestMove.IsChecked.Value;
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SayCurrentHelpText()
        {
            _currentHelpText = $"{_rm.GetString("CurrentCOMPort")} {textBlockCurrentPort.Text}";
            _synthesizer?.SpeakAsync(_currentHelpText);
            _currentHelpText = $"{_rm.GetString("SelectedCOMPort")} {comboBoxComPorts.SelectionBoxItem}";
            _synthesizer?.SpeakAsync(_currentHelpText);
            bool selected = checkBoxSayLiftUpDown.IsChecked.HasValue && checkBoxSayLiftUpDown.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("SayLiftUpDown")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
            selected = checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("SayAllMovesSelectedFigure")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
            selected = checkBoxBestMove.IsChecked.HasValue && checkBoxBestMove.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("SayBestMoveSelectedFigure")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _currentHelpText += ".";
            _synthesizer?.SpeakAsync(_currentHelpText);
        }

        private void WinConfigureTactum_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                _synthesizer?.SpeakAsync(_rm.GetString("ConfigureTactumSpeech"));
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    SayCurrentHelpText();
                }
            }

            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ButtonOk_OnClick(sender, e);
            }
        }

        private void ComboBoxComPorts_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                buttonCheck.ToolTip = $"{_rm.GetString("CheckSelectedCOMPortTip")} {e.AddedItems[0]}";
                _currentHelpText = $"{_rm.GetString("CurrentCOMPort")} {e.AddedItems[0]}";
                _synthesizer?.SpeakAsync(_currentHelpText);
            }
        }

        private void ButtonOk_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync($"{_rm.GetString("Button")} {_rm.GetString("Ok")}");
        }

        private void ButtonCancel_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync($"{_rm.GetString("Button")} {_rm.GetString("Cancel")}");
        }

        private void ButtonCheck_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _currentHelpText =
                $"{_rm.GetString("Button")} {_rm.GetString("Check")}  {_rm.GetString("CurrentCOMPort")} {comboBoxComPorts.SelectionBoxItem}";
            _synthesizer?.SpeakAsync(_currentHelpText);
            // _synthesizer?.SpeakAsync(_rm.GetString("CheckSelectedCOMPortTip"));
        }

        private void CheckBoxSayLiftUpDown_OnGotFocus(object sender, RoutedEventArgs e)
        {
            bool selected = checkBoxSayLiftUpDown.IsChecked.HasValue && checkBoxSayLiftUpDown.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("SayLiftUpDown")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _synthesizer?.SpeakAsync(_currentHelpText);
        }

        private void CheckBoxPossibleMoves_OnGotFocus(object sender, RoutedEventArgs e)
        {
            bool selected = checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("SayAllMovesSelectedFigure")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _synthesizer?.SpeakAsync(_currentHelpText);
        }

        private void CheckBoxBestMove_OnGotFocus(object sender, RoutedEventArgs e)
        {
            bool selected = checkBoxBestMove.IsChecked.HasValue && checkBoxBestMove.IsChecked.Value;
            _currentHelpText = $"{_rm.GetString("SayBestMoveSelectedFigure")} ";
            _currentHelpText += selected ? _rm.GetString("IsSelected") : _rm.GetString("IsUnSelected");
            _synthesizer?.SpeakAsync(_currentHelpText);
        }

        private void CheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync(_rm.GetString("IsSelected"));
        }

        private void CheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _synthesizer?.SpeakAsync(_rm.GetString("IsUnSelected"));
        }

        private void ComboBoxComPorts_OnGotFocus(object sender, RoutedEventArgs e)
        {
            _currentHelpText = $"{_rm.GetString("ComboBox")} {_rm.GetString("ComPorts")} ";
            _currentHelpText += $"{_rm.GetString("CurrentCOMPort")} {comboBoxComPorts.SelectionBoxItem}";
            _synthesizer?.SpeakAsync(_currentHelpText);
        }
    }
}
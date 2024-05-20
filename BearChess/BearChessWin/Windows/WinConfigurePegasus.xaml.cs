using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChess.PegasusLoader;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using www.SoLaNoSoft.com.BearChessWpfCustomControlLib;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigurePegasus.xaml
    /// </summary>
    public partial class WinConfigurePegasus : Window
    {
        private readonly Configuration _configuration;
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private readonly ILogging _fileLogger;
        private PegasusLoader _loader;

        public WinConfigurePegasus(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            _fileName = Path.Combine(_configuration.FolderPath, PegasusLoader.EBoardName,
                $"{PegasusLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                Directory.CreateDirectory(Path.Combine(fileInfo.DirectoryName, "log"));
            }

            try
            {
                _fileLogger = new FileLogger(Path.Combine(fileInfo.DirectoryName, "log", "PegasusCfg.log"), 10, 10);
                _fileLogger.Active = bool.Parse(configuration.GetConfigValue("writeLogFiles", "true"));
            }
            catch
            {
                _fileLogger = null;
            }
            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            _eChessBoardConfiguration.UseBluetooth = true;
            checkBoxOwnMoves.IsChecked = _eChessBoardConfiguration.ShowOwnMoves;
            checkBoxBestMove.IsChecked = _eChessBoardConfiguration.ShowPossibleMovesEval;
            sliderDim.Value = _eChessBoardConfiguration.DimLevel >= sliderDim.Minimum && _eChessBoardConfiguration.DimLevel <= sliderDim.Maximum ? _eChessBoardConfiguration.DimLevel : 1;
            sliderSpeed.Value = _eChessBoardConfiguration.Debounce >= sliderSpeed.Minimum && _eChessBoardConfiguration.Debounce <= sliderSpeed.Maximum ? _eChessBoardConfiguration.Debounce : 2;
        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new ProgressWindow()
                {
                    Owner = this
                };
                var portName = "BTLE";
                var pegasusLoader = new PegasusLoader(true, PegasusLoader.EBoardName);
                infoWindow.SetMaxValue(1);
                infoWindow.Show();
                infoWindow.SetCurrentValue(1, portName);
                if (pegasusLoader.CheckComPort(portName))
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
            _eChessBoardConfiguration.ShowMoveLine = false;
            _eChessBoardConfiguration.ShowOwnMoves = checkBoxOwnMoves.IsChecked.HasValue && checkBoxOwnMoves.IsChecked.Value;
            _eChessBoardConfiguration.ShowPossibleMoves = false;
            _eChessBoardConfiguration.ShowPossibleMovesEval = checkBoxBestMove.IsChecked.HasValue && checkBoxBestMove.IsChecked.Value;
            _eChessBoardConfiguration.DimLeds = true;
            _eChessBoardConfiguration.DimLevel = (int)sliderDim.Value;
            _eChessBoardConfiguration.Debounce = (int)sliderSpeed.Value;
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CheckBoxOwnMoves_OnChecked(object sender, RoutedEventArgs e)
        {
            checkBoxBestMove.IsEnabled = false;
        }

        private void CheckBoxOwnMoves_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxBestMove.IsEnabled = true;
        }

      

        private void CheckBoxBestMove_OnChecked(object sender, RoutedEventArgs e)
        {
            checkBoxOwnMoves.IsEnabled = false;
            checkBoxOwnMoves.IsChecked = false;

        }

        private void CheckBoxBestMove_OnUnchecked(object sender, RoutedEventArgs e)
        {
            checkBoxOwnMoves.IsEnabled = true;
        }

        private void ButtonIncrementDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDim.Value < sliderDim.Maximum)
            {
                sliderDim.Value++;
            }
        }

        private void ButtonDecrementDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDim.Value > sliderDim.Minimum)
            {
                sliderDim.Value--;
            }
        }

        private void SliderDim_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loader != null)
            {
              
                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetDebounce((int)sliderSpeed.Value);
                _loader.SetLedsFor(new SetLEDsParameter()
                {
                    FieldNames = new[] { "e2", "e4" },
                    IsThinking = false,
                    ForceShow = true
                });
            }
        }

        private void ButtonShowDim_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                _loader = new PegasusLoader();
                buttonShowDim.Visibility = Visibility.Collapsed;
                buttonShowDim2.Visibility = Visibility.Visible;

                _loader.SetDebounce((int)sliderSpeed.Value);
                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetLedsFor(new SetLEDsParameter()
                {
                    FieldNames = new[] { "e2", "e4" },
                    IsThinking = false,
                    ForceShow = true
                });
            }
            else
            {
                buttonShowDim.Visibility = Visibility.Visible;
                buttonShowDim2.Visibility = Visibility.Collapsed;
           //     _loader.SetAllLedsOn();
                _loader.SetAllLedsOff(true);
                Thread.Sleep(500);
                _loader.Release();
                _loader.Close();
                Thread.Sleep(500);
                _loader.Dispose();
                _loader = null;
            }
        }
      

        private void ButtonSpeedDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderSpeed.Value > sliderSpeed.Minimum)
            {
                sliderSpeed.Value--;
            }
        }


        private void ButtonSpeedAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderSpeed.Value < sliderSpeed.Maximum)
            {
                sliderSpeed.Value++;
            }
        }

        private void SliderSpeed_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loader != null)
            {

                _loader.DimLeds((int)sliderDim.Value);
                _loader.SetDebounce((int)sliderSpeed.Value);
                _loader.SetLedsFor(new SetLEDsParameter()
                {
                    FieldNames = new[] { "e2", "e4" },
                    IsThinking = false,
                    ForceShow = true
                });
            }
        }
    }
}

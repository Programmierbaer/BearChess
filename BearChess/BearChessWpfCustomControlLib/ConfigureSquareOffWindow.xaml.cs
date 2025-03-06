using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.SquareOffProLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
{
    /// <summary>
    /// Interaktionslogik für ConfigureSquareOffWindow.xaml
    /// </summary>
    public partial class ConfigureSquareOffWindow : Window
    {
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;        
        private const int _defaultScanTime = 250;
        private const int _defaultDelayTime = 100;
        private const int _defaultScanIncr = 50;
        private const int _defaultDelayIncr = 10;
        private readonly ResourceManager _rm;

        public ConfigureSquareOffWindow(Configuration configuration)
        {
            InitializeComponent();
            _rm = SpeechTranslator.ResourceManager;
            _fileName = Path.Combine(configuration.FolderPath, SquareOffProLoader.EBoardName,
                $"{SquareOffProLoader.EBoardName}Cfg.xml");
            var fileInfo = new FileInfo(_fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            _eChessBoardConfiguration = EChessBoardConfiguration.Load(_fileName);
            sliderScanTime.Value = _eChessBoardConfiguration.ScanTime < sliderScanTime.Minimum || _eChessBoardConfiguration.ScanTime > sliderScanTime.Maximum
                ? _defaultScanTime
                : _eChessBoardConfiguration.ScanTime;
            sliderDelayTime.Value = _eChessBoardConfiguration.Debounce < sliderDelayTime.Minimum || _eChessBoardConfiguration.Debounce > sliderDelayTime.Maximum
                ? _defaultDelayTime
                : _eChessBoardConfiguration.Debounce;
            checkBoxMoveLine.IsChecked = _eChessBoardConfiguration.ShowMoveLine;
            checkBoxDefault.IsChecked = true;
            //checkBoxOwnMoves.IsChecked = _eChessBoardConfiguration.ShowOwnMoves;
            checkBoxPossibleMoves.IsChecked = _eChessBoardConfiguration.ShowPossibleMoves;
            checkBoxBestMove.IsChecked = _eChessBoardConfiguration.ShowPossibleMovesEval;
            SetScanText();
            SetDelayText();
        }
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.ScanTime = (int)sliderScanTime.Value;
            _eChessBoardConfiguration.Debounce = (int)sliderDelayTime .Value;
            _eChessBoardConfiguration.ShowMoveLine = checkBoxMoveLine.IsChecked.HasValue && checkBoxMoveLine.IsChecked.Value;
            _eChessBoardConfiguration.ShowOwnMoves = false;
            _eChessBoardConfiguration.ShowPossibleMoves = checkBoxPossibleMoves.IsChecked.HasValue && checkBoxPossibleMoves.IsChecked.Value;
            _eChessBoardConfiguration.ShowPossibleMovesEval = checkBoxBestMove.IsChecked.HasValue && checkBoxBestMove.IsChecked.Value;
            EChessBoardConfiguration.Save(_eChessBoardConfiguration, _fileName);
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonTimeDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderScanTime.Value >= sliderScanTime.Minimum + _defaultScanIncr)
            {
                sliderScanTime.Value -= _defaultScanIncr;
            }
            else
            {
                sliderScanTime.Value = sliderScanTime.Minimum;
            }
        }

        private void ButtonTimeAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderScanTime.Value <= sliderScanTime.Maximum - _defaultScanIncr)
            {
                sliderScanTime.Value += _defaultScanIncr;
            }
            else
            {
                sliderScanTime.Value = sliderScanTime.Maximum;
            }
        }

        private void SetScanText()
        {
            if (textBlockScansPerSec != null)
            {
                textBlockScansPerSec.Text = $"{_rm.GetString("Every")} {sliderScanTime.Value.ToString("###")} ms.";
            }
        }
        private void SetDelayText()
        {
            if (textBlockDelayPerSec != null)
            {
                textBlockDelayPerSec.Text = $"{_rm.GetString("Every")} {sliderDelayTime.Value.ToString("###")} ms.";
            }
        }

        private void SliderScan_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetScanText();
        }

        private void ButtonResetScan_OnClick(object sender, RoutedEventArgs e)
        {
            sliderScanTime.Value = _defaultScanTime;
        }

        private void SliderDelay_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetDelayText();
        }

        private void ButtonDelayTimeDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDelayTime.Value >= sliderDelayTime.Minimum + _defaultDelayIncr)
            {
                sliderDelayTime.Value -= _defaultDelayIncr;
            }
            else
            {
                sliderDelayTime.Value = sliderDelayTime.Minimum;
            }
        }

        private void ButtonDelayTimeAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (sliderDelayTime.Value <= sliderDelayTime.Maximum - _defaultDelayIncr)
            {
                sliderDelayTime.Value += _defaultDelayIncr;
            }
            else
            {
                sliderDelayTime.Value = sliderDelayTime.Maximum;
            }
        }

        private void ButtonResetDefault_OnClick(object sender, RoutedEventArgs e)
        {
            sliderDelayTime.Value = _defaultDelayTime;
        }
    }
}

using System.IO;
using System.Resources;
using System.Threading;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.SquareOffProLoader;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigureSquareOff.xaml
    /// </summary>
    public partial class WinConfigureSquareOff : Window
    {
        private readonly string _fileName;
        private readonly EChessBoardConfiguration _eChessBoardConfiguration;
        private SquareOffProLoader _loader;
        private const int _defaultScanTime = 250;
        private const int _defaultScanIncr = 50;
        private readonly ResourceManager _rm;

        public WinConfigureSquareOff(Configuration configuration)
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
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _eChessBoardConfiguration.ScanTime = (int)sliderScanTime.Value;
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

        private void SliderScan_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetScanText();
        }

        private void ButtonResetScan_OnClick(object sender, RoutedEventArgs e)
        {
            sliderScanTime.Value = _defaultScanTime;
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_loader == null)
            {
                _loader = new SquareOffProLoader();
            }
            else
            {
                _loader.SetAllLedsOff(false);
                Thread.Sleep(1000);
                _loader.Close();
                _loader = null;
            }
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            if (_loader != null)
            {
                _loader.AdditionalInformation(textBoxCommand.Text);
            }
        }
    }
}
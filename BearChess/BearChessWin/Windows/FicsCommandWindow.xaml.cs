using System.ComponentModel;
using System.Windows;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für FicsCommandWindow.xaml
    /// </summary>
    public partial class FicsCommandWindow : Window
    {
        private readonly Configuration _configuration;
        private readonly int _commandIndex;

        public FicsCommandWindow(Configuration configuration, int commandIndex)
        {
            _configuration = configuration;
            _commandIndex = commandIndex;
            InitializeComponent();
            Top = _configuration.GetWinDoubleValue("FicsCommandWindowTop", Configuration.WinScreenInfo.Top,
                                                   SystemParameters.VirtualScreenHeight,
                                                   SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("FicsCommandWindowLeft", Configuration.WinScreenInfo.Left,
                                                    SystemParameters.VirtualScreenHeight,
                                                    SystemParameters.VirtualScreenWidth);
            Description =  _configuration.GetConfigValue($"FicsCommand{commandIndex}_Description", string.Empty);
            FicsCommand =  _configuration.GetConfigValue($"FicsCommand{commandIndex}_Command", string.Empty);
        }

        public string Description
        {
            get => textBlockDescription.Text;
            set => textBlockDescription.Text = value;
        }

        public string FicsCommand
        {
            get => textBlockCommand.Text;
            set => textBlockCommand.Text = value;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FicsCommand))
            {
                _configuration.DeleteConfigValue($"FicsCommand{_commandIndex}_Description");
                _configuration.DeleteConfigValue($"FicsCommand{_commandIndex}_Command");
            }
            else
            {
                _configuration.SetConfigValue($"FicsCommand{_commandIndex}_Description", Description);
                _configuration.SetConfigValue($"FicsCommand{_commandIndex}_Command", FicsCommand);
            }

            DialogResult = true;
        }


        private void FicsCommandWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("FicsCommandWindowTop", Top);
            _configuration.SetDoubleValue("FicsCommandWindowLeft", Left);
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            Description = string.Empty;
            FicsCommand = string.Empty;
        }
    }
}

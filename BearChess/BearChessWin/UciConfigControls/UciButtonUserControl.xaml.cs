using System.Windows;
using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für UciButtonUserControl.xaml
    /// </summary>
    public partial class UciButtonUserControl : UserControl, IUciConfigUserControl
    {
        private readonly string _message;
        private readonly Window _parent;
        private  MessageWindow _infoWindow;

        public UciConfigValue ConfigValue { get; }

        public UciButtonUserControl()
        {
            InitializeComponent();
        }


        public UciButtonUserControl(string message, Window parent) : this()
        {
            _message = message;
            _parent = parent;
            ConfigValue = new UciConfigValue();
            _infoWindow = null;

        }


        public void ResetToDefault()
        {
            //
        }

        private void ButtonValue_OnClick(object sender, RoutedEventArgs e)
        {
            if (_infoWindow!=null)
            {
                return;
            }
            _infoWindow = new MessageWindow(_message)
            {
                Owner = _parent
            };
            _infoWindow.Closed += _infoWindow_Closed;
            _infoWindow.Show();
        }

        private void _infoWindow_Closed(object sender, System.EventArgs e)
        {
            _infoWindow = null;
        }

        private void UciButtonUserControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _infoWindow?.Close();
        }
    }
}

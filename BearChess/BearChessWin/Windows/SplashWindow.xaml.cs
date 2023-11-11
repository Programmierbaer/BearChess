using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
   

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private readonly Dictionary<string, SplashProgressControl> _allControls;

        public delegate void SplashProgressControlEventHandler(object sender, SplashProgressControlEventArgs args);

        public event SplashProgressControlEventHandler OnCancelClick;

        public bool Cancel { get; private set; }

        public SplashWindow()
        {
            InitializeComponent();
            Cancel = false;
            CancelButton.Visibility = Visibility.Visible;
            SubCancelLabel.Visibility = Visibility.Hidden;
       
        }

        public SplashWindow(SplashProgressControlContent[] allContents, bool showCancel) : this()
        {
            StackPanelProgressControl.Children.Clear();

            _allControls = new Dictionary<string, SplashProgressControl>();
            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;
            SubCancelLabel.Visibility = showCancel ? Visibility.Hidden : Visibility.Collapsed;
            foreach (SplashProgressControlContent controlContent in allContents.OrderBy(c => c.Identifier))
            {
                SplashProgressControl progressControl = new SplashProgressControl(controlContent.ShowCancel);
                progressControl.OnCancelClick += SplashProgressControl_OnCancelClick;
                progressControl.SetContent(controlContent);
                StackPanelProgressControl.Children.Add(progressControl);
                _allControls[controlContent.Identifier] = progressControl;
            }

            if (_allControls.Count > 0)
            {
                switch (_allControls.Count)
                {
                    case 0: break;
                    case 1:
                    case 2:
                        {
                            Height += _allControls.First().Value.Height * _allControls.Count;
                            break;
                        }
                    default:
                        {
                            Height += _allControls.First().Value.Height * 2;
                            break;
                        }
                }
            }

            SizeToContent = SizeToContent.WidthAndHeight;
        }

        public void SetStartupLocation(WindowStartupLocation startupLocation)
        {
            WindowStartupLocation = startupLocation;
        }
        public void SetStartupLocation(double left, double top)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = left;
            Top = top;
        }


        public void SetContent(SplashProgressControlContent[] allContents)
        {
            foreach (SplashProgressControlContent splashProgressControlContent in allContents)
            {
                if (_allControls.ContainsKey(splashProgressControlContent.Identifier))
                {
                    _allControls[splashProgressControlContent.Identifier].SetContent(splashProgressControlContent);
                }
            }
        }


        public void SetValue(string id, double value)
        {
            if (!string.IsNullOrWhiteSpace(id) && _allControls.ContainsKey(id))
            {
                _allControls[id].SetValue(value);
            }
        }

        /// <summary>
        /// Aktualisiert den Anzeigetext <paramref name="titel"/> und aktuellen Wert <paramref name="value"/> für den Content mit der Identifikation <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Identifikation des Contents</param>
        /// <param name="titel">Aktueller Anzeigetext</param>
        /// <param name="value">Aktueller Wert</param>
        public void SetValue(string id, string titel, double value)
        {
            if (!string.IsNullOrWhiteSpace(id) && _allControls.ContainsKey(id))
            {
                _allControls[id].SetValue(titel, value);
            }
        }


        /// <summary>
        /// Verhindert die Anzeige des x-Buttons zum Schließen des Fensters
        /// </summary>
        public void HideCloseButton()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        #region private

        private void SplashProgressControl_OnCancelClick(object sender, SplashProgressControlEventArgs args)
        {
            OnCancelClick?.Invoke(this, args);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            CancelButton.IsEnabled = false;
            SubCancelLabel.Visibility = Visibility.Visible;
            foreach (object child in StackPanelProgressControl.Children)
            {
                SplashProgressControl splashProgressControl = child as SplashProgressControl;
                splashProgressControl?.DoCancel();
            }
        }

        #endregion
    }
}

using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für SerialPortTestWindow.xaml
    /// </summary>
    public partial class SerialPortTestWindow : Window
    {

        private IComPort _comPort;
        private Thread _readingThread;


        public SerialPortTestWindow()
        {
            InitializeComponent();
            _readingThread = new Thread(ReadingFromPort) { IsBackground = true };
            _readingThread.Start();
        }

        private void ReadingFromPort()
        {
            while (true)
            {
                var readLine = _comPort?.ReadLine();
                if (!string.IsNullOrWhiteSpace(readLine))
                {
                    Dispatcher?.Invoke(() =>
                    {
                        listBoxLog.Items.Add(readLine); ;
                        listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
                    });
                }
                Thread.Sleep(50);
            }   
        }


        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            listBoxLog.Items.Add(textBoxSend.Text);
            listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
            _comPort?.Write(textBoxSend.Text+ Environment.NewLine);
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_comPort == null)
            {
                _comPort = new SerialComPortEventBased(textBoxPort.Text, int.Parse(textBoxBaud.Text), Parity.None, 8,
                                                       StopBits.One);
                _comPort.Open();
                buttonConnect.Content = "Disconnect";

            }
            else
            {
                _comPort.Close();
                _comPort = null;
                buttonConnect.Content = "Connect";
            }
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            listBoxLog.Items.Clear();
        }
    }
}

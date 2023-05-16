using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
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
        private StringBuilder _allLines = new StringBuilder();


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
                        _allLines.AppendLine(readLine);
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
                _comPort = new SerialComPortStreamBased(textBoxPort.Text, int.Parse(textBoxBaud.Text), Parity.None, 8,
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
            _allLines.Clear();
        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_allLines.ToString());
        }

        private void CheckBoxRTS_OnChecked(object sender, RoutedEventArgs e)
        {
            _comPort.RTS = true;
        }

        private void CheckBoxRTS_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _comPort.RTS = false;
        }

        private void CheckBoxDTR_OnChecked(object sender, RoutedEventArgs e)
        {
            _comPort.DTR = true;
        }

        private void CheckBoxDTR_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _comPort.DTR = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using WebSocketSharp;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using WebSocket = WebSocketSharp.WebSocket;
using WebSocketState = System.Net.WebSockets.WebSocketState;

namespace www.SoLaNoSoft.com.BearChessWin
{

    // https://github.com/Marfusios/websocket-client
    /// <summary>
    /// Interaktionslogik für SerialPortTestWindow.xaml
    /// </summary>
    public partial class SerialPortTestWindow : Window
    {
        private WebSocket webSocket;
        private readonly string[] _ledUpperLeftToField =
      {
            "", "", "", "", "", "", "", "", "",
            "", "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8",
            "", "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8",
            "", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8",
            "", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8",
            "", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8",
            "", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8",
            "", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8",
            "", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8"
        };

        private readonly string[] _ledUpperLeftToFieldInvert =
        {

            "", "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1",
            "", "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1",
            "", "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1",
            "", "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1",
            "", "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1",
            "", "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1",
            "", "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1",
            "", "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1",
            "", "", "", "", "", "", "", "", ""
        };

        private readonly string[] _ledUpperRightToField =
        {
            "", "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8",
            "", "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8",
            "", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8",
            "", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8",
            "", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8",
            "", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8",
            "", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8",
            "", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8",
            "", "", "", "", "", "", "", "", ""
        };

        private readonly string[] _ledUpperRightToFieldInvert =
        {
            "", "", "", "", "", "", "", "", "",
            "", "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1",
            "", "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1",
            "", "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1",
            "", "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1",
            "", "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1",
            "", "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1",
            "", "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1",
            "", "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1",
        };

        private readonly string[] _ledLowerRightToField =
        {
            "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8", "",
            "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8", "",
            "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "",
            "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "",
            "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "",
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "",
            "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "",
            "", "", "", "", "", "", "", "", ""
        };

        private readonly string[] _ledLowerRightToFieldInvert =
        {
            "", "", "", "", "", "", "", "", "",
            "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1", "",
            "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1", "",
            "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1", "",
            "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1", "",
            "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1", "",
            "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1", "",
            "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1", "",
            "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1", ""
        };

        private readonly string[] _ledLowerLeftToField =
        {
            "", "", "", "", "", "", "", "", "",
            "h1", "h2", "h3", "h4", "h5", "h6", "h7", "h8", "",
            "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8", "",
            "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "",
            "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "",
            "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "",
            "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "",
            "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "",
            "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "",
            "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", ""
        };

        private readonly string[] _ledLowerLeftToFieldInvert =
        {
            "a8", "a7", "a6", "a5", "a4", "a3", "a2", "a1", "",
            "b8", "b7", "b6", "b5", "b4", "b3", "b2", "b1", "",
            "c8", "c7", "c6", "c5", "c4", "c3", "c2", "c1", "",
            "d8", "d7", "d6", "d5", "d4", "d3", "d2", "d1", "",
            "e8", "e7", "e6", "e5", "e4", "e3", "e2", "e1", "",
            "f8", "f7", "f6", "f5", "f4", "f3", "f2", "f1", "",
            "g8", "g7", "g6", "g5", "g4", "g3", "g2", "g1", "",
            "h8", "h7", "h6", "h5", "h4", "h3", "h2", "h1", "",
            "", "", "", "", "", "", "", "", ""
        };

        private IComPort _comPort;
        private Thread _readingThread;
        private StringBuilder _allLines = new StringBuilder();
        private ClientPipe _clientPipe;
        private string _currentPosition = string.Empty;
        private readonly ISpeech _synthesizer;

        public SerialPortTestWindow()
        {
            InitializeComponent();
            _synthesizer = BearChessSpeech.Instance;
//            _readingThread = new Thread(ReadingFromPort) { IsBackground = true };
//            _readingThread.Start();
        }

        private void ReadingFromPort()
        {
            while (true)
            {
                string readLine = string.Empty;
                var readByteArray = _comPort?.ReadByteArray();
                if (readByteArray != null)
                {
                    for (int i = 0; i < readByteArray.Length; i++)
                    {
                        readLine += ConvertFromRead(readByteArray[i]);
                    }

                    //int? readByte = _comPort?.ReadByte();
                    //if (readByte != null)
                    {
                        //string readLine = ConvertFromRead(readByte.Value);
                        if (!string.IsNullOrWhiteSpace(readLine))
                        {
                            Dispatcher?.Invoke(() =>
                            {
                                listBoxLog.Items.Add($"{readLine}");
                                ;
                                listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
                                _allLines.AppendLine($"{DateTime.UtcNow:o} {readLine}");
                            });
                        }
                    }
                }

                Thread.Sleep(50);
            }   
        }

        private void ReadingFromPortSingle()
        {
            string readLine = string.Empty;
            while (true)
            {
             
                var readByteArray = _comPort?.ReadByteArray();
                if (readByteArray != null)
                {
                    for (int i = 0; i < readByteArray.Length; i++)
                    {
                        readLine += ConvertFromRead(readByteArray[i]);
                    }

                    //int? readByte = _comPort?.ReadByte();
                    //if (readByte != null && readByte!=-1)
                    {
                     //   readLine = ConvertFromRead(readByte.Value);
                        if (!string.IsNullOrWhiteSpace(readLine))
                        {
                            Dispatcher?.Invoke(() =>
                            {
                                listBoxLog.Items.Add($"{readLine}");
                                ;
                                listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
                                _allLines.AppendLine($"{DateTime.UtcNow:o} {readLine}");
                                readLine = string.Empty;
                            });
                        }
                    }
                }

                Thread.Sleep(50);
            }
        }

        private string ConvertFromRead(int data)
        {
            // var i = data & 127;
           // return Encoding.ASCII.GetString(new[] { (byte)data });
            var i = data & 127;
            return Encoding.ASCII.GetString(new[] { (byte)i });
        }

        private byte[] ConvertToSend(string data)
        {
            var addBlockCrc = CRCConversions.AddBlockCrc(data);
            byte[] addOddPars = new byte[addBlockCrc.Length];
            for (int i = 0; i < addBlockCrc.Length; i++)
            {
                addOddPars[i] = CRCConversions.AddOddPar(addBlockCrc[i].ToString());
            }

            return addOddPars;
        }


        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            listBoxLog.Items.Add(textBoxSend.Text);
            listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
            if (checkBoxChesstimation.IsChecked.HasValue && checkBoxChesstimation.IsChecked.Value)
            {
                var convertToSend = ConvertToSend(textBoxSend.Text);
                _comPort?.Write(convertToSend, 0, convertToSend.Length);
            }
            else
            {
                _clientPipe?.WriteString(textBoxSend.Text);
                //_clientPipe?.WriteBytes(Encoding.ASCII.GetBytes(textBoxSend.Text));
                _comPort?.Write(textBoxSend.Text);
                webSocket?.Send(textBoxSend.Text);

            }
            // _comPort?.Write(textBoxSend.Text);
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_comPort == null)
            {
                //  _comPort = new SerialComPortStreamBased(textBoxPort.Text, int.Parse(textBoxBaud.Text), Parity.None, 8,
                //                                         StopBits.One);

                //_comPort = new SerialComportForByteArray(textBoxPort.Text, 115200, Parity.None, 8, StopBits.One, null)
                //{ ReadTimeout = 1000, WriteTimeout = 1000 };
                if (checkBoxChesstimation.IsChecked.HasValue && checkBoxChesstimation.IsChecked.Value)
                {
                    _comPort = new SerialComportForByteArray(textBoxPort.Text, 38400, Parity.Odd, 7, StopBits.One, null)
                               { ReadTimeout = 1000, WriteTimeout = 1000 };
                }
                else
                {
                    _comPort = new SerialComportForByteArray(textBoxPort.Text, 115200, Parity.None, 8, StopBits.One, null)
                    { ReadTimeout = 1000, WriteTimeout = 1000 };
                }

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
            ClipboardHelper.SetText(_allLines.ToString());
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

        private string GetLedForFields(string[] fieldNames, bool thinking)
        {
            var codes = string.Empty;
            var toCode = "FF";
            var fromCode = "FF";
            
            for (var i = 0; i < 81; i++)
            {
                var code = "00";
                for (var j = 0; j < fieldNames.Length; j++)
                {
                    code = "00";
                    if (true)
                    {
                        if (_ledUpperRightToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;

                            break;
                        }


                        if (_ledUpperLeftToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;

                            break;
                        }

                        if (_ledLowerRightToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;

                            break;
                        }

                        if (_ledLowerLeftToField[i].Contains(fieldNames[j].ToLower()))
                        {
                            code = j == 0 ? fromCode : toCode;

                            break;
                        }
                    }
                 

                }

                codes += code;
            }

            return codes.Replace("FFFFFFFF","FFF00FFF");
        }

        private void ButtonGet_OnClick(object sender, RoutedEventArgs e)
        {
            _synthesizer.SpeakAsync(textBoxFrom.Text,true);
           //textBoxSend.Text = "L22"+ GetLedForFields(textBoxFrom.Text.Split(",".ToCharArray()), false);
        }

        public  async Task Connect(string uri)
        {
            ClientWebSocket webSocket = null;

            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                await Task.WhenAll(Receive(webSocket), Send(webSocket));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
             
            }
        }

        private  async Task Send(ClientWebSocket webSocket)
        {
            var random = new Random();
            byte[] buffer = new byte[256];
            bool already = false;
            while (webSocket.State == WebSocketState.Open)
            {
                if (!already)
                {
                    buffer = Encoding.UTF8.GetBytes("fen");

                    await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, false,
                        CancellationToken.None);
                    already = true;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        private  async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[64];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        string res = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        listBoxLog.Items.Add($"{res}");
                        ;
                        listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));

                    });
                }
            }
        }

        private async void buttonConnectIP_Click(object sender, RoutedEventArgs e)
        {
            //var url = new Uri(textBoxAddress.Text);
            ClientWebSocket _clientWebSocket;
            try
            {
                _clientWebSocket = new ClientWebSocket();
                //await _clientWebSocket.ConnectAsync(new Uri($"ws://{textBoxAddress.Text}"), CancellationToken.None);
                //await Task.WhenAll(Receive(_clientWebSocket), Send(_clientWebSocket));

                webSocket = new WebSocket($"ws://{textBoxAddress.Text}");
                webSocket.OnMessage += WebSocket_OnMessage;
                webSocket.OnClose += WebSocket_OnClose;
                webSocket.OnOpen += WebSocket_OnOpen;
                webSocket.Connect();
                webSocket.Send("fen");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }





        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                listBoxLog.Items.Add($"Opened");
                ;
                listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
                _allLines.AppendLine($"{DateTime.UtcNow:o} {e}");
            });
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                listBoxLog.Items.Add($"Closed: {e.Reason}");
                ;
                listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
                _allLines.AppendLine($"{DateTime.UtcNow:o} {e.Reason}");
            });
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                listBoxLog.Items.Add($"Message: {e.Data}");
                ;
                listBoxLog.ScrollIntoView(listBoxLog.Items.GetItemAt(listBoxLog.Items.Count - 1));
                _allLines.AppendLine($"{DateTime.UtcNow:o} {e.Data}");
            });
        }

        private void buttonDisConnectIP_Click(object sender, RoutedEventArgs e)
        {
            webSocket.Close();
        }

        private void buttonConnectPipe_Click(object sender, RoutedEventArgs e)
        {

            _clientPipe = new ClientPipe(".", "BearChessPipe", p => p.StartStringReaderAsync());
         
            try
            {
                _clientPipe.Connect(500);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonDisConnectPipe_Click(object sender, RoutedEventArgs e)
        {
            if (_clientPipe != null)
            {
                _clientPipe.Close();
                _clientPipe = null;
            }
        }
    }
}

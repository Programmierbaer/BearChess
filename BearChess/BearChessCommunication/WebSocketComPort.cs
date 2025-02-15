using System;
using System.Collections.Concurrent;
using WebSocketSharp;
using WebSocket = WebSocketSharp.WebSocket;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class WebSocketComPort : IComPort
    {
        public string PortName { get; }
        public string DeviceName => string.Empty;
        public string Baud { get; }
        private WebSocket _webSocket;
       
        private readonly ConcurrentQueue<string> _allLines = new ConcurrentQueue<string>();

        public WebSocketComPort(string webSocketAddr)
        {
            if (!(webSocketAddr.IndexOf(":")>4))
            {
                webSocketAddr += ":8888";
            }
            PortName = webSocketAddr;
            Baud = string.Empty;
        }

        public void Open()
        {

            _webSocket = new WebSocket(PortName);
            _webSocket.OnMessage += WebSocket_OnMessage;
            _webSocket.OnClose += WebSocket_OnClose;
            _webSocket.OnError += WebSocket_OnError;
            _webSocket.Connect();

        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
           Close();
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            //
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            _allLines.Enqueue(e.Data);
        }

        public void Close()
        {
            if (_webSocket != null)
            {
              _webSocket.Close();
              _webSocket = null;
            }
        }

        public bool IsOpen => _webSocket != null && _webSocket.IsAlive && _webSocket.ReadyState == WebSocketState.Open;

        public string ReadLine()
        {
            if (_allLines.TryDequeue(out string line))
            {
                return line;
            }
            return string.Empty;
        }

        public int ReadByte()
        {
            return 0;
        }

        public byte[] ReadByteArray()
        {
            return Array.Empty<byte>();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _webSocket.Send(buffer);

        }

        public void Write(string command)
        {
            _webSocket.Send(command);
        }

        public void WriteLine(string command)
        {
            _webSocket.Send(command + Environment.NewLine);
        }

        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public void ClearBuffer()
        {
            while (_allLines.TryDequeue(out _)) ;
        }

        public string ReadBattery()
        {
            return string.Empty;
        }

        public bool RTS { get; set; }
        public bool DTR { get; set; }
    }
}
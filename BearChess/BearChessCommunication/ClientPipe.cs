using System;
using System.IO.Pipes;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public class ClientPipe : BasicPipe
    {
        protected NamedPipeClientStream clientPipeStream;

        public ClientPipe(string serverName, string pipeName, Action<BasicPipe> asyncReaderStart)
        {
            this.asyncReaderStart = asyncReaderStart;
            clientPipeStream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            pipeStream = clientPipeStream;
        }

        public void Connect()
        {
            clientPipeStream.Connect();
            asyncReaderStart(this);
        }

        public void Connect(int timeOut)
        {
            clientPipeStream.Connect(timeOut);
            asyncReaderStart(this);
        }
    }
}

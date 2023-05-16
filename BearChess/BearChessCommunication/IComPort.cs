namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication

{
    public interface IComPort
    {
        string PortName { get; }
        string Baud { get; }
        void Open();
        void Close();
        bool IsOpen { get; }
        string ReadLine();
        int ReadByte();
        byte[] ReadByteArray();
        void Write(byte[] buffer, int offset, int count);
        void Write(string command);
        void WriteLine(string command);
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
        void ClearBuffer();

        bool RTS { get; set; }
        bool DTR { get; set; }
    }
}
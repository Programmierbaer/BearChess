namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    public interface IComPort
    {
        string PortName { get; }
        void Open();
        void Close();
        bool IsOpen { get; }
        string ReadLine();
        int ReadByte();
        void Write(byte[] buffer, int offset, int count);
        int ReadTimeout { get; set; }
    }
}
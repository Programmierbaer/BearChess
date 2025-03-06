namespace www.SoLaNoSoft.com.BearChessBase
{
    public class BearChessServerMessage
    {
        public string Address
        {
            get; set;
        }
        public string ActionCode
        {
            get; set;
        }
        public string Message
        {
            get; set;
        }

        public string Ack
        {
            get; set;
        }

        public string Color
        {
            get; set;
        }

        public BearChessServerMessage()
        {
            Address = string.Empty;
            ActionCode = string.Empty;
            Message = string.Empty;
            Ack = string.Empty;
            Color = string.Empty;
        }
    }
}
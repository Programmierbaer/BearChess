namespace www.SoLaNoSoft.com.BearChessBase
{

    public class BearChessClientInformation
    {
        public string Address
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }

        public BearChessClientInformation()
        {
            Address = string.Empty;
            Name = string.Empty;
        }

        public override string ToString()
        {
            return $"{Name} ({Address})";
        }
    }


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

        public BearChessServerMessage()
        {
            Address = string.Empty;
            ActionCode = string.Empty;
            Message = string.Empty;
            Ack = string.Empty;
        }
    }
}
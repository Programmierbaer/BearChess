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
}
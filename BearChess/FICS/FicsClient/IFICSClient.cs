namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public interface IFICSClient : ITelnetClient
    {
        bool AsGuest { get; }
        string Username { get; set; }
    }
}
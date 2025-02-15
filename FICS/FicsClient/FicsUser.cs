namespace www.SoLaNoSoft.com.BearChess.FicsClient
{
    public class FicsUser
    {
        public string UserName { get; set; }
        public string StandardElo { get; set; }
        public string BlitzElo { get; set; }
        public string LightningElo { get; set; }
        public bool UnregisteredUser { get; set; }
        public bool ComputerUser { get; set; }
        public bool OpenForGames { get; set; }
        public bool OnlyUnratedGames { get; set; }

        public override string ToString()
        {
            return UserName; 
        }
    }
}

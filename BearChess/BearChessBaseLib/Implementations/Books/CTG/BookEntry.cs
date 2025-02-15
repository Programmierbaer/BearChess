namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    public class BookEntry
    {
        public Move Move { get; set; }
        public float Weight { get; set; }
        public float Score { get; set; }
        public int Recommendations { get; set; }
        public int Annotation { get; set; }
        public int Commentary { get; set; }

        public int NumberOfGames { get; set; }
        public int NumberOfWinsForWhite { get; set; }
        public int NumberOfWinsForBlack { get; set; }
        public int NumberOfDraws { get; set; }



        public BookEntry(Move move)
        {
            Move = move;
            Weight = 1;
            Recommendations = 0;
            Score = 0;
            Annotation = 0;
            NumberOfGames = 0;
            NumberOfWinsForWhite = 0;
            NumberOfWinsForBlack = 0;
            NumberOfDraws = 0;
            Commentary = 0;
        }

        public override string ToString()
        {
            return StringHelper.MoveToUCIString(Move) + " (" + Weight + ")";
        }
    }
}
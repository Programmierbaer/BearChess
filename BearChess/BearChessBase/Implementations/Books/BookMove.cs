namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class BookMove
    {
        public string FromField { get; }
        public string ToField { get; set; }
        public uint Weight { get; set; }
        public int PlyCount { get; }
        public uint NextMovePointer { get; }
        public uint NoOfGames { get; }
        public uint NoOfWins { get; }
        public uint NoOfLoss { get; }
        public uint NoOfDraws { get; }
        public string FenPosition { get; set; }
        public bool EmptyMove { get; private set; }

        public BookMove(string fromField, string toField, ushort weight)
        {
            FromField = fromField;
            ToField = toField;
            Weight = weight;
            PlyCount = 0;
            NextMovePointer = 0;
            NoOfGames = 0;
            NoOfWins = 0;
            NoOfLoss = 0;
            NoOfDraws = 0;
            FenPosition = string.Empty;
            EmptyMove = string.IsNullOrWhiteSpace(fromField) && string.IsNullOrWhiteSpace(toField);
        }

        public BookMove(string fromField, string toField, ushort weight, int plyCount, uint nextMovePointer, uint noOfGames, uint noOfWins, uint noOfLoss)
        {
            FromField = fromField;
            ToField = toField;
            PlyCount = plyCount;
            NextMovePointer = nextMovePointer;
            NoOfGames = noOfGames;
            NoOfWins = noOfWins;
            NoOfLoss = noOfLoss;
            NoOfDraws = NoOfGames - NoOfWins - NoOfLoss;
            FenPosition = string.Empty;
            EmptyMove = string.IsNullOrWhiteSpace(fromField) && string.IsNullOrWhiteSpace(toField);
            if (noOfWins == 0)
            {
                Weight = 0;
            }
            else
            {
                if (weight == 0 && noOfWins > 1)
                {
                    weight = 1;
                }
                decimal zahl = (decimal) 100 / (decimal) NoOfGames * (decimal) NoOfWins;
                decimal zahl2 = (decimal) 100 / (decimal) NoOfGames * (decimal) NoOfDraws / (decimal) 2;
                uint zahl3 = (uint) zahl + (uint) zahl2;
                Weight = zahl3 * weight;
            }
        }
    }
}
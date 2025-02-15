namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public class PolyglotBookMove : IBookMoveBase
    {
        public string FromField { get; set; }
        public string ToField { get; set; }
        public uint Weight { get; set; }
        public bool EmptyMove { get; set; }

        public string FenPosition { get; set; }
        public uint NoOfGames { get; }
        public uint NoOfWins { get; }
        public uint NoOfLoss { get; }
        public int Recommendations { get; set; }
        public string Annotation { get; set; }
        public string Commentary { get; set; }
        public string MoveText { get; set; }
        public uint NoOfDraws { get; }

        public PolyglotBookMove(string fromField, string toField, ushort weight)
        {
            FromField = fromField;
            ToField = toField;
            Weight = weight;
            NoOfGames = 0;
            NoOfWins = 0;
            NoOfLoss = 0;
            NoOfDraws = 0;
            EmptyMove = string.IsNullOrWhiteSpace(fromField) && string.IsNullOrWhiteSpace(toField);
            MoveText = $"{fromField}{toField}";
            Recommendations = 0;
            Annotation = string.Empty;
            Commentary = string.Empty;
        }

     
    }
}
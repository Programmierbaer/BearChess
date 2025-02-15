using System;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    public class CTGBookMove : IBookMoveBase
    {
        public string FromField { get; set; }
        public string ToField { get; set; }
        public uint Weight { get; set; }
        public bool EmptyMove { get; set; }

        public string FenPosition { get; set; }

        public uint NoOfGames { get; }
        public uint NoOfWins { get; }
        public uint NoOfLoss { get; }
        public uint NoOfDraws { get; }
        public int Recommendations { get; set; }
        public string Annotation { get; set; }
        public string Commentary { get; set; }


        public string MoveText { get; set; }


        public CTGBookMove(string fromField, string toField, float weight, int noOfGames, int noOfWins, int noOfLoss, int noOfDraws)
        {
            FromField = fromField;
            ToField = toField;
            Weight = (uint)Math.Round(weight);
            NoOfGames = (uint)noOfGames;
            NoOfWins = (uint)noOfWins;
            NoOfLoss = (uint)noOfLoss;
            NoOfDraws = (uint)noOfDraws;
            EmptyMove = string.IsNullOrWhiteSpace(fromField) && string.IsNullOrWhiteSpace(toField);
            MoveText = $"{fromField}{toField}";
            Recommendations = 0;
            Annotation = string.Empty;
            Commentary = string.Empty;
        }


    }
}
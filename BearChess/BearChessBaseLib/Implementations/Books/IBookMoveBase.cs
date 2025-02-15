namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public interface IBookMoveBase
    {
        string FromField { get; set; }
        string ToField { get; set; }
        uint Weight { get; set; }
        bool EmptyMove { get;  set; }

        string FenPosition { get; set; }

        uint NoOfGames { get; }
        uint NoOfWins { get; }
        uint NoOfLoss { get; }
        int Recommendations { get; set; }
        string Annotation { get; set; }
        string Commentary { get; set; }


        string MoveText { get; set; }
    }
}
namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    public class PgnConfiguration
    {
        private bool _includeComment;
        private bool _includeMoveTime;
        private bool _includeEvaluation;
        private bool _includeSymbols;
        public bool PurePgn { get; set; }

        public bool IncludeComment 
        {
            get => _includeComment && !PurePgn;
            set => _includeComment = value;
        }

        public bool IncludeMoveTime
        {
            get => _includeMoveTime && !PurePgn;
            set => _includeMoveTime = value;
        }

        public bool IncludeEvaluation
        {
            get => _includeEvaluation && !PurePgn;
            set => _includeEvaluation = value;
        }

        public bool IncludeSymbols
        {
            get => _includeSymbols && !PurePgn;
            set => _includeSymbols = value;
        }


        public PgnConfiguration()
        {

        }
        
    }
}
namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface IInternalChessBoard
    {
        /// <summary>
        /// Set the board position given by <paramref name="fenPosition"/>.
        /// </summary>
        void SetPosition(string fenPosition);

        /// <summary>
        /// Make the move from <paramref name="fromField"/> to <paramref name="toField"/> and the promoted piece,
        /// </summary>
        void MakeMove(string fromField, string toField, string promote);

        /// <summary>
        /// Returns the move based on the current position related to the new position given by <paramref name="newFenPosition"/>.
        /// If <paramref name="ignoreRule"/> is false and the detected move is not valid, the result is "".
        /// </summary>
        string GetMove(string newFenPosition, bool ignoreRule);

        string GetChangedFigure(string oldFenPosition, string newFenPosition);

        /// <summary>
        /// Returns the current position
        /// </summary>
        string GetPosition();

        /// <summary>
        /// Returns figure FEN code on the field number <paramref name="field"/> .
        /// </summary>
        string GetFigureOnField(int field);

        /// <summary>
        /// Initiates a new game
        /// </summary>
        void NewGame();

        /// <summary>
        /// Return a move to satisfy the requirement of the UCI command "stop".
        /// </summary>
        string GetBestMove();

        /// <summary>
        /// Indicates if the given <paramref name="fenPosition"/> equals the chess start position
        /// </summary>
        /// <param name="fenPosition"></param>
        bool IsBasePosition(string fenPosition);
    }
}
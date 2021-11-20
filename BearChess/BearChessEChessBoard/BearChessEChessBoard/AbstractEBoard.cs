using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard

{
    public abstract class AbstractEBoard : IEBoard
    {

        public static string BlackPawnFen = "p";
        public static string BlackRookFen = "r";
        public static string BlackKnightFen = "n";
        public static string BlackBishopFen = "b";
        public static string BlackQueenFen = "q";
        public static string BlackKingFen = "k";
        public static string WhitePawnFen = "P";
        public static string WhiteRookFen = "R";
        public static string WhiteKnightFen = "N";
        public static string WhiteBishopFen = "B";
        public static string WhiteQueenFen = "Q";
        public static string WhiteKingFen = "K";


        protected ISerialCommunication _serialCommunication;
        protected object _locker = new object();
        protected ILogging _logger;
        protected bool _playWithWhite = true;
        protected bool _isFirstInstance;

        public bool IsCalibrated { get; protected set; }
        public bool PiecesAreOnBasePosition { get; protected set; }
        public bool IsConnected { get; protected set; }

        public string UnknownPieceCode => "unknown";

        public void SetLedForFields(string fromFieldName, string toFieldName)
        {
            SetLedForFields(new[] { fromFieldName, toFieldName });
        }

        public abstract bool CheckComPort(string portName);

        public abstract void SetLedForFields(string[] fieldNames);

        public abstract void SetLastLeds();

        public abstract void SetAllLedsOff();

        public abstract void SetAllLedsOn();

        public abstract void DimLeds(bool dimLeds);

        public abstract void DimLeds(int level);

        public abstract void FlashSync(bool flashSync);

        public abstract void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight);

        public abstract void Calibrate();

        public abstract DataFromBoard GetPiecesFen();

        public void PlayWithWhite()
        {
            _playWithWhite = true;
        }

        public void PlayWithBlack()
        {
            _playWithWhite = false;
        }

        public bool PlayingWithWhite => _playWithWhite;

        public void NewGame()
        {
            _logger?.LogDebug("New game");
            EnsureConnection();
        }

        public void SetComPort(string portName)
        {
            if (_serialCommunication.SetComPort(portName))
            {
                // Initiate Reconnect
                IsConnected = false;
            }
        }

        public string GetCurrentCOMPort()
        {
            return _serialCommunication.CurrentComPort;
        }

        public void SendCommand(string anyCommand)
        {
            lock (_locker)
            {
                _serialCommunication?.Send(anyCommand);
            }
        }

        protected bool EnsureConnection()
        {
            if (IsConnected)
            {
                return true;
            }

            if (!IsConnected && _serialCommunication.Connect())
            {
                _serialCommunication.StartCommunication();
                IsConnected = true;
                return true;
            }
            _logger?.LogError("B: chess board not found");
            return false;
        }

        public void Dispose()
        {
            _serialCommunication.DisConnect();
        }
    }
}

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public interface ISerialCommunication
    {
        string CurrentComPort { get; }

        /// <summary>
        /// Indicates if the communication is active to send and receive data
        /// </summary>
        bool IsCommunicating { get; }

        /// <summary>
        /// Returns read data from board
        /// </summary>
        DataFromBoard GetFromBoard();

        /// <summary>
        /// Returns read data from board
        /// </summary>
        string GetRawFromBoard(string param);

        void SendRawToBoard(string param);

        void SendRawToBoard(byte[] param);

        /// <summary>
        /// Starts communication with the board for reading and sending
        /// </summary>
        void StartCommunication();

        /// <summary>
        /// Sends <paramref name="data"/> to the board. 
        /// </summary>
        /// <param name="data">data to send</param>
        void Send(byte[] data, bool forcedSend);

        void Send(byte[] data);

        void Send(string data);

        /// <summary>
        /// Close connection to board
        /// </summary>
        void DisConnect();

        /// <summary>
        /// Close connection to board
        /// </summary>
        void DisConnectFromCheck();


        /// <summary>
        /// Stops the communication but keep connected
        /// </summary>
        void StopCommunication();

        /// <summary>
        /// Clears internal buffer of reads and data to send
        /// </summary>
        void Clear();

        void ClearToBoard();

        /// <summary>
        /// Open the connection
        /// </summary>
        /// <returns>true if successful</returns>
        bool Connect();

        /// <summary>
        /// Open the connection 
        /// </summary>
        /// <returns>true if successful</returns>
        bool CheckConnect(string comPort);

        /// <summary>
        /// Returns the data for calibrating the board
        /// </summary>
        /// <returns>Calibrated data</returns>
        string GetCalibrateData();

        /// <summary>
        /// Set the COM-Port. Returns true if the given port name differs from the current.
        /// </summary>
        /// <param name="portName">Name of the COM-Port</param>
        bool SetComPort(string portName);

        string BoardInformation { get; }
    }
}

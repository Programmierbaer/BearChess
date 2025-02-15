namespace www.SoLaNoSoft.com.BearChess.CertaboChessBoard
{
    /// <summary>
    /// Storage for calibration data
    /// </summary>
    public interface ICalibrateStorage
    {
        /// <summary>
        /// Saves calibration data <paramref name="codes"/>.
        /// </summary>
        /// <param name="codes">Board codes for base position</param>
        /// <returns>true if successful</returns>
        bool SaveCalibrationData(CalibrateData codes);

        /// <summary>
        /// Returns saved calibration data.
        /// </summary>
        CalibrateData GetCalibrationData();

        /// <summary>
        /// Returns the saved queen codes for both colors
        /// </summary>
        string GetQueensCodes(bool white);

        /// <summary>
        /// Saves calibration data <paramref name="codes"/> of one or two queens for both colors
        /// </summary>
        /// <returns>true if successful</returns>
        bool SaveQueensCodes(bool white, string codes);
    }
}

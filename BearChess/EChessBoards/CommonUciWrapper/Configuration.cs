namespace www.SoLaNoSoft.com.BearChess.CommonUciWrapper
{
    /// <summary>
    /// Configuration values for the UCI engine
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Configured COM port name. The UCI engine runs smoother if the COM port is available from the beginning.
        /// </summary>
        public string PortName { get; set; }
    }
}

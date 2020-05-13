namespace Easy_Logger.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 
        /// </summary>
        public ILoggingConfiguration Settings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerEntry"></param>
        public bool SaveToLog(ILoggerEntry loggerEntry);
    }
}

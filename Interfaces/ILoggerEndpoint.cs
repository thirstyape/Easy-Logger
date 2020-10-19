namespace Easy_Logger.Interfaces
{
    /// <summary>
    /// Defines properties and methods required to record data to a logging endpoint
    /// </summary>
    public interface ILoggerEndpoint
    {
        /// <summary>
        /// The configuration object to pass to the logger
        /// </summary>
        public ILoggingConfiguration Settings { get; set; }

        /// <summary>
        /// Saves the provided entry to the endpoint
        /// </summary>
        /// <param name="loggerEntry">The data to log</param>
        public bool SaveToLog(ILoggerEntry loggerEntry);
    }
}

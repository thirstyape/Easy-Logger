using Easy_Logger.Enums;

namespace Easy_Logger.Interfaces
{
    /// <summary>
    /// Defines properties required to configure the logging service
    /// </summary>
    public interface ILoggingConfiguration
    {
        /// <summary>
        /// Specifies whether to record to text log files
        /// </summary>
        public bool UseTextLogger { get; set; }

        /// <summary>
        /// Specifies whether to record to JSON log files
        /// </summary>
        public bool UseJsonLogger { get; set; }

        /// <summary>
        /// Specifies whether to record to a SQL log table
        /// </summary>
        public bool UseSqlLogger { get; set; }

        /// <summary>
        /// The directory to save text or JSON logs to
        /// </summary>
        public string LogDirectory { get; set; }

        /// <summary>
        /// The filename to save text-based logs to (may include date parts as follows: [Date:yyyy]_otherdetails_[Date:MM-dd])
        /// </summary>
        public string LogFilename { get; set; }

        /// <summary>
        /// Specifies whether text-based logs should store logs in dated subdirectories (ex. /logfolder/2020/05/01/logfile.txt)
        /// </summary>
        public bool UseDatedSubdirectory { get; set; }

        /// <summary>
        /// The level of dated subdirectory to use
        /// </summary>
        public DatedSubdirectoryModes DatedSubdirectoryMode { get; set; }

        /// <summary>
        /// The name of the SQL table to store log data in
        /// </summary>
        public string LogSqlTable { get; set; }

        /// <summary>
        /// The text to use to connect to the SQL database
        /// </summary>
        public string SqlConnectionString { get; set; }
    }
}

using Easy_Logger.Enums;

namespace Easy_Logger.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILoggingConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public bool UseTextLogger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UseJsonLogger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UseSqlLogger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LogDirectory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LogFilename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UseDatedSubdirectory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DatedSubdirectoryModes DatedSubdirectoryMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LogSqlTable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SqlConnectionString { get; set; }
    }
}

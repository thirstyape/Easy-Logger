using Easy_Logger.Enums;
using Easy_Logger.Interfaces;

using System;
using System.IO;

namespace Easy_Logger.Models
{
    /// <summary>
    /// Default implementation of <see cref="ILoggingConfiguration"/>
    /// </summary>
    public class LoggingConfiguration : ILoggingConfiguration
    {
        /// <inheritdoc/>
        public bool UseTextLogger { get; set; } = true;

        /// <inheritdoc/>
        public bool UseJsonLogger { get; set; }

        /// <inheritdoc/>
        public bool UseSqlLogger { get; set; }

        /// <inheritdoc/>
        public string LogDirectory { get; set; } = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Temp\\Logs");

        /// <inheritdoc/>
        public string LogFilename { get; set; } = "[Date:yyyy-MM-dd_HH]";

        /// <inheritdoc/>
        public bool UseDatedSubdirectory { get; set; } = true;

        /// <inheritdoc/>
        public DatedSubdirectoryModes DatedSubdirectoryMode { get; set; } = DatedSubdirectoryModes.Daily;

        /// <inheritdoc/>
        public string LogSqlTable { get; set; } = "ApplicationLogs";

        /// <inheritdoc/>
        public string SqlConnectionString { get; set; }
    }
}

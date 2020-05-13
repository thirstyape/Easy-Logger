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
        public bool UseTextLogger { get; set; } = true;
        public bool UseJsonLogger { get; set; }
        public bool UseSqlLogger { get; set; }
        public string LogDirectory { get; set; } = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Temp\\Logs");
        public string LogFilename { get; set; } = "[Date:yyyy-MM-dd_HH]";
        public bool UseDatedSubdirectory { get; set; } = true;
        public DatedSubdirectoryModes DatedSubdirectoryMode { get; set; } = DatedSubdirectoryModes.Daily;
        public string LogSqlTable { get; set; } = "ApplicationLogs";
        public string SqlConnectionString { get; set; }
    }
}

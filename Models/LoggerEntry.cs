using Easy_Logger.Interfaces;

using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy_Logger.Models
{
    /// <summary>
    /// Default implementation of <see cref="ILoggerEntry"/>
    /// </summary>
    public class LoggerEntry : ILoggerEntry
    {
        /// <inheritdoc/>
        [Column("LogDate")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <inheritdoc/>
        [Column("LogTag")]
        public string Tag { get; set; } = "Log:Message";

        /// <inheritdoc/>
        [Column("LogMessage")]
        public string Message { get; set; }

        /// <inheritdoc/>
        [Column("LogSeverity")]
        public LogLevel Severity { get; set; } = LogLevel.Information;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                $"Severity :: {Severity} ;; " +
                $"{Tag} :: {Message} ;; " +
                Environment.NewLine;
        }
    }
}

using Easy_Logger.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace Easy_Logger.Models
{
    /// <summary>
    /// Default implementation of <see cref="ILoggerEntry"/>
    /// </summary>
    public class LoggerEntry : ILoggerEntry
    {
        /// <summary>
        /// Creates a new logger message
        /// </summary>
        public LoggerEntry()
        {
            Message = string.Empty;
        }

        /// <summary>
        /// Creates a new logger message
        /// </summary>
        /// <param name="message">The text to record in the logs</param>
        public LoggerEntry(string message)
        {
            Message = message;
        }

        /// <inheritdoc/>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <inheritdoc/>
        public string? Source { get; set; }

        /// <inheritdoc/>
        public string Message { get; set; }

        /// <inheritdoc/>
        public LogLevel Severity { get; set; }

        /// <inheritdoc/>
        public EventId? Id { get; set; }
    }
}

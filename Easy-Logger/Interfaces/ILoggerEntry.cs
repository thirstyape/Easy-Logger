using Microsoft.Extensions.Logging;
using System;

namespace Easy_Logger.Interfaces
{
    /// <summary>
    /// Defines properties required by for logging messages
    /// </summary>
    public interface ILoggerEntry
    {
        /// <summary>
        /// The time at which the entry occurred
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The source that produced the log entry
        /// </summary>
        public string? Source { get; }

        /// <summary>
        /// The text to record to the log(s)
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The importance of the entry
        /// </summary>
        LogLevel Severity { get; }

        /// <summary>
        /// Stores the Event ID passed to the logging function
        /// </summary>
        EventId? Id { get; }
    }
}

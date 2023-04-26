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
        /// A label to identify and filter the entry (ex. Error:Invalid Input)
        /// </summary>
        public string? Tag { get; }

        /// <summary>
        /// The text to record to the log(s)
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The importance of the entry
        /// </summary>
        LogLevel Severity { get; }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Easy_Logger.Interfaces
{
    /// <summary>
    /// Defines properties required by logging options implementations
    /// </summary>
    public interface ILoggerConfiguration
    {
        /// <summary>
        /// The log levels to record log entries for
        /// </summary>
        LogLevel[] LogLevels { get; }

        /// <summary>
        /// Any log messages containing these strings will not be recorded
        /// </summary>
        List<string> IgnoredMessages { get; }

        /// <summary>
        /// A custom formatter to apply to all log messages
        /// </summary>
        Func<ILoggerEntry, string>? Formatter { get; }
    }
}

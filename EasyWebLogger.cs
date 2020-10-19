using Easy_Logger.Interfaces;
using Easy_Logger.Models;

using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Easy_Logger
{
    /// <summary>
    /// Class to connect <see cref="ILogger"/> to <see cref="EasyLoggerService"/>
    /// </summary>
    public class EasyWebLogger : EasyLoggerService, ILogger
    {
        private readonly LogLevel[] LogLevels;

        public EasyWebLogger(ILoggingConfiguration loggingConfiguration, LogLevel[] logLevels) : base(loggingConfiguration)
        {
            LogLevels = logLevels;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return LogLevels.Contains(logLevel);
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel) == false)
                return;

            var loggerEntry = new LoggerEntry()
            {
                Message = formatter(state, exception),
                Severity = logLevel
            };

            SaveToLog(loggerEntry);
        }
    }
}

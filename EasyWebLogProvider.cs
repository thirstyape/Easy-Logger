using Easy_Logger.Interfaces;

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Easy_Logger
{
    /// <summary>
    /// Creates and injects <see cref="ILogger"/> as required
    /// </summary>
    public class EasyWebLogProvider : ILoggerProvider
    {
        private readonly ILoggingConfiguration LoggingConfiguration;
        private readonly LogLevel[] LogLevels;
        private readonly ConcurrentDictionary<string, EasyWebLogger> Loggers = new ConcurrentDictionary<string, EasyWebLogger>();

        public EasyWebLogProvider(ILoggingConfiguration loggingConfiguration, LogLevel[] logLevels)
        {
            LoggingConfiguration = loggingConfiguration;
            LogLevels = logLevels;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return Loggers.GetOrAdd(categoryName, name => new EasyWebLogger(LoggingConfiguration, LogLevels));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Loggers.Clear();
        }
    }
}

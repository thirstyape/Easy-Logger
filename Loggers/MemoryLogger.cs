using Easy_Logger.Interfaces;
using Easy_Logger.Models;
using Easy_Logger.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Logging endpoint that records to an in-memory <see cref="ConcurrentDictionary{TKey, TValue}"/>
    /// </summary>
    public class MemoryLogger : ILogger
    {
        private readonly Func<MemoryLoggerConfiguration> Configuration;
        private readonly string Source;

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="configuration">A function to return the configuration for the logger</param>
        public MemoryLogger(string source, Func<MemoryLoggerConfiguration> configuration)
        {
            Configuration = configuration;
            Source = source;
        }

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="logLevels">A function to return the log levels to record log entries for</param>
        /// <param name="ignoredMessages">Any log messages containing these strings will not be recorded</param>
        /// <param name="expiry">A function to return the duration before log messages expire and are removed from the collection</param>
        /// <param name="memoryLog">The dictionary to store log messages in</param>
        public MemoryLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, Func<TimeSpan> expiry, ConcurrentDictionary<Guid, ILoggerEntry> memoryLog)
        {
            Configuration = () => new MemoryLoggerConfiguration()
            {
                LogLevels = logLevels(),
                IgnoredMessages = ignoredMessages,
                Expiry = expiry(),
                MemoryLog = memoryLog
            };

            Source = source;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => default!;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => Configuration().LogLevels.Contains(logLevel);

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel) == false)
                return;

            var entry = new LoggerEntry(formatter(state, exception))
            {
                Id = eventId,
                Severity = logLevel,
                Source = Source
            };

            if (Configuration().IgnoredMessages.Any(x => entry.Message.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return;

            Configuration().MemoryLog.TryAdd(Guid.NewGuid(), entry);

            var expired = DateTime.Now.Add(Configuration().Expiry.Negate());

            foreach (var item in Configuration().MemoryLog.Where(x => expired > x.Value.Timestamp))
                Configuration().MemoryLog.TryRemove(item.Key, out ILoggerEntry _);
        }
    }
}

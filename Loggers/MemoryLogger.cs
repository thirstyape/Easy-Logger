using Easy_Logger.Interfaces;
using Easy_Logger.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// 
    /// </summary>
    public class MemoryLogger : ILogger
    {
        private readonly string Source;
        private readonly Func<LogLevel[]> LogLevels;
        private readonly List<string> IgnoredMessages;

        private readonly Func<TimeSpan> LogExpiry;
        private readonly ConcurrentDictionary<Guid, ILoggerEntry> MemoryLog;

        public MemoryLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, Func<TimeSpan> logExpiry, ConcurrentDictionary<Guid, ILoggerEntry> memoryLog)
        {
            Source = source;
            LogLevels = logLevels;
            IgnoredMessages = ignoredMessages;

            LogExpiry = logExpiry;
            MemoryLog = memoryLog;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => default!;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => LogLevels().Contains(logLevel);

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel) == false)
                return;

            var content = formatter(state, exception);
            var entry = new LoggerEntry
            {
                Severity = logLevel,
                Tag = Source,
                Message = content
            };

            if (IgnoredMessages.Any(x => content.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return;

            MemoryLog.TryAdd(Guid.NewGuid(), entry);

            var expired = DateTime.Now.Add(LogExpiry().Negate());

            foreach (var item in MemoryLog.Where(x => expired < x.Value.Timestamp))
                MemoryLog.TryRemove(item.Key, out ILoggerEntry _);
        }
    }
}

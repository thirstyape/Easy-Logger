using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// 
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly string Source;
        private readonly Func<LogLevel[]> LogLevels;
        private readonly List<string> IgnoredMessages;

        private readonly Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap;
        
        public ConsoleLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, Dictionary<LogLevel, ConsoleColor> logLevelToColorMap)
        {
            Source = source;
            LogLevels = logLevels;
            IgnoredMessages = ignoredMessages;

            LogLevelToColorMap = logLevelToColorMap;
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

            var now = DateTime.Now;
            var content = formatter(state, exception);
            
            if (IgnoredMessages.Any(x => content.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return;

            Console.ForegroundColor = LogLevelToColorMap[logLevel];
            Console.WriteLine($"{now:yyyy-MM-dd HH:mm:ss.fff}; Severity={logLevel}; Source={Source}");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(content);
        }
    }
}

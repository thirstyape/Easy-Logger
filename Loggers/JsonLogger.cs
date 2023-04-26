using Easy_Logger.Enums;
using Easy_Logger.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Logging endpoint that records to JSON files
    /// </summary>
    public class JsonLogger : FileLoggerBase, ILogger
    {
        public JsonLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, string logDirectory, string logfileNameTemplate, DatedSubdirectoryModes subdirectoryMode) : base(source, logLevels, ignoredMessages, logDirectory, logfileNameTemplate, subdirectoryMode) { }

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
            var content = formatter(state, exception).Replace(";", "").Replace("=", "");
            var entry = new LoggerEntry
            {
                Timestamp = now,
                Severity = logLevel,
                Tag = Source,
                Message = content
            };

            if (IgnoredMessages.Any(x => content.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return;

            try
            {
                var directory = GetTextLogDirectory(now);
                var path = Path.Combine(directory, GetTextLogFilename(now), ".json");

                Directory.CreateDirectory(directory);

                using var writer = new StreamWriter(File.Open(path, FileMode.Append));
                writer.WriteLine(JsonSerializer.Serialize(entry) + ',');
                writer.Close();
            }
            catch { }
        }
    }
}

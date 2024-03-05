using Easy_Logger.Enums;
using Easy_Logger.Models;
using Easy_Logger.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Logging endpoint that records to text files
    /// </summary>
    public class TextLogger : FileLoggerBase, ILogger
    {
        private readonly Func<TextLoggerConfiguration> Configuration;

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="configuration">A function to return the configuration for the logger</param>
        public TextLogger(string source, Func<TextLoggerConfiguration> configuration) : base(source, configuration)
        {
            Configuration = configuration;
        }

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="logLevels">A function to return the log levels to record log entries for</param>
        /// <param name="ignoredMessages">Any log messages containing these strings will not be recorded</param>
        /// <param name="logDirectory">The top-level file system directory to save geneated log files into</param>
        /// <param name="logfileNameTemplate">The template to generate the filename to save logs to</param>
        /// <param name="subdirectoryMode">Specifies how to create dated subdirectories under the log directory</param>
        public TextLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, string logDirectory, string logfileNameTemplate, DatedSubdirectoryModes subdirectoryMode) : base(source, logLevels, ignoredMessages, logDirectory, logfileNameTemplate, subdirectoryMode)
        {
            Configuration = () => new TextLoggerConfiguration()
            {
                LogLevels = logLevels(),
                IgnoredMessages = ignoredMessages,
                LogDirectory = logDirectory,
                LogfileNameTemplate = logfileNameTemplate,
                SubdirectoryMode = subdirectoryMode
            };
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

			var current = Configuration();

			if (current.IgnoredMessages.Any(x => entry.Message.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return;

            string text;

            if (current.Formatter == null)
                text = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}; Severity={entry.Severity}; Source={entry.Source}; Text={entry.Message.Replace(";", "").Replace("=", "")}";
            else
                text = current.Formatter.Invoke(entry);

            try
            {
                var directory = GetTextLogDirectory(entry.Timestamp);
                var file = GetTextLogFilename(entry.Timestamp);
                var path = Path.Combine(directory, $"{file}.txt");

                Directory.CreateDirectory(directory);

                using var writer = new StreamWriter(File.Open(path, FileMode.Append));
                writer.WriteLine(text);
                writer.Close();
            }
            catch { }
        }
    }
}

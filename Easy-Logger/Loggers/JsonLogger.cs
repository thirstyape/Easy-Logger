using Easy_Logger.Enums;
using Easy_Logger.Models;
using Easy_Logger.Providers;
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
        private readonly Func<JsonLoggerConfiguration> Configuration;

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="configuration">A function to return the configuration for the logger</param>
        public JsonLogger(string source, Func<JsonLoggerConfiguration> configuration) : base(source, configuration)
        {
            Configuration = configuration;
        }

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="logLevels">A function to return the log levels to record log entries for</param>
        /// <param name="ignoredMessages">Any log messages containing these strings will not be recorded</param>
        /// <param name="logDirectory">The top-level file system directory to save geneated log files into</param>
        /// <param name="logfileNameTemplate">The template to generate the filename to save logs to</param>
        /// <param name="subdirectoryMode">Specifies how to create dated subdirectories under the log directory</param>
        public JsonLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, string logDirectory, string logfileNameTemplate, DatedSubdirectoryModes subdirectoryMode) : base(source, logLevels, ignoredMessages, logDirectory, logfileNameTemplate, subdirectoryMode)
        {
            Configuration = () => new JsonLoggerConfiguration()
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

            try
            {
                var directory = GetTextLogDirectory(entry.Timestamp);
                var file = GetTextLogFilename(entry.Timestamp);
                var path = Path.Combine(directory, $"{file}.json");

                Directory.CreateDirectory(directory);

                if (current.IsDirtyMode)
                {
                    var text = JsonSerializer.Serialize(entry, current.Options) + ',';

					using var writer = new StreamWriter(File.Open(path, FileMode.Append));
					writer.WriteLine(text);
					writer.Close();
				}
                else
                {
					var entries = new List<LoggerEntry>();

					if (File.Exists(path))
					{
						var text = File.ReadAllText(path);
						entries = JsonSerializer.Deserialize<List<LoggerEntry>>(text, current.Options)!;
					}

					entries.Add(entry);

					using var writer = new StreamWriter(File.Open(path, FileMode.Create));
					writer.Write(JsonSerializer.Serialize(entries, current.Options));
					writer.Close();
				}
            }
            catch { }
        }
    }
}

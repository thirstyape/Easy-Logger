using Easy_Logger.Enums;
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
        public TextLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, string logDirectory, string logfileNameTemplate, DatedSubdirectoryModes subdirectoryMode) : base(source, logLevels, ignoredMessages, logDirectory, logfileNameTemplate, subdirectoryMode) { }

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
            var text = $"{now:yyyy-MM-dd HH:mm:ss.fff}; Severity={logLevel}; Source={Source}; Text={content}";

            if (IgnoredMessages.Any(x => content.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return;

            try
            {
                var directory = GetTextLogDirectory(now);
                var path = Path.Combine(directory, GetTextLogFilename(now), ".txt");

                Directory.CreateDirectory(directory);

                using var writer = new StreamWriter(File.Open(path, FileMode.Append));
                writer.WriteLine(text);
                writer.Close();
            }
            catch { }
        }

        private delegate string ToStringDelegate();

        /// <summary>
        /// Checks to see whether the provided object has a ToString() override method
        /// </summary>
        /// <param name="instance">The object to check</param>
        private bool OverridesToString(object instance)
        {
            if (instance == null)
                return false;

            ToStringDelegate test = instance.ToString;

            return test.Method.DeclaringType == instance.GetType();
        }
    }
}

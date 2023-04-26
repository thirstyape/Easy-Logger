using Easy_Logger.Enums;
using Easy_Logger.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Base class that provides methods to assist with file-based logging endpoints (ex. .txt or .json)
    /// </summary>
    public abstract class FileLoggerBase
    {
        /// <summary>
        /// Stores the source for the logger
        /// </summary>
        protected internal readonly string Source;

        private readonly Func<FileLoggerConfiguration> Configuration;
        private readonly Dictionary<string, string> FilenameDateParts;

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="configuration">A function to return the configuration for the logger</param>
        protected FileLoggerBase(string source, Func<FileLoggerConfiguration> configuration)
        {
            Configuration = configuration;
            Source = source;

            FilenameDateParts = new Dictionary<string, string>();
            SetFilenameDateParts();
        }

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="logLevels">A function to return the log levels to record log entries for</param>
        /// <param name="ignoredMessages">Any log messages containing these strings will not be recorded</param>
        /// <param name="logDirectory">The top-level file system directory to save geneated log files into</param>
        /// <param name="logfileNameTemplate">The template to generate the filename to save logs to</param>
        /// <param name="subdirectoryMode">Specifies how to create dated subdirectories under the log directory</param>
        protected FileLoggerBase(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, string logDirectory, string logfileNameTemplate, DatedSubdirectoryModes subdirectoryMode)
        {
            Configuration = () => new FileLoggerConfiguration()
            {
                LogLevels = logLevels(),
                IgnoredMessages = ignoredMessages,
                LogDirectory = logDirectory,
                LogfileNameTemplate = logfileNameTemplate,
                SubdirectoryMode = subdirectoryMode
            };

            Source = source;

            FilenameDateParts = new Dictionary<string, string>();
            SetFilenameDateParts();
        }

        /// <summary>
        /// Prepares the <see cref="FilenameDateParts"/> library
        /// </summary>
        private void SetFilenameDateParts()
        {
            var dateParts = Regex.Matches(Configuration().LogfileNameTemplate, @"\[Date:[dfgmsyFHM_-]+\]");

            foreach (var match in dateParts.Cast<Match>())
            {
                if (FilenameDateParts.ContainsKey(match.Value))
                    continue;

                FilenameDateParts.Add(match.Value, match.Value.Split(':').Last().TrimEnd(']'));
            }
        }

        /// <summary>
        /// Returns the directory to save logs within after applying any required date based folders
        /// </summary>
        /// <param name="date">The date to store the log</param>
        protected internal string GetTextLogDirectory(DateTime date)
        {
            var year = date.Year.ToString();
            var month = date.Month.ToString().PadLeft(2, '0');
            var day = date.Day.ToString().PadLeft(2, '0');
            var hour = date.Hour.ToString().PadLeft(2, '0');

            var directory = Configuration().SubdirectoryMode switch
            {
                DatedSubdirectoryModes.Hourly => Path.Combine(Configuration().LogDirectory, year, month, day, hour),
                DatedSubdirectoryModes.Daily => Path.Combine(Configuration().LogDirectory, year, month, day),
                DatedSubdirectoryModes.Monthly => Path.Combine(Configuration().LogDirectory, year, month),
                DatedSubdirectoryModes.Yearly => Path.Combine(Configuration().LogDirectory, year),
                _ => Configuration().LogDirectory
            };

            return directory;
        }

        /// <summary>
        /// Returns the filename to save logs under after replacing any template parts
        /// </summary>
        /// <param name="date">The date to store the log</param>
        protected internal string GetTextLogFilename(DateTime date)
        {
            var filename = Configuration().LogfileNameTemplate.Replace("[Source]", Source);

            foreach (var part in FilenameDateParts)
                filename = filename.Replace(part.Key, date.ToString(part.Value));

            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILoggerConfiguration"/> for file-based logging endpoints
    /// </summary>
    public class FileLoggerConfiguration : ILoggerConfiguration
    {
        /// <inheritdoc/>
        public LogLevel[] LogLevels { get; set; } = new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical };

        /// <inheritdoc/>
        public List<string> IgnoredMessages { get; set; } = new List<string>();

        /// <inheritdoc/>
        public Func<ILoggerEntry, string>? Formatter { get; set; }

        /// <summary>
        /// The top-level file system directory to save geneated log files into
        /// </summary>
        public string LogDirectory { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Temp\\Logs") : "/tmp/logs";

        /// <summary>
        /// The template to generate the filename to save logs to
        /// </summary>
        /// <remarks>
        /// Supported template parts:
        /// 
        /// [Date:MM-dd]: Outputs the timestamp for the log entry using standard <see cref="DateTime.ToString(string)"/> options, can be used multiple times.
        /// [Source]    : Outputs the namespace for the source that generated the log entry.
        /// 
        /// Example: 
        ///   <example>
        ///   Template: [Date:yyyy-MM-dd]_[Source]_[Date:HH]
        ///   Outputs : 2023-04-26_My.Namespace_15
        ///   </example>
        /// </remarks>
        public string LogfileNameTemplate { get; set; } = "[Date:yyyy-MM-dd_HH]";

        /// <summary>
        /// Specifies how to create dated subdirectories under the <see cref="LogDirectory"/>
        /// </summary>
        public DatedSubdirectoryModes SubdirectoryMode { get; set; } = DatedSubdirectoryModes.None;
    }
}

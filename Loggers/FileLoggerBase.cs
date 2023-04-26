using Easy_Logger.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Base class that provides methods to assist with text-based logging endpoints (ex. .txt or .json)
    /// </summary>
    public abstract class FileLoggerBase
    {
        protected readonly string Source;
        protected readonly Func<LogLevel[]> LogLevels;
        protected readonly List<string> IgnoredMessages;

        private readonly string LogDirectory;
        private readonly string LogfileNameTemplate;
        private readonly DatedSubdirectoryModes SubdirectoryMode;

        private readonly Dictionary<string, string> FilenameDateParts;

        protected FileLoggerBase(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, string logDirectory, string logfileNameTemplate, DatedSubdirectoryModes subdirectoryMode)
        {
            Source = source;
            LogLevels = logLevels;
            IgnoredMessages = ignoredMessages;

            LogDirectory = logDirectory;
            LogfileNameTemplate = logfileNameTemplate;
            SubdirectoryMode = subdirectoryMode;

            FilenameDateParts = new Dictionary<string, string>();

            var dateParts = Regex.Matches(LogfileNameTemplate, @"\[Date:[dfgmsyFHM_-]+\]");

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
        protected string GetTextLogDirectory(DateTime date)
        {
            var year = date.Year.ToString();
            var month = date.Month.ToString().PadLeft(2, '0');
            var day = date.Day.ToString().PadLeft(2, '0');
            var hour = date.Hour.ToString().PadLeft(2, '0');

            var directory = SubdirectoryMode switch
            {
                DatedSubdirectoryModes.Hourly => Path.Combine(LogDirectory, year, month, day, hour),
                DatedSubdirectoryModes.Daily => Path.Combine(LogDirectory, year, month, day),
                DatedSubdirectoryModes.Monthly => Path.Combine(LogDirectory, year, month),
                DatedSubdirectoryModes.Yearly => Path.Combine(LogDirectory, year),
                _ => LogDirectory
            };

            return directory;
        }

        /// <summary>
        /// Returns the filename to save logs under after applying any required date based filenaming
        /// </summary>
        /// <param name="date">The date to store the log</param>
        protected string GetTextLogFilename(DateTime date)
        {
            var filename = LogfileNameTemplate.Replace("[Source]", Source);

            foreach (var part in FilenameDateParts)
                filename = filename.Replace(part.Key, date.ToString(part.Value));

            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}

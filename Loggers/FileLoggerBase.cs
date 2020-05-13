using Easy_Logger.Enums;
using Easy_Logger.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class FileLoggerBase
    {
        protected readonly Dictionary<string, string> FilenameDateParts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingConfiguration"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        protected FileLoggerBase(ILoggingConfiguration loggingConfiguration)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(loggingConfiguration.LogDirectory))
                throw new ArgumentNullException(nameof(loggingConfiguration.LogDirectory), "Must provide directory to store text-based logs");

            if (string.IsNullOrWhiteSpace(loggingConfiguration.LogFilename))
                throw new ArgumentNullException(nameof(loggingConfiguration.LogFilename), "Must provide filename to save text-based logs under");

            // Prepare directory and file name dictionary
            var dateParts = Regex.Matches(loggingConfiguration.LogFilename, @"\[Date:[dfgmsyFHM_-]+\]");

            if (dateParts.Count == 0 && Regex.IsMatch(loggingConfiguration.LogFilename, @"[a-zA-Z0-9 \._-]+") == false)
                throw new ArgumentException("Invalid log filename specified", nameof(loggingConfiguration.LogFilename));

            Directory.CreateDirectory(loggingConfiguration.LogDirectory);

            FilenameDateParts = new Dictionary<string, string>();

            foreach (Match match in dateParts)
            {
                if (FilenameDateParts.ContainsKey(match.Value))
                    continue;

                FilenameDateParts.Add(match.Value, match.Value.Split(':')[1].TrimEnd(']'));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="loggingConfiguration"></param>
        protected string GetTextLogDirectory(DateTime date, ILoggingConfiguration loggingConfiguration)
        {
            if (loggingConfiguration.UseDatedSubdirectory == false)
                return loggingConfiguration.LogDirectory;

            var year = date.Year;
            var month = date.Month.ToString().PadLeft(2, '0');
            var day = date.Day.ToString().PadLeft(2, '0');
            var hour = date.Hour.ToString().PadLeft(2, '0');

            var subdirectory = loggingConfiguration.DatedSubdirectoryMode switch
            {
                DatedSubdirectoryModes.Hourly => $"{year}\\{month}\\{day}\\{hour}",
                DatedSubdirectoryModes.Daily => $"{year}\\{month}\\{day}",
                DatedSubdirectoryModes.Weekly => $"{year}\\{month}",
                DatedSubdirectoryModes.Yearly => year.ToString(),
                _ => null,
            };

            var directory = Path.Combine(loggingConfiguration.LogDirectory, subdirectory);

            Directory.CreateDirectory(directory);

            return directory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="loggingConfiguration"></param>
        protected string GetTextLogFilename(DateTime date, ILoggingConfiguration loggingConfiguration)
        {
            var filename = loggingConfiguration.LogFilename;

            foreach (var part in FilenameDateParts)
                filename = filename.Replace(part.Key, date.ToString(part.Value));

            return filename;
        }
    }
}

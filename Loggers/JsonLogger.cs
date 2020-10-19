using Easy_Logger.Interfaces;

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Logging endpoint that records to JSON files
    /// </summary>
    public class JsonLogger : FileLoggerBase, ILoggerEndpoint
    {
        public JsonLogger(ILoggingConfiguration loggingConfiguration) : base(loggingConfiguration)
        {
            Settings = loggingConfiguration;
        }

        /// <inheritdoc/>
        public ILoggingConfiguration Settings { get; set; }

        /// <inheritdoc/>
        public bool SaveToLog(ILoggerEntry loggerEntry)
        {
            var directory = GetTextLogDirectory(loggerEntry.Timestamp, Settings);
            var filename = GetTextLogFilename(loggerEntry.Timestamp, Settings);

            var path = Path.Combine(directory, $"{filename}.json");

            SaveToLog(path, loggerEntry);

            return true;
        }

        /// <summary>
        /// Saves the provided entry to the endpoint
        /// </summary>
        /// <param name="path">The full path to store the entry</param>
        /// <param name="entry">The log data to record</param>
        /// <param name="retries">The number of times to retry on failure</param>
        private void SaveToLog(string path, ILoggerEntry entry, int retries = 3)
        {
            try
            {
                // Write to log
                using var writer = new StreamWriter(File.Open(path, FileMode.Append));
                writer.WriteLine(JsonSerializer.Serialize(entry) + ',');
            }
            catch
            {
                // Check for out of tries
                if (retries == 0)
                    throw;

                // Delay and try again
                int delay;

                if (retries >= 3)
                    delay = 100;
                else if (retries == 2)
                    delay = 500;
                else
                    delay = 1_000;

                Task.Delay(delay).Wait();

                SaveToLog(path, entry, --retries);
            }
        }
    }
}

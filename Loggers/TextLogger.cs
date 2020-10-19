using Easy_Logger.Interfaces;

using System;
using System.IO;
using System.Threading.Tasks;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Logging endpoint that records to text files
    /// </summary>
    public class TextLogger : FileLoggerBase, ILoggerEndpoint
    {
        public TextLogger(ILoggingConfiguration loggingConfiguration) : base(loggingConfiguration)
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

            var path = Path.Combine(directory, $"{filename}.txt");

            string message;

            if (OverridesToString(loggerEntry))
                message = loggerEntry.ToString();
            else
                message = $"{loggerEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                    $"Severity :: {loggerEntry.Severity} ;; " +
                    $"{loggerEntry.Tag} :: {loggerEntry.Message} ;; " +
                    Environment.NewLine;

            SaveToLog(path, message);

            return true;
        }

        /// <summary>
        /// Saves the provided entry to the endpoint
        /// </summary>
        /// <param name="path">The full path to store the entry</param>
        /// <param name="entry">The log data to record</param>
        /// <param name="retries">The number of times to retry on failure</param>
        private void SaveToLog(string path, string entry, int retries = 3)
        {
            try
            {
                // Write to log
                using var writer = new StreamWriter(File.Open(path, FileMode.Append));
                writer.WriteLine(entry);
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

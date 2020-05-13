using Easy_Logger.Interfaces;

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonLogger : FileLoggerBase, ILogger
    {
        public JsonLogger(ILoggingConfiguration loggingConfiguration) : base(loggingConfiguration)
        {
            Settings = loggingConfiguration;
        }

        public ILoggingConfiguration Settings { get; set; }

        public bool SaveToLog(ILoggerEntry loggerEntry)
        {
            var directory = GetTextLogDirectory(loggerEntry.Timestamp, Settings);
            var filename = GetTextLogFilename(loggerEntry.Timestamp, Settings);

            var path = Path.Combine(directory, filename);

            SaveToLog(path, loggerEntry);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="entry"></param>
        /// <param name="retries"></param>
        private void SaveToLog(string path, ILoggerEntry entry, int retries = 3)
        {
            try
            {
                // Write to log
                using var writer = new StreamWriter(File.Open(path, FileMode.Append));
                writer.WriteLine(JsonSerializer.Serialize(entry));
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

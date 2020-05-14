using Easy_Logger.Interfaces;
using Easy_Logger.Loggers;
using Easy_Logger.Models;

using System.Collections.Generic;
using System.Linq;

namespace Easy_Logger
{
    /// <summary>
    /// Main class to record data to logging endpoints
    /// </summary>
    public class EasyLoggerService
    {
        private readonly List<ILogger> Loggers;

        /// <summary>
        /// Prepares the logging service for recording to logging endpoints
        /// </summary>
        /// <param name="loggingConfiguration">The parameters to record log records with</param>
        public EasyLoggerService(ILoggingConfiguration loggingConfiguration)
        {
            Settings = loggingConfiguration;

            // Prepare loggers
            Loggers = new List<ILogger>();

            if (Settings.UseTextLogger)
                Loggers.Add(new TextLogger(Settings));

            if (Settings.UseJsonLogger)
                Loggers.Add(new JsonLogger(Settings));

            if (Settings.UseSqlLogger)
                Loggers.Add(new SqlLogger(Settings));
        }

        /// <summary>
        /// The configuration settings to use when recording logs
        /// </summary>
        public ILoggingConfiguration Settings { get; private set; }

        /// <summary>
        /// Records the provided message to the configured endpoints
        /// </summary>
        /// <param name="message">The text to record to the log(s)</param>
        public bool SaveToLog(string message)
        {
            return SaveToLog(new LoggerEntry()
            {
                Message = message
            });
        }

        /// <summary>
        /// Records the provided logging entry to the configured endpoints
        /// </summary>
        /// <param name="loggerEntry">The data to record to the log(s)</param>
        public bool SaveToLog(ILoggerEntry loggerEntry)
        {
            return Loggers.All(x => x.SaveToLog(loggerEntry));
        }

        /// <summary>
        /// Adds a custom logging endpoint
        /// </summary>
        /// <param name="logger">The endpoint to add</param>
        public void AddLogger(ILogger logger)
        {
            Loggers.Add(logger);
        }
    }
}

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
        /// <summary>
        /// Contains the individual logging endpoints
        /// </summary>
        protected readonly List<ILoggerEndpoint> Endpoints;

        /// <summary>
        /// Prepares the logging service for recording to logging endpoints
        /// </summary>
        /// <param name="loggingConfiguration">The parameters to record log records with</param>
        public EasyLoggerService(ILoggingConfiguration loggingConfiguration)
        {
            Settings = loggingConfiguration;

            // Prepare loggers
            Endpoints = new List<ILoggerEndpoint>();

            if (Settings.UseTextLogger)
                Endpoints.Add(new TextLogger(Settings));

            if (Settings.UseJsonLogger)
                Endpoints.Add(new JsonLogger(Settings));
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
            return Endpoints.All(x => x.SaveToLog(loggerEntry));
        }

        /// <summary>
        /// Adds a custom logging endpoint
        /// </summary>
        /// <param name="logger">The endpoint to add</param>
        public void AddLogger(ILoggerEndpoint logger)
        {
            Endpoints.Add(logger);
        }
    }
}

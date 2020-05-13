using Easy_Logger.Interfaces;
using Easy_Logger.Loggers;
using Easy_Logger.Models;

using System.Collections.Generic;
using System.Linq;

namespace Easy_Logger
{
    /// <summary>
    /// 
    /// </summary>
    public class EasyLoggerService
    {
        private readonly List<ILogger> Loggers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingConfiguration"></param>
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
        /// 
        /// </summary>
        public ILoggingConfiguration Settings { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public bool SaveToLog(string message)
        {
            return SaveToLog(new LoggerEntry()
            {
                Message = message
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerEntry"></param>
        public bool SaveToLog(ILoggerEntry loggerEntry)
        {
            Loggers.All(x => x.SaveToLog(loggerEntry));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public void AddLogger(ILogger logger)
        {
            Loggers.Add(logger);
        }
    }
}

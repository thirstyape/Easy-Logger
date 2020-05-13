using Microsoft.Extensions.Logging;
using System;

namespace Easy_Logger.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILoggerEntry
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        LogLevel Severity { get; set; }
    }
}

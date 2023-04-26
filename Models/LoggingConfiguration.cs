using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Easy_Logger.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LoggingConfiguration
    {
        /// <inheritdoc/>
        public string LogDirectory { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Temp\\Logs") : "/tmp/logs";

        /// <inheritdoc/>
        public string LogFilename { get; set; } = "[Date:yyyy-MM-dd_HH]";
    }
}

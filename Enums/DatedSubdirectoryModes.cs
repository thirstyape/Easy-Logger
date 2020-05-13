namespace Easy_Logger.Enums
{
    /// <summary>
    /// Enum that specifies how text-based logging subdirectories are created
    /// </summary>
    public enum DatedSubdirectoryModes
    {
        /// <summary>
        /// Creates a new directory each hour (i.e. yyyy/mm/dd/hh/logfile.log)
        /// </summary>
        Hourly,

        /// <summary>
        /// Creates a new directory each day (i.e. yyyy/mm/dd/logfile.log)
        /// </summary>
        Daily,

        /// <summary>
        /// Creates a new directory each month (i.e. yyyy/mm/logfile.log)
        /// </summary>
        Monthly,

        /// <summary>
        /// Creates a new directory each year (i.e. yyyy/logfile.log)
        /// </summary>
        Yearly
    }
}

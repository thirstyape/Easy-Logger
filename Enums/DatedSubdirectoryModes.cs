namespace Easy_Logger.Enums
{
    /// <summary>
    /// Enum that specifies how text-based logging subdirectories are created
    /// </summary>
    public enum DatedSubdirectoryModes
    {
        /// <summary>
        /// Does not create new directories based on time
        /// </summary>
        None,

        /// <summary>
        /// Creates a new directory each hour (i.e. yyyy/mm/dd/hh/logfile.txt)
        /// </summary>
        Hourly,

        /// <summary>
        /// Creates a new directory each day (i.e. yyyy/mm/dd/logfile.txt)
        /// </summary>
        Daily,

        /// <summary>
        /// Creates a new directory each month (i.e. yyyy/mm/logfile.txt)
        /// </summary>
        Monthly,

        /// <summary>
        /// Creates a new directory each year (i.e. yyyy/logfile.txt)
        /// </summary>
        Yearly
    }
}

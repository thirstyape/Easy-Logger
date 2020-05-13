using Easy_Logger.Interfaces;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Logging endpoint that records to a SQL database
    /// </summary>
    public class SqlLogger : ILogger
    {
        private readonly SqlServerService sql;
        private readonly List<SqlParameter> parameters;

        private Dictionary<string, string> columnNames;
        private Type lastEntryType;

        /// <summary>
        /// Prepares the logger for adding data to a SQL database
        /// </summary>
        /// <param name="loggingConfiguration">The settings to use with the logger</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SqlLogger(ILoggingConfiguration loggingConfiguration)
        {
            Settings = loggingConfiguration;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(Settings.LogSqlTable))
                throw new ArgumentNullException(nameof(Settings.LogSqlTable), "Must provide SQL table name to add logs to");

            if (string.IsNullOrWhiteSpace(Settings.SqlConnectionString))
                throw new ArgumentNullException(nameof(Settings.SqlConnectionString), "Must provide SQL connection string");

            // Prepare class
            sql = new SqlServerService(Settings.SqlConnectionString);
            parameters = new List<SqlParameter>();
        }

        public ILoggingConfiguration Settings { get; set; }

        public bool SaveToLog(ILoggerEntry loggerEntry)
        {
            try
            {
                SetColumnNames(loggerEntry);

                var query = $"INSERT INTO {Settings.LogSqlTable} " +
                    $"({columnNames["LogDate"]}, {columnNames["LogTag"]}, {columnNames["LogMessage"]}, {columnNames["LogSeverity"]}) " +
                    "VALUES (@LogDate, @LogTag, @LogMessage, @LogSeverity)";

                parameters.Add(new SqlParameter("@LogDate", SqlDbType.DateTime2) { Value = loggerEntry.Timestamp });
                parameters.Add(new SqlParameter("@LogTag", SqlDbType.VarChar) { Value = loggerEntry.Tag });
                parameters.Add(new SqlParameter("@LogMessage", SqlDbType.VarChar) { Value = loggerEntry.Message });
                parameters.Add(new SqlParameter("@LogSeverity", SqlDbType.Int) { Value = loggerEntry.Severity });

                return sql.ExecuteNonQuery(query, parameters) == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the column names to use when saving log data to SQL
        /// </summary>
        /// <param name="loggerEntry">The entry to save to SQL</param>
        private void SetColumnNames(ILoggerEntry loggerEntry)
        {
            // Use cached values when possible
            if (lastEntryType != null && lastEntryType == loggerEntry.GetType())
                return;
            else
                lastEntryType = loggerEntry.GetType();

            // Update values
            var logDate = (lastEntryType.GetProperty(nameof(loggerEntry.Timestamp)) as MemberInfo).GetCustomAttribute<ColumnAttribute>().Name ?? "LogDate";
            var logTag = (lastEntryType.GetProperty(nameof(loggerEntry.Tag)) as MemberInfo).GetCustomAttribute<ColumnAttribute>().Name ?? "LogTag";
            var logMessage = (lastEntryType.GetProperty(nameof(loggerEntry.Message)) as MemberInfo).GetCustomAttribute<ColumnAttribute>().Name ?? "LogMessage";
            var logSeverity = (lastEntryType.GetProperty(nameof(loggerEntry.Severity)) as MemberInfo).GetCustomAttribute<ColumnAttribute>().Name ?? "LogSeverity";

            columnNames = new Dictionary<string, string>()
            {
                ["LogDate"] = logDate,
                ["LogTag"] = logTag,
                ["LogMessage"] = logMessage,
                ["LogSeverity"] = logSeverity
            };
        }
    }
}

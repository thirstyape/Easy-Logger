using Easy_Logger.Models;
using Easy_Logger.Providers;
using Easy_Logger_Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Easy_Logger.Loggers
{
    /// <summary>
    /// Logging endpoint that records to a SQL Server database
    /// </summary>
    public class SqlLogger : ILogger
    {
        private readonly SqlConnectionService SqlConnection;
        private readonly Func<SqlLoggerConfiguration> Configuration;
        private readonly string Source;

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="configuration">A function to return the configuration for the logger</param>
        public SqlLogger(string source, Func<SqlLoggerConfiguration> configuration)
        {
            Configuration = configuration;
            Source = source;
            SqlConnection = new SqlConnectionService(Configuration().ConnectionString);
        }

        /// <param name="source">Stores the source for the logger</param>
        /// <param name="logLevels">A function to return the log levels to record log entries for</param>
        /// <param name="ignoredMessages">Any log messages containing these strings will not be recorded</param>
        /// <param name="connectionString">Stores the connection details for the SQL Server to record logs on</param>
        /// <param name="sqlTableName">The text name of the table to store logs to in SQL Server</param>
        public SqlLogger(string source, Func<LogLevel[]> logLevels, List<string> ignoredMessages, SqlConnectionStringBuilder connectionString, string sqlTableName)
        {
            Configuration = () => new SqlLoggerConfiguration(connectionString, sqlTableName)
            {
                LogLevels = logLevels(),
                IgnoredMessages = ignoredMessages
            };

            Source = source;
            SqlConnection = new SqlConnectionService(Configuration().ConnectionString);
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => default!;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => Configuration().LogLevels.Contains(logLevel);

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel) == false)
                return;

            var entry = new LoggerEntry(formatter(state, exception))
            {
                Id = eventId,
                Severity = logLevel,
                Source = Source
            };

			var current = Configuration();

			if (current.IgnoredMessages.Any(x => entry.Message.Contains(x, StringComparison.OrdinalIgnoreCase)))
                return;

            var query = $"INSERT INTO {GetSqlName(current.SqlTableName)} ({GetSqlName(current.LogDateColumnName)}, {GetSqlName(current.LogSourceColumnName)}, {GetSqlName(current.LogMessageColumnName)}, {GetSqlName(current.LogSeverityColumnName)}) VALUES (@LogDate, @LogSource, @LogMessage, @LogSeverity)";
            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@LogDate", SqlDbType.DateTime2) { Value = entry.Timestamp },
                new SqlParameter("@LogSource", SqlDbType.NVarChar) { Value = entry.Source },
                new SqlParameter("@LogMessage", SqlDbType.NVarChar) { Value = entry.Message },
                new SqlParameter("@LogSeverity", SqlDbType.Int) { Value = (int)entry.Severity }
            };

            try
            {
                SqlConnection.ExecuteNonQuery(query, parameters);
            }
            catch { }
        }

        private string GetSqlName(string columnName) => string.Concat(columnName.Where(x => char.IsLetterOrDigit(x) || x == '_' || x == '#'));
    }
}

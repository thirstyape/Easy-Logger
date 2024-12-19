using Easy_Logger.Interfaces;
using Easy_Logger.Loggers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;

namespace Easy_Logger.Providers
{
    /// <summary>
    /// Creates instances of <see cref="SqlLogger"/> as required
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider">Documentation</see>.
    /// </remarks>
    [ProviderAlias("SqlLogger")]
    public class SqlLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);
        private SqlLoggerConfiguration Configuration;
        private readonly IDisposable? OnChangeToken;

		/// <param name="configuration">The configuration to use with created loggers</param>
		public SqlLoggerProvider(SqlLoggerConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <param name="configuration">The configuration to use with created loggers</param>
        public SqlLoggerProvider(IOptionsMonitor<SqlLoggerConfiguration> configuration)
        {
            Configuration = configuration.CurrentValue;
            OnChangeToken = configuration.OnChange(updated => Configuration = updated);
        }

        private SqlLoggerConfiguration GetConfiguration() => Configuration;

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => Loggers.GetOrAdd(categoryName, source => new SqlLogger(source, GetConfiguration));

        /// <inheritdoc/>
        public void Dispose()
        {
            Loggers.Clear();
            OnChangeToken?.Dispose();
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILoggerConfiguration"/> for <see cref="SqlLogger"/>
    /// </summary>
    public class SqlLoggerConfiguration : ILoggerConfiguration
    {
        /// <param name="connectionString">The connection string to use to connect to the SQL Server</param>
        /// <param name="sqlTableName">The text name of the table to store logs to in SQL Server</param>
        public SqlLoggerConfiguration(SqlConnectionStringBuilder connectionString, string sqlTableName)
        {
            ConnectionString = connectionString;
            SqlTableName = sqlTableName;
        }

        /// <inheritdoc/>
        public LogLevel[] LogLevels { get; set; } = new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical };

        /// <inheritdoc/>
        public List<string> IgnoredMessages { get; set; } = new List<string>();

        /// <inheritdoc/>
        /// <remarks>
        /// Not used in <see cref="SqlLogger"/>
        /// </remarks>
        public Func<ILoggerEntry, string>? Formatter { get; set; }

        /// <summary>
        /// Stores the connection details for the SQL Server to record logs on
        /// </summary>
        public SqlConnectionStringBuilder ConnectionString { get; set; }

        /// <summary>
        /// The text name of the table to store logs to in SQL Server
        /// </summary>
        /// <remarks>
        /// May only use a combination of letters, numbers, _, and #
        /// </remarks>
        public string SqlTableName { get; set; }

        /// <summary>
        /// The text name of the column to store the log date in
        /// </summary>
        /// <remarks>
        /// May only use a combination of letters, numbers, _, and #
        /// </remarks>
        public string LogDateColumnName { get; set; } = "LogDate";

        /// <summary>
        /// The text name of the column to store the log source in
        /// </summary>
        /// <remarks>
        /// May only use a combination of letters, numbers, _, and #
        /// </remarks>
        public string LogSourceColumnName { get; set; } = "LogSource";

        /// <summary>
        /// The text name of the column to store the log message in
        /// </summary>
        /// <remarks>
        /// May only use a combination of letters, numbers, _, and #
        /// </remarks>
        public string LogMessageColumnName { get; set; } = "LogMessage";

        /// <summary>
        /// The text name of the column to store the log severity in
        /// </summary>
        /// <remarks>
        /// May only use a combination of letters, numbers, _, and #
        /// </remarks>
        public string LogSeverityColumnName { get; set; } = "LogSeverity";
    }

    /// <summary>
    /// Contains methods to consume <see cref="SqlLogger"/> in a DI environment
    /// </summary>
    public static class SqlLoggerExtensions
    {
        /// <summary>
        /// Adds <see cref="SqlLogger"/> to the service collection with default options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        /// <param name="server">The DNS name or IP address of the server to connect to</param>
        /// <param name="database">The name of the database to run queries against</param>
        /// <param name="username">The username to access the server</param>
        /// <param name="password">The password to access the server</param>
        /// <param name="table">The text name of the table to store logs to in SQL Server</param>
        /// <param name="integratedSecurity">Specifies whether to use the current Windows credentials to connect to the SQL Server</param>
        public static ILoggingBuilder AddSqlLogger(this ILoggingBuilder builder, string server, string database, string? username, string? password, string table, bool integratedSecurity = false)
        {
            return builder.AddSqlLogger(new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = username,
                Password = password,
                IntegratedSecurity = integratedSecurity
            }, table);
        }

        /// <summary>
        /// Adds <see cref="SqlLogger"/> to the service collection with default options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        /// <param name="connectionString">The connection string to use to connect to the SQL Server</param>
        /// <param name="sqlTableName">The text name of the table to store logs to in SQL Server</param>
        public static ILoggingBuilder AddSqlLogger(this ILoggingBuilder builder, SqlConnectionStringBuilder connectionString, string sqlTableName)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SqlLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<SqlLoggerConfiguration, SqlLoggerProvider>(builder.Services);
            builder.Services.Configure<SqlLoggerConfiguration>(x => new SqlLoggerConfiguration(connectionString, sqlTableName));

            return builder;
        }

        /// <summary>
        /// Adds <see cref="SqlLogger"/> to the service collection with the provided options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        /// <param name="configure">The options to use for created loggers</param>
        public static ILoggingBuilder AddSqlLogger(this ILoggingBuilder builder, Action<SqlLoggerConfiguration> configure)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SqlLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<SqlLoggerConfiguration, SqlLoggerProvider>(builder.Services);
            builder.Services.Configure(configure);

            return builder;
        }
    }
}

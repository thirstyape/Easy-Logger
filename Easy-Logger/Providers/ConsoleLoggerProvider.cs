using Easy_Logger.Interfaces;
using Easy_Logger.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Easy_Logger.Providers
{
    /// <summary>
    /// Creates instances of <see cref="ConsoleLogger"/> as required
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider">Documentation</see>.
    /// </remarks>
    [ProviderAlias("ConsoleLogger")]
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);
        private ConsoleLoggerConfiguration Configuration;
        private readonly IDisposable OnChangeToken;

        /// <param name="configuration">The configuration to use with created loggers</param>
        public ConsoleLoggerProvider(IOptionsMonitor<ConsoleLoggerConfiguration> configuration)
        {
            Configuration = configuration.CurrentValue;
            OnChangeToken = configuration.OnChange(updated => Configuration = updated);
        }

        private ConsoleLoggerConfiguration GetConfiguration() => Configuration;

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => Loggers.GetOrAdd(categoryName, source => new ConsoleLogger(source, GetConfiguration));

        /// <inheritdoc/>
        public void Dispose()
        {
            Loggers.Clear();
            OnChangeToken.Dispose();
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILoggerConfiguration"/> for <see cref="ConsoleLogger"/>
    /// </summary>
    public class ConsoleLoggerConfiguration : ILoggerConfiguration
    {
        /// <inheritdoc/>
        public LogLevel[] LogLevels { get; set; } = new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical };

        /// <inheritdoc/>
        public List<string> IgnoredMessages { get; set; } = new List<string>();

        /// <inheritdoc/>
        public Func<ILoggerEntry, string>? Formatter { get; set; } = entry =>
        {
            return $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}; Severity={entry.Severity}; Source={entry.Source}" + Environment.NewLine + entry.Message + Environment.NewLine;
        };

        /// <summary>
        /// A dictionary containing the colors to ouput log messages in based on severity
        /// </summary>
        /// <remarks>
        /// When an option is missing from the map <see cref="ConsoleColor.White"/> will be used
        /// </remarks>
        public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new Dictionary<LogLevel, ConsoleColor>()
        {
            [LogLevel.Trace] = ConsoleColor.Cyan,
            [LogLevel.Debug] = ConsoleColor.Blue,
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Critical] = ConsoleColor.Magenta
        };

        /// <summary>
        /// Specfies whether to color log messages according to their <see cref="LogLevel"/>
        /// </summary>
        /// <remarks>
        /// Certain platforms may not support colorization
        /// </remarks>
        public bool UseColoredMessages { get; set; } = true;
    }

    /// <summary>
    /// Contains methods to consume <see cref="ConsoleLogger"/> in a DI environment
    /// </summary>
    public static class ConsoleLoggerExtensions
    {
        /// <summary>
        /// Adds <see cref="ConsoleLogger"/> to the service collection with default options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        public static ILoggingBuilder AddConsoleLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ConsoleLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<ConsoleLoggerConfiguration, ConsoleLoggerProvider>(builder.Services);
            builder.Services.Configure<ConsoleLoggerConfiguration>(x => new ConsoleLoggerConfiguration());

            return builder;
        }

        /// <summary>
        /// Adds <see cref="ConsoleLogger"/> to the service collection with the provided options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        /// <param name="configure">The options to use for created loggers</param>
        public static ILoggingBuilder AddConsoleLogger(this ILoggingBuilder builder, Action<ConsoleLoggerConfiguration> configure)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ConsoleLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<ConsoleLoggerConfiguration, ConsoleLoggerProvider>(builder.Services);
            builder.Services.Configure(configure);

            return builder;
        }
    }
}

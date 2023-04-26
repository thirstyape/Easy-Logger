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
    /// Creates instances of <see cref="MemoryLogger"/> as required
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider">Documentation</see>.
    /// </remarks>
    [ProviderAlias("MemoryLogger")]
    public class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);
        private MemoryLoggerConfiguration Configuration;
        private readonly IDisposable OnChangeToken;

        /// <param name="configuration">The configuration to use with created loggers</param>
        public MemoryLoggerProvider(IOptionsMonitor<MemoryLoggerConfiguration> configuration)
        {
            Configuration = configuration.CurrentValue;
            OnChangeToken = configuration.OnChange(updated => Configuration = updated);
        }

        /// <summary>
        /// Stores generated log items when no memory log is provided in the configuration
        /// </summary>
        public static ConcurrentDictionary<Guid, ILoggerEntry> DefaultLogger { get; set; } = new ConcurrentDictionary<Guid, ILoggerEntry>();

        private MemoryLoggerConfiguration GetConfiguration() => Configuration;

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => Loggers.GetOrAdd(categoryName, source => new MemoryLogger(source, GetConfiguration));

        /// <inheritdoc/>
        public void Dispose()
        {
            Loggers.Clear();
            OnChangeToken.Dispose();
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILoggerConfiguration"/> for <see cref="MemoryLogger"/>
    /// </summary>
    public class MemoryLoggerConfiguration : ILoggerConfiguration
    {
        /// <inheritdoc/>
        public LogLevel[] LogLevels { get; set; } = new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical };

        /// <inheritdoc/>
        public List<string> IgnoredMessages { get; set; } = new List<string>();

        /// <inheritdoc/>
        /// <remarks>
        /// Not used in <see cref="MemoryLogger"/>
        /// </remarks>
        public Func<ILoggerEntry, string>? Formatter { get; set; }

        /// <summary>
        /// The duration before log messages expire and are removed from the collection
        /// </summary>
        public TimeSpan Expiry { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// The dictionary to store log messages in
        /// </summary>
        public ConcurrentDictionary<Guid, ILoggerEntry> MemoryLog { get; set; } = MemoryLoggerProvider.DefaultLogger;
    }

    /// <summary>
    /// Contains methods to consume <see cref="MemoryLogger"/> in a DI environment
    /// </summary>
    public static class MemoryLoggerExtensions
    {
        /// <summary>
        /// Adds <see cref="MemoryLogger"/> to the service collection with default options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<MemoryLoggerConfiguration, MemoryLoggerProvider>(builder.Services);
            builder.Services.Configure<MemoryLoggerConfiguration>(x => new MemoryLoggerConfiguration());

            return builder;
        }

        /// <summary>
        /// Adds <see cref="MemoryLogger"/> to the service collection with the provided options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        /// <param name="configure">The options to use for created loggers</param>
        public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, Action<MemoryLoggerConfiguration> configure)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<MemoryLoggerConfiguration, MemoryLoggerProvider>(builder.Services);
            builder.Services.Configure(configure);

            return builder;
        }
    }
}

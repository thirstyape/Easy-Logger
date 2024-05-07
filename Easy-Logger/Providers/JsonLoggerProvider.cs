using Easy_Logger.Interfaces;
using Easy_Logger.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Easy_Logger.Providers
{
    /// <summary>
    /// Creates instances of <see cref="JsonLogger"/> as required
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider">Documentation</see>.
    /// </remarks>
    [ProviderAlias("JsonLogger")]
    public class JsonLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);
        private JsonLoggerConfiguration Configuration;
        private readonly IDisposable OnChangeToken;

        /// <param name="configuration">The configuration to use with created loggers</param>
        public JsonLoggerProvider(IOptionsMonitor<JsonLoggerConfiguration> configuration)
        {
            Configuration = configuration.CurrentValue;
            OnChangeToken = configuration.OnChange(updated => Configuration = updated);
        }

        private JsonLoggerConfiguration GetConfiguration() => Configuration;

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => Loggers.GetOrAdd(categoryName, source => new JsonLogger(source, GetConfiguration));

        /// <inheritdoc/>
        public void Dispose()
        {
            Loggers.Clear();
            OnChangeToken.Dispose();
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILoggerConfiguration"/> for <see cref="JsonLogger"/>
    /// </summary>
    public class JsonLoggerConfiguration : FileLoggerConfiguration
    {
        /// <summary>
        /// Specifies whether to enable dirty JSON mode. Dirty mode does not produce a valid JSON file, it will be missing the opening and closing bracket; however, it does not need to parse the file on new entries and thus is much faster.
        /// </summary>
        public bool IsDirtyMode { get; set; }

        /// <inheritdoc cref="ILoggerConfiguration.Formatter" />
        /// <remarks>
        /// Not used in <see cref="JsonLogger"/>
        /// </remarks>
        public new Func<ILoggerEntry, string>? Formatter { get; set; }

        /// <summary>
        /// Serialization options to use when saving log entries
        /// </summary>
        public JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions()
        {
            AllowTrailingCommas = false,
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = false,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Contains methods to consume <see cref="JsonLogger"/> in a DI environment
    /// </summary>
    public static class JsonLoggerExtensions
    {
        /// <summary>
        /// Adds <see cref="JsonLogger"/> to the service collection with default options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        public static ILoggingBuilder AddJsonLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, JsonLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<JsonLoggerConfiguration, JsonLoggerProvider>(builder.Services);
            builder.Services.Configure<JsonLoggerConfiguration>(x => new JsonLoggerConfiguration());

            return builder;
        }

        /// <summary>
        /// Adds <see cref="JsonLogger"/> to the service collection with the provided options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        /// <param name="configure">The options to use for created loggers</param>
        public static ILoggingBuilder AddJsonLogger(this ILoggingBuilder builder, Action<JsonLoggerConfiguration> configure)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, JsonLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<JsonLoggerConfiguration, JsonLoggerProvider>(builder.Services);
            builder.Services.Configure(configure);

            return builder;
        }
    }
}

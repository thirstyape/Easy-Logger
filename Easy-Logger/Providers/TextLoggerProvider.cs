using Easy_Logger.Interfaces;
using Easy_Logger.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Easy_Logger.Providers
{
    /// <summary>
    /// Creates instances of <see cref="TextLogger"/> as required
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider">Documentation</see>.
    /// </remarks>
    [ProviderAlias("TextLogger")]
    public class TextLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);
        private TextLoggerConfiguration Configuration;
        private readonly IDisposable? OnChangeToken;

		public TextLoggerProvider()
		{
			Configuration = new TextLoggerConfiguration();
		}

		/// <param name="configuration">The configuration to use with created loggers</param>
		public TextLoggerProvider(TextLoggerConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <param name="configuration">The configuration to use with created loggers</param>
        public TextLoggerProvider(IOptionsMonitor<TextLoggerConfiguration> configuration)
        {
            Configuration = configuration.CurrentValue;
            OnChangeToken = configuration.OnChange(updated => Configuration = updated);
        }

        private TextLoggerConfiguration GetConfiguration() => Configuration;

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => Loggers.GetOrAdd(categoryName, source => new TextLogger(source, GetConfiguration));

        /// <inheritdoc/>
        public void Dispose()
        {
            Loggers.Clear();
            OnChangeToken?.Dispose();
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILoggerConfiguration"/> for <see cref="TextLogger"/>
    /// </summary>
    public class TextLoggerConfiguration : FileLoggerConfiguration
    {
        /// <inheritdoc cref="ILoggerConfiguration.Formatter" />
        public new Func<ILoggerEntry, string>? Formatter { get; set; } = entry =>
        {
            return $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}; Severity={entry.Severity}; Source={entry.Source}; Text={entry.Message.Replace(";", "").Replace("=", "")}";
        };
    }

    /// <summary>
    /// Contains methods to consume <see cref="TextLogger"/> in a DI environment
    /// </summary>
    public static class TextLoggerExtensions
    {
        /// <summary>
        /// Adds <see cref="TextLogger"/> to the service collection with default options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        public static ILoggingBuilder AddTextLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<TextLoggerConfiguration, TextLoggerProvider>(builder.Services);
            builder.Services.Configure<TextLoggerConfiguration>(x => new TextLoggerConfiguration());

            return builder;
        }

        /// <summary>
        /// Adds <see cref="TextLogger"/> to the service collection with the provided options
        /// </summary>
        /// <param name="builder">The builder containing the service collection</param>
        /// <param name="configure">The options to use for created loggers</param>
        public static ILoggingBuilder AddTextLogger(this ILoggingBuilder builder, Action<TextLoggerConfiguration> configure)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<TextLoggerConfiguration, TextLoggerProvider>(builder.Services);
            builder.Services.Configure(configure);

            return builder;
        }
    }
}

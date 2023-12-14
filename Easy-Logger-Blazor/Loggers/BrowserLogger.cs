using Easy_Logger.Models;
using Easy_Logger.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Easy_Logger.Loggers;

/// <summary>
/// Logging endpoint that records to the browser console using JS Interop
/// </summary>
public class BrowserLogger : ILogger
{
	private readonly Func<BrowserLoggerConfiguration> Configuration;
	private readonly string Source;
	private readonly IJSRuntime JsRuntime;

	/// <param name="source">Stores the source for the logger</param>
	/// <param name="jsRuntime">A JS Runtime object to record logs</param>
	/// <param name="configuration">A function to return the configuration for the logger</param>
	public BrowserLogger(string source, IJSRuntime jsRuntime, Func<BrowserLoggerConfiguration> configuration)
	{
		Configuration = configuration;
		JsRuntime = jsRuntime;
		Source = source;
	}

	/// <param name="source">Stores the source for the logger</param>
	/// <param name="jsRuntime">A JS Runtime object to record logs</param>
	/// <param name="logLevels">A function to return the log levels to record log entries for</param>
	/// <param name="ignoredMessages">Any log messages containing these strings will not be recorded</param>
	public BrowserLogger(string source, IJSRuntime jsRuntime, Func<LogLevel[]> logLevels, List<string> ignoredMessages) 
	{
		Configuration = () => new BrowserLoggerConfiguration()
		{
			LogLevels = logLevels(),
			IgnoredMessages = ignoredMessages
		};

		JsRuntime = jsRuntime;
		Source = source;
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

		string message;

		if (current.Formatter == null)
			message = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}; Severity={entry.Severity}; Source={entry.Source}" + Environment.NewLine + entry.Message;
		else
			message = current.Formatter.Invoke(entry);

		if (current.CssStyles != null && current.Formatter != null)
		{
			_ = logLevel switch
			{
				LogLevel.Critical or LogLevel.Error => JsRuntime.InvokeVoidAsync("console.error", message, current.CssStyles.ToArray()),
				LogLevel.Warning => JsRuntime.InvokeVoidAsync("console.warn", message, current.CssStyles.ToArray()),
				LogLevel.Information => JsRuntime.InvokeVoidAsync("console.info", message, current.CssStyles.ToArray()),
				LogLevel.Debug => JsRuntime.InvokeVoidAsync("console.debug", message, current.CssStyles.ToArray()),
				LogLevel.Trace => JsRuntime.InvokeVoidAsync("console.trace", message, current.CssStyles.ToArray()),
				_ => JsRuntime.InvokeVoidAsync("console.log", message, current.CssStyles.ToArray())
			};
		}
		else
		{
			_ = logLevel switch
			{
				LogLevel.Critical or LogLevel.Error => JsRuntime.InvokeVoidAsync("console.error", message),
				LogLevel.Warning => JsRuntime.InvokeVoidAsync("console.warn", message),
				LogLevel.Information => JsRuntime.InvokeVoidAsync("console.info", message),
				LogLevel.Debug => JsRuntime.InvokeVoidAsync("console.debug", message),
				LogLevel.Trace => JsRuntime.InvokeVoidAsync("console.trace", message),
				_ => JsRuntime.InvokeVoidAsync("console.log", message)
			};
		}
	}
}

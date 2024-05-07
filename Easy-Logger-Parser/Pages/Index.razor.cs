using easy_blazor_bulma;
using Easy_Logger.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Easy_Logger_Parser.Pages;

public partial class Index : ComponentBase
{
	private readonly DataModel InputModel = new();
	private readonly FilterModel ViewModel = new();

	private readonly TooltipOptions TooltipMode = TooltipOptions.Right | TooltipOptions.HasArrow | TooltipOptions.Multiline;

	private async Task AddFile(InputFileChangeEventArgs args)
	{
		var file = args.GetMultipleFiles(1).FirstOrDefault();

		if (file == null)
			return;

		var buffer = new byte[file.Size];
		var max = 10 * 1_048_576;

		await file.OpenReadStream(max).ReadAsync(buffer);

		InputModel.LogFileData = System.Text.Encoding.UTF8.GetString(buffer);
		TryParseLogFileData(InputModel.LogFileData);
	}

	private void OnLogFileDataChanged(ChangeEventArgs args)
	{
		var changed = args.Value?.ToString();

		if (string.IsNullOrWhiteSpace(changed) == false)
			TryParseLogFileData(changed);
	}

	private bool TryParseLogFileData(string data)
	{
		try
		{
			InputModel.LogEntries = JsonSerializer.Deserialize<IEnumerable<LoggerEntryDeserializer>>(data)?.Cast<ILoggerEntry>().ToList();

			if (InputModel.LogEntries != null)
			{
				InputModel.LogSources = InputModel.LogEntries.Where(x => string.IsNullOrWhiteSpace(x.Source) == false).Select(x => x.Source!).Distinct();

				ViewModel.Start = InputModel.LogEntries.Min(x => x.Timestamp);
				ViewModel.End = InputModel.LogEntries.Max(x => x.Timestamp);
				ViewModel.SelectedLogLevels = LogLevelFlagged.None;

				foreach (var level in InputModel.LogEntries.Select(x => x.Severity).Distinct())
					ViewModel.SelectedLogLevels |= StandardToFlagged[level];
			}

			return true;
		}
		catch (JsonException)
		{
			if (data.StartsWith('[') == false && data.EndsWith(']') == false)
				return TryParseLogFileData($"[{data}]");
			else
				return false;
		}
		catch
		{
			return false;
		}
	}

	private List<ILoggerEntry> GetDisplayLogEntries()
	{
        var entries = InputModel.LogEntries?.AsEnumerable();

        if (entries == null)
			return [];

		if (ViewModel.Start != null)
			entries = entries.Where(x => x.Timestamp >= ViewModel.Start.Value);

		if (ViewModel.End != null)
            entries = entries.Where(x => x.Timestamp <= ViewModel.End.Value);

		if (ViewModel.EventNumber != null)
			entries = entries.Where(x => x.Id != null && x.Id.Value.Id == ViewModel.EventNumber.Value);

		if (ViewModel.SelectedLogLevels != LogLevelFlagged.None)
		{
			var levels = new List<LogLevel>();

            foreach (var flag in Enum.GetValues<LogLevelFlagged>())
				if ((ViewModel.SelectedLogLevels & flag) != 0)
					levels.Add(FlaggedToStandard[flag]);

			entries = entries.Where(x => levels.Contains(x.Severity));
        }

		if (string.IsNullOrWhiteSpace(ViewModel.Source) == false)
			entries = entries.Where(x => string.Equals(x.Source, ViewModel.Source, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(ViewModel.EventName) == false)
            entries = entries.Where(x => x.Id != null && string.Equals(x.Id.Value.Name, ViewModel.EventName, StringComparison.OrdinalIgnoreCase));

		if (string.IsNullOrWhiteSpace(ViewModel.SearchMessage) == false)
			entries = entries.Where(x => x.Message.Contains(ViewModel.SearchMessage, StringComparison.OrdinalIgnoreCase));

        return entries.OrderByDescending(x => x.Timestamp).ToList();
	}

	private class DataModel
	{
		[Display(Name = "Log File Data", Description = "Contains the JSON from the logs to parse")]
		public string? LogFileData { get; set; }

		public List<ILoggerEntry>? LogEntries { get; set; }
		public IEnumerable<string> LogSources { get; set; } = [];
	}

	private class FilterModel
	{
		[Display(Name = "Start", Description = "Filters to log entries created at or after the specified time")]
		public DateTime? Start { get; set; }

		[Display(Name = "End", Description = "Filters to log entries created at or before the specified time")]
		public DateTime? End { get; set; }

		[Display(Name = "Source", Description = "Filters to log entries with a matching source value")]
		public string? Source { get; set; }

		[Display(Name = "Log Levels", Description = "Filters to log entries with a level matching one of the selected options")]
		public LogLevelFlagged SelectedLogLevels { get; set; } = LogLevelFlagged.None;

		[Display(Name = "Event Id", Description = "Filters to log entries with a matching id value")]
		public int? EventNumber { get; set; }

		[Display(Name = "Event Name", Description = "Filters to log entries with a matching name value")]
		public string? EventName { get; set; }

		[Display(Name = "Message Text", Description = "Filters to log entries containing the provided text")]
		public string? SearchMessage { get; set; }
	}

	[Flags]
	private enum LogLevelFlagged
	{
		None = 0b_00000000_00000000_00000000_00000000,
        Trace = 0b_00000000_00000000_00000000_00000001,
        Debug = 0b_00000000_00000000_00000000_00000010,
        Information = 0b_00000000_00000000_00000000_00000100,
        Warning = 0b_00000000_00000000_00000000_00001000,
        Error = 0b_00000000_00000000_00000000_00010000,
        Critical = 0b_00000000_00000000_00000000_00100000
    }

	private static readonly Dictionary<LogLevelFlagged, LogLevel> FlaggedToStandard = new()
	{
        [LogLevelFlagged.None] = LogLevel.None,
        [LogLevelFlagged.Trace] = LogLevel.Trace,
		[LogLevelFlagged.Debug] = LogLevel.Debug,
		[LogLevelFlagged.Information] = LogLevel.Information,
		[LogLevelFlagged.Warning] = LogLevel.Warning,
		[LogLevelFlagged.Error] = LogLevel.Error,
		[LogLevelFlagged.Critical] = LogLevel.Critical
	};

	private readonly Dictionary<LogLevel, LogLevelFlagged> StandardToFlagged = new()
	{
        [LogLevel.None] = LogLevelFlagged.None,
        [LogLevel.Trace] = LogLevelFlagged.Trace,
        [LogLevel.Debug] = LogLevelFlagged.Debug,
        [LogLevel.Information] = LogLevelFlagged.Information,
        [LogLevel.Warning] = LogLevelFlagged.Warning,
        [LogLevel.Error] = LogLevelFlagged.Error,
        [LogLevel.Critical] = LogLevelFlagged.Critical
    };
}

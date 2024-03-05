using Easy_Logger.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace Easy_Logger_Parser;

internal class LoggerEntryDeserializer : ILoggerEntry
{
    public LoggerEntryDeserializer()
    {
        Message = string.Empty;
    }

    public LoggerEntryDeserializer(string message)
    {
        Message = message;
    }

    /// <inheritdoc/>
    public DateTime Timestamp { get; set; }

    /// <inheritdoc/>
    public string? Source { get; set; }

    /// <inheritdoc/>
    public string Message { get; set; }

    /// <inheritdoc/>
    public LogLevel Severity { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public EventId? Id => IdDeserializer != null ? new EventId(IdDeserializer.Id, IdDeserializer.Name) : null;

    /// <summary>
    /// Used to deserialize <see cref="Id"/>
    /// </summary>
    [JsonPropertyName("Id")]
    public EventIdDeserializer? IdDeserializer { get; set; }

    internal class EventIdDeserializer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}

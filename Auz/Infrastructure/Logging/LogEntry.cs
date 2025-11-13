using System.Text.Json.Serialization;

namespace Auz.Infrastructure.Logging;

/// <summary>
/// Representa uma entrada de log estruturada
/// </summary>
public class LogEntry
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("duration")]
    public long? Duration { get; set; }

    [JsonPropertyName("exception")]
    public ExceptionDetails? Exception { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; set; }

    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    [JsonPropertyName("service")]
    public string? Service { get; set; }
}

/// <summary>
/// Detalhes de exceção para serialização
/// </summary>
public class ExceptionDetails
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; set; }

    [JsonPropertyName("innerException")]
    public ExceptionDetails? InnerException { get; set; }
}

/// <summary>
/// Modelo para o formato de push do Loki
/// </summary>
public class LokiPushRequest
{
    [JsonPropertyName("streams")]
    public List<LokiStream> Streams { get; set; } = new();
}

/// <summary>
/// Stream do Loki com labels e entries
/// </summary>
public class LokiStream
{
    [JsonPropertyName("stream")]
    public Dictionary<string, string> Stream { get; set; } = new();

    [JsonPropertyName("values")]
    public List<List<string>> Values { get; set; } = new();
}


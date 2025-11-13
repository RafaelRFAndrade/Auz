namespace Auz.Infrastructure.Logging;

/// <summary>
/// Interface para o logger customizado do Grafana Loki
/// </summary>
public interface ILokiLogger
{
    /// <summary>
    /// Registra um log de nível Debug
    /// </summary>
    void LogDebug(string message, Dictionary<string, object>? properties = null, Exception? exception = null);

    /// <summary>
    /// Registra um log de nível Information
    /// </summary>
    void LogInformation(string message, Dictionary<string, object>? properties = null, Exception? exception = null);

    /// <summary>
    /// Registra um log de nível Warning
    /// </summary>
    void LogWarning(string message, Dictionary<string, object>? properties = null, Exception? exception = null);

    /// <summary>
    /// Registra um log de nível Error
    /// </summary>
    void LogError(string message, Dictionary<string, object>? properties = null, Exception? exception = null);

    /// <summary>
    /// Registra um log de nível Critical
    /// </summary>
    void LogCritical(string message, Dictionary<string, object>? properties = null, Exception? exception = null);
}


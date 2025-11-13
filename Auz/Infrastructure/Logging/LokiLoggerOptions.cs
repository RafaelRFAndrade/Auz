namespace Auz.Infrastructure.Logging;

/// <summary>
/// Opções de configuração para o Loki Logger
/// </summary>
public class LokiLoggerOptions
{
    /// <summary>
    /// Habilita ou desabilita o logging para Loki
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Host do servidor Loki
    /// </summary>
    public string Host { get; set; } = "http://localhost";

    /// <summary>
    /// Porta do servidor Loki
    /// </summary>
    public int Port { get; set; } = 3100;

    /// <summary>
    /// Tamanho do lote para envio de logs
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Intervalo em milissegundos para flush automático
    /// </summary>
    public int FlushInterval { get; set; } = 5000;

    /// <summary>
    /// Número de tentativas de retry
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Timeout em milissegundos para requisições HTTP
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Threshold para circuit breaker (número de falhas consecutivas)
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// Labels estáticos para todos os logs
    /// </summary>
    public Dictionary<string, string> Labels { get; set; } = new();

    /// <summary>
    /// Obtém a URL completa do endpoint do Loki
    /// </summary>
    public string GetLokiUrl() => $"{Host}:{Port}/loki/api/v1/push";
}


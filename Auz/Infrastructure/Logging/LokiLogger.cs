using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Auz.Infrastructure.Logging;

/// <summary>
/// Implementação do logger para Grafana Loki com suporte a retry, circuit breaker e background processing
/// </summary>
public class LokiLogger : ILokiLogger, IAsyncDisposable
{
    private readonly LokiLoggerOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentQueue<LogEntry> _logQueue;
    private readonly SemaphoreSlim _flushSemaphore;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage>? _circuitBreakerPolicy;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _backgroundFlushTask;
    private bool _disposed = false;

    // Padrões para sanitização de dados sensíveis
    private static readonly string[] SensitivePatterns = new[]
    {
        "password",
        "pwd",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "bearer",
        "credential",
        "accesskey",
        "secretkey"
    };

    public LokiLogger(
        IOptions<LokiLoggerOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logQueue = new ConcurrentQueue<LogEntry>();
        _flushSemaphore = new SemaphoreSlim(1, 1);
        _cancellationTokenSource = new CancellationTokenSource();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Configurar retry policy com exponential backoff
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: _options.RetryAttempts,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log interno de retry (sem usar o logger para evitar loop)
                    Console.WriteLine($"[LokiLogger] Retry {retryCount}/{_options.RetryAttempts} após {timespan.TotalSeconds}s");
                });

        // Configurar circuit breaker
        if (_options.CircuitBreakerThreshold > 0)
        {
            _circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: _options.CircuitBreakerThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (result, duration) =>
                    {
                        Console.WriteLine($"[LokiLogger] Circuit breaker aberto por {duration.TotalSeconds}s");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("[LokiLogger] Circuit breaker fechado");
                    });
        }

        // Combinar policies
        _retryPolicy = _circuitBreakerPolicy != null
            ? Policy.WrapAsync(_circuitBreakerPolicy, retryPolicy)
            : retryPolicy;

        // Iniciar task de flush em background
        _backgroundFlushTask = Task.Run(BackgroundFlushAsync, _cancellationTokenSource.Token);
    }

    public void LogDebug(string message, Dictionary<string, object>? properties = null, Exception? exception = null)
    {
        EnqueueLog("Debug", message, properties, exception);
    }

    public void LogInformation(string message, Dictionary<string, object>? properties = null, Exception? exception = null)
    {
        EnqueueLog("Information", message, properties, exception);
    }

    public void LogWarning(string message, Dictionary<string, object>? properties = null, Exception? exception = null)
    {
        EnqueueLog("Warning", message, properties, exception);
    }

    public void LogError(string message, Dictionary<string, object>? properties = null, Exception? exception = null)
    {
        EnqueueLog("Error", message, properties, exception);
    }

    public void LogCritical(string message, Dictionary<string, object>? properties = null, Exception? exception = null)
    {
        EnqueueLog("Critical", message, properties, exception);
    }

    private void EnqueueLog(string level, string message, Dictionary<string, object>? properties, Exception? exception)
    {
        if (!_options.Enabled || _disposed)
            return;

        try
        {
            var logEntry = CreateLogEntry(level, message, properties, exception);
            _logQueue.Enqueue(logEntry);
        }
        catch (Exception ex)
        {
            // Fallback para console em caso de erro no enqueue
            Console.WriteLine($"[LokiLogger] Erro ao enfileirar log: {ex.Message}");
        }
    }

    private LogEntry CreateLogEntry(string level, string message, Dictionary<string, object>? properties, Exception? exception)
    {
        _options.Labels.TryGetValue("app", out var appName);
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow.ToString("O"),
            Level = level,
            Message = SanitizeMessage(message),
            Hostname = Environment.MachineName,
            Service = appName ?? "unknown"
        };

        // Adicionar propriedades sanitizadas
        if (properties != null && properties.Count > 0)
        {
            logEntry.Properties = SanitizeProperties(properties);
        }

        // Adicionar detalhes da exceção
        if (exception != null)
        {
            logEntry.Exception = CreateExceptionDetails(exception);
        }

        return logEntry;
    }

    private ExceptionDetails CreateExceptionDetails(Exception exception)
    {
        var details = new ExceptionDetails
        {
            Type = exception.GetType().FullName ?? exception.GetType().Name,
            Message = SanitizeMessage(exception.Message),
            StackTrace = exception.StackTrace
        };

        if (exception.InnerException != null)
        {
            details.InnerException = CreateExceptionDetails(exception.InnerException);
        }

        return details;
    }

    private string SanitizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return message;

        var sanitized = message;
        foreach (var pattern in SensitivePatterns)
        {
            // Substituir valores sensíveis por [REDACTED]
            var regex = new System.Text.RegularExpressions.Regex(
                $@"({pattern}\s*[:=]\s*)([^\s""',;}}]+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            sanitized = regex.Replace(sanitized, "$1[REDACTED]");
        }

        return sanitized;
    }

    private Dictionary<string, object> SanitizeProperties(Dictionary<string, object> properties)
    {
        var sanitized = new Dictionary<string, object>(properties.Count);

        foreach (var kvp in properties)
        {
            var key = kvp.Key.ToLowerInvariant();
            var isSensitive = SensitivePatterns.Any(pattern => key.Contains(pattern, StringComparison.OrdinalIgnoreCase));

            if (isSensitive)
            {
                sanitized[kvp.Key] = "[REDACTED]";
            }
            else if (kvp.Value is string strValue)
            {
                sanitized[kvp.Key] = SanitizeMessage(strValue);
            }
            else if (kvp.Value is Dictionary<string, object> dictValue)
            {
                sanitized[kvp.Key] = SanitizeProperties(dictValue);
            }
            else
            {
                sanitized[kvp.Key] = kvp.Value;
            }
        }

        return sanitized;
    }

    private async Task BackgroundFlushAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_options.FlushInterval, _cancellationTokenSource.Token);
                await FlushLogsAsync();
            }
            catch (OperationCanceledException)
            {
                // Esperado quando o token é cancelado
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LokiLogger] Erro no background flush: {ex.Message}");
            }
        }

        // Flush final ao encerrar
        await FlushLogsAsync();
    }

    public async Task FlushLogsAsync()
    {
        if (!_options.Enabled || _logQueue.IsEmpty)
            return;

        await _flushSemaphore.WaitAsync();
        try
        {
            var logsToSend = new List<LogEntry>();
            var count = 0;

            // Coletar logs do batch
            while (count < _options.BatchSize && _logQueue.TryDequeue(out var logEntry))
            {
                logsToSend.Add(logEntry);
                count++;
            }

            if (logsToSend.Count > 0)
            {
                await SendLogsToLokiAsync(logsToSend);
            }
        }
        finally
        {
            _flushSemaphore.Release();
        }
    }

    private async Task SendLogsToLokiAsync(List<LogEntry> logs)
    {
        if (!_options.Enabled || logs.Count == 0)
            return;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("LokiLogger");
            httpClient.Timeout = TimeSpan.FromMilliseconds(_options.TimeoutMs);

            // Agrupar logs por labels (streams)
            var streams = GroupLogsByStream(logs);

            var pushRequest = new LokiPushRequest
            {
                Streams = streams
            };

            // Aplicar retry policy e circuit breaker
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await httpClient.PostAsJsonAsync(
                    _options.GetLokiUrl(),
                    pushRequest,
                    _jsonOptions,
                    cancellationToken: CancellationToken.None);
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[LokiLogger] Falha ao enviar logs: {response.StatusCode} - {errorContent}");
            }
        }
        catch (BrokenCircuitException)
        {
            // Circuit breaker está aberto, reenfileirar logs
            foreach (var log in logs)
            {
                _logQueue.Enqueue(log);
            }
            Console.WriteLine("[LokiLogger] Circuit breaker aberto, logs reenfileirados");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LokiLogger] Erro ao enviar logs: {ex.Message}");
            // Reenfileirar logs em caso de erro
            foreach (var log in logs)
            {
                _logQueue.Enqueue(log);
            }
        }
    }

    private List<LokiStream> GroupLogsByStream(List<LogEntry> logs)
    {
        var streams = new Dictionary<string, LokiStream>();

        foreach (var log in logs)
        {
            // Criar chave única baseada nos labels
            var streamKey = CreateStreamKey(log);

            if (!streams.TryGetValue(streamKey, out var stream))
            {
                stream = new LokiStream
                {
                    Stream = CreateStreamLabels(log)
                };
                streams[streamKey] = stream;
            }

            // Converter timestamp para nanosegundos (Unix timestamp em nanosegundos)
            var timestamp = DateTimeOffset.Parse(log.Timestamp).ToUnixTimeMilliseconds() * 1_000_000;
            var logJson = JsonSerializer.Serialize(log, _jsonOptions);

            stream.Values.Add(new List<string>
            {
                timestamp.ToString(),
                logJson
            });
        }

        return streams.Values.ToList();
    }

    private string CreateStreamKey(LogEntry log)
    {
        var labels = CreateStreamLabels(log);
        return string.Join("|", labels.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    private Dictionary<string, string> CreateStreamLabels(LogEntry log)
    {
        var labels = new Dictionary<string, string>(_options.Labels);

        // Adicionar labels dinâmicos
        labels["level"] = log.Level.ToLowerInvariant();
        labels["hostname"] = log.Hostname ?? Environment.MachineName;
        _options.Labels.TryGetValue("app", out var appName);
        labels["service"] = log.Service ?? appName ?? "unknown";

        if (!string.IsNullOrEmpty(log.CorrelationId))
        {
            labels["correlationId"] = log.CorrelationId;
        }

        return labels;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Cancelar task de background
        _cancellationTokenSource.Cancel();

        try
        {
            // Aguardar task finalizar (com timeout)
            await Task.WhenAny(
                _backgroundFlushTask,
                Task.Delay(TimeSpan.FromSeconds(10)));
        }
        catch
        {
            // Ignorar erros no dispose
        }

        // Flush final
        await FlushLogsAsync();

        _cancellationTokenSource.Dispose();
        _flushSemaphore.Dispose();
    }
}


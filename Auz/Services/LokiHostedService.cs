using Auz.Infrastructure.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auz.Services;

/// <summary>
/// Hosted Service para garantir inicialização e finalização graceful do Loki Logger
/// </summary>
public class LokiHostedService : IHostedService
{
    private readonly LokiLogger _lokiLogger;
    private readonly ILogger<LokiHostedService> _logger;

    public LokiHostedService(LokiLogger lokiLogger, ILogger<LokiHostedService> logger)
    {
        _lokiLogger = lokiLogger;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loki Hosted Service iniciado");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loki Hosted Service finalizando, fazendo flush dos logs...");
        
        try
        {
            await _lokiLogger.FlushLogsAsync();
            _logger.LogInformation("Loki Hosted Service finalizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao finalizar Loki Hosted Service");
        }
    }
}


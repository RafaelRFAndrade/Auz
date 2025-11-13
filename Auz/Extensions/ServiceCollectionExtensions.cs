using Auz.Infrastructure.Logging;
using Auz.Middleware;
using Auz.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auz.Extensions;

/// <summary>
/// Extensões para registro de serviços do Loki Logger
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona o Loki Logger ao container de DI
    /// </summary>
    public static IServiceCollection AddLokiLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar opções
        services.Configure<LokiLoggerOptions>(configuration);

        // Registrar HttpClient
        services.AddHttpClient("LokiLogger", client =>
        {
            var options = configuration.Get<LokiLoggerOptions>() ?? new LokiLoggerOptions();
            client.Timeout = TimeSpan.FromMilliseconds(options.TimeoutMs);
            client.BaseAddress = new Uri($"{options.Host}:{options.Port}");
        });

        // Registrar logger como singleton
        services.AddSingleton<LokiLogger>();
        services.AddSingleton<ILokiLogger>(sp => sp.GetRequiredService<LokiLogger>());

        // Registrar hosted service
        services.AddHostedService<LokiHostedService>();

        return services;
    }
}


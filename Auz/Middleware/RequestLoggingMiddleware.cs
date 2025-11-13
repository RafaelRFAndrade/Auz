using System.Diagnostics;
using Auz.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Auz.Middleware;

/// <summary>
/// Middleware para capturar automaticamente informações de requisições HTTP
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILokiLogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILokiLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.TraceIdentifier;
        var userId = GetUserId(context);

        // Adicionar correlation ID ao contexto para uso em outros lugares
        context.Items["CorrelationId"] = correlationId;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log de erro com todas as informações da requisição
            var errorProperties = CreateRequestProperties(context, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
            errorProperties["correlationId"] = correlationId;
            if (!string.IsNullOrEmpty(userId))
            {
                errorProperties["userId"] = userId;
            }

            _logger.LogError(
                $"Erro ao processar requisição: {ex.Message}",
                errorProperties,
                ex);

            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Log da requisição (apenas para métodos importantes e erros)
            if (ShouldLogRequest(context))
            {
                var properties = CreateRequestProperties(context, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
                properties["correlationId"] = correlationId;
                if (!string.IsNullOrEmpty(userId))
                {
                    properties["userId"] = userId;
                }

                var logLevel = DetermineLogLevel(context.Response.StatusCode);
                var message = $"HTTP {context.Request.Method} {context.Request.Path} - {context.Response.StatusCode}";

                switch (logLevel)
                {
                    case "Error":
                        _logger.LogError(message, properties);
                        break;
                    case "Warning":
                        _logger.LogWarning(message, properties);
                        break;
                    default:
                        _logger.LogInformation(message, properties);
                        break;
                }
            }
        }
    }

    private Dictionary<string, object> CreateRequestProperties(HttpContext context, long duration, int statusCode)
    {
        var properties = new Dictionary<string, object>
        {
            ["method"] = context.Request.Method,
            ["path"] = context.Request.Path + context.Request.QueryString,
            ["statusCode"] = statusCode,
            ["duration"] = duration,
            ["userAgent"] = context.Request.Headers["User-Agent"].ToString(),
            ["ipAddress"] = GetClientIpAddress(context)
        };

        // Adicionar query parameters (sanitizados)
        if (context.Request.Query.Count > 0)
        {
            var queryParams = new Dictionary<string, object>();
            foreach (var kvp in context.Request.Query)
            {
                queryParams[kvp.Key] = kvp.Value.ToString();
            }
            properties["queryParameters"] = queryParams;
        }

        return properties;
    }

    private string? GetUserId(HttpContext context)
    {
        // Tentar obter do claim de usuário
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier) 
            ?? context.User?.FindFirst("sub")
            ?? context.User?.FindFirst("userId");

        return userIdClaim?.Value;
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Tentar obter IP real (considerando proxies)
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return ip.Split(',')[0].Trim();
    }

    private bool ShouldLogRequest(HttpContext context)
    {
        // Logar todas as requisições que não sejam health checks ou swagger
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        
        return !path.Contains("/swagger")
            && !path.Contains("/health")
            && !path.Contains("/favicon.ico");
    }

    private string DetermineLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => "Error",
            >= 400 => "Warning",
            _ => "Information"
        };
    }
}


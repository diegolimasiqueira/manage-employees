using System.Diagnostics;

namespace ManageEmployees.API.Middlewares;

/// <summary>
/// Middleware para logging de todas as requisições HTTP
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        
        // Capturar informações da requisição
        var method = request.Method;
        var path = request.Path;
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
        var userEmail = context.User?.Identity?.Name ?? "Anonymous";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        try
        {
            // Executar próximo middleware
            await _next(context);
            
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            // Log da requisição com nível baseado no status code
            if (statusCode >= 500)
            {
                _logger.LogError(
                    "HTTP {Method} {Path}{QueryString} respondeu {StatusCode} em {Elapsed}ms | User: {User} | IP: {IP}",
                    method, path, queryString, statusCode, elapsed, userEmail, ipAddress);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "HTTP {Method} {Path}{QueryString} respondeu {StatusCode} em {Elapsed}ms | User: {User} | IP: {IP}",
                    method, path, queryString, statusCode, elapsed, userEmail, ipAddress);
            }
            else
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path}{QueryString} respondeu {StatusCode} em {Elapsed}ms | User: {User} | IP: {IP}",
                    method, path, queryString, statusCode, elapsed, userEmail, ipAddress);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;
            
            _logger.LogError(ex,
                "HTTP {Method} {Path}{QueryString} falhou com exceção após {Elapsed}ms | User: {User} | IP: {IP}",
                method, path, queryString, elapsed, userEmail, ipAddress);
            
            throw;
        }
    }
}


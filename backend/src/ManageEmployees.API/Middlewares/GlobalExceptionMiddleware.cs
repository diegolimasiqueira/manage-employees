using System.Net;
using System.Text.Json;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Domain.Exceptions;

namespace ManageEmployees.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var userEmail = context.User?.Identity?.Name ?? "Anonymous";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var (statusCode, response) = exception switch
        {
            Domain.Exceptions.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(validationEx.Message, validationEx.Errors)
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse.Fail(notFoundEx.Message)
            ),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ApiResponse.Fail(unauthorizedEx.Message)
            ),
            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse.Fail(forbiddenEx.Message)
            ),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                ApiResponse.Fail(conflictEx.Message)
            ),
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(domainEx.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                _env.IsDevelopment() 
                    ? ApiResponse.Fail($"[DEV] {exception.GetType().Name}: {exception.Message}")
                    : ApiResponse.Fail("Ocorreu um erro interno no servidor.")
            )
        };
        
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, 
                "Erro não tratado em {Method} {Path} | User: {User} | IP: {IP} | Exception: {ExceptionType} | Message: {Message}",
                method, path, userEmail, ipAddress, exception.GetType().Name, exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "Exceção de domínio: {ExceptionType} em {Method} {Path} | User: {User} | IP: {IP} | Message: {Message}", 
                exception.GetType().Name, method, path, userEmail, ipAddress, exception.Message);
        }
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

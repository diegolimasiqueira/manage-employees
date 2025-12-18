using System.Net;
using System.Text.Json;
using FluentAssertions;
using ManageEmployees.API.Middlewares;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ManageEmployees.Tests.API.Middlewares;

public class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task HandlesValidationException()
    {
        var errors = new Dictionary<string, string[]> { { "Field", new[] { "Error" } } };
        var (status, response) = await InvokeAsync(new ValidationException(errors), isDevelopment: true);

        status.Should().Be((int)HttpStatusCode.BadRequest);
        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response, SerializerOptions)!;
        apiResponse.Message.Should().Be("Ocorreram erros de validação.");
        apiResponse.Errors.Should().ContainKey("Field");
    }

    public static IEnumerable<object[]> DomainExceptionData =>
    [
        new object[] { new NotFoundException("entity", Guid.NewGuid()), (int)HttpStatusCode.NotFound },
        new object[] { new UnauthorizedException("message"), (int)HttpStatusCode.Unauthorized },
        new object[] { new ForbiddenException("message"), (int)HttpStatusCode.Forbidden },
        new object[] { new ConflictException("conflict"), (int)HttpStatusCode.Conflict },
        new object[] { new DomainException("message"), (int)HttpStatusCode.BadRequest }
    ];

    [Theory]
    [MemberData(nameof(DomainExceptionData))]
    public async Task HandlesDomainExceptions(Exception exception, int expectedStatus)
    {
        var (status, response) = await InvokeAsync(exception, isDevelopment: false);

        status.Should().Be(expectedStatus);
        JsonSerializer.Deserialize<ApiResponse>(response, SerializerOptions)!.Message.Should().Be(exception.Message);
    }

    [Fact]
    public async Task HandlesUnknownExceptionAsInternalServerError()
    {
        var (status, response) = await InvokeAsync(new Exception("boom"), isDevelopment: false);

        status.Should().Be((int)HttpStatusCode.InternalServerError);
        response.Should().Contain("erro interno");
    }

    [Fact]
    public async Task HandlesUnknownExceptionInDevelopment_ReturnsDetailedMessage()
    {
        var (status, response) = await InvokeAsync(new Exception("boom"), isDevelopment: true);

        status.Should().Be((int)HttpStatusCode.InternalServerError);
        response.Should().Contain("[DEV]");
    }

    [Fact]
    public void UseGlobalExceptionMiddleware_AddsMiddleware()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var app = new ApplicationBuilder(services.BuildServiceProvider());

        var returned = GlobalExceptionMiddlewareExtensions.UseGlobalExceptionMiddleware(app);

        returned.Should().BeSameAs(app);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNext()
    {
        var middleware = new GlobalExceptionMiddleware(_ => Task.CompletedTask,
            NullLogger<GlobalExceptionMiddleware>.Instance, BuildEnv(isDevelopment: false));
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    private static async Task<(int Status, string Body)> InvokeAsync(Exception exception, bool isDevelopment)
    {
        var middleware = new GlobalExceptionMiddleware(_ => throw exception, NullLogger<GlobalExceptionMiddleware>.Instance, BuildEnv(isDevelopment));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return (context.Response.StatusCode, await reader.ReadToEndAsync());
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static IHostEnvironment BuildEnv(bool isDevelopment) =>
        new TestEnv(isDevelopment ? Environments.Development : Environments.Production);

    private sealed class TestEnv : IHostEnvironment
    {
        public TestEnv(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

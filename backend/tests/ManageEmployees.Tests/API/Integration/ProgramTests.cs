using FluentAssertions;
using ManageEmployees.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace ManageEmployees.Tests.API.Integration;

public class ProgramTests
{
    [Fact]
    public async Task Application_StartsAndServesSwagger()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Manage Employees API");
    }

    [Fact]
    public void Application_WhenMigrationFails_Throws()
    {
        using var factory = new FailingMigrationFactory();
        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }

    private sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(cfg =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:DefaultConnection", "DataSource=:memory:"},
                    {"JwtSettings:SecretKey", "ThisIsASecretKeyForTests123456"},
                    {"JwtSettings:Issuer", "TestIssuer"},
                    {"JwtSettings:Audience", "TestAudience"}
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.RemoveAll<ApplicationDbContext>();
                _connection.Open();
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Dispose();
        }
    }

    private sealed class FailingMigrationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(cfg =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:DefaultConnection", "Host=invalid;Database=invalid;Username=user;Password=pass"},
                    {"JwtSettings:SecretKey", "ThisIsASecretKeyForTests123456"},
                    {"JwtSettings:Issuer", "TestIssuer"},
                    {"JwtSettings:Audience", "TestAudience"}
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql("Host=invalid;Database=invalid;Username=user;Password=pass"));
            });
        }
    }
}

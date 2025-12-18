using System.Text;
using FluentAssertions;
using ManageEmployees.API.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ManageEmployees.Tests.API.Extensions;

public class AuthExtensionsTests
{
    [Fact]
    public async Task AddJwtAuthentication_ConfiguresBearerOptionsAndEvents()
    {
        var settings = new Dictionary<string, string?>
        {
            {"JwtSettings:SecretKey", Convert.ToBase64String(Encoding.UTF8.GetBytes("supersecretkeysupersecretkey"))},
            {"JwtSettings:Issuer", "Issuer"},
            {"JwtSettings:Audience", "Audience"}
        };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJwtAuthentication(new ConfigurationBuilder().AddInMemoryCollection(settings!).Build());

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        options.TokenValidationParameters.ValidIssuer.Should().Be("Issuer");
        options.TokenValidationParameters.ValidAudience.Should().Be("Audience");
        options.TokenValidationParameters.IssuerSigningKey.Should().BeOfType<SymmetricSecurityKey>();

        var scheme = new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler));
        var expiredContext = new AuthenticationFailedContext(new DefaultHttpContext(), scheme, options)
        {
            Exception = new SecurityTokenExpiredException()
        };

        await options.Events!.OnAuthenticationFailed!(expiredContext);
        expiredContext.Response.Headers.Should().ContainKey("Token-Expired");

        var generalContext = new AuthenticationFailedContext(new DefaultHttpContext(), scheme, options)
        {
            Exception = new Exception()
        };

        await options.Events!.OnAuthenticationFailed!(generalContext);
        generalContext.Response.Headers.Should().NotContainKey("Token-Expired");
    }

    [Fact]
    public void AddJwtAuthentication_ThrowsIfSecretMissing()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddJwtAuthentication(configuration));
    }
}

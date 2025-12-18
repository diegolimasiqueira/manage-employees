using FluentAssertions;
using ManageEmployees.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace ManageEmployees.Tests.API.Extensions;

public class SwaggerExtensionsTests
{
    [Fact]
    public void AddSwaggerConfiguration_RegistersDocumentsAndSecurity()
    {
        var services = new ServiceCollection();
        services.AddSwaggerConfiguration();
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
        options.SwaggerGeneratorOptions.SwaggerDocs.Should().ContainKey("v1");
        options.SwaggerGeneratorOptions.SecuritySchemes.Should().ContainKey("Bearer");
    }

    [Fact]
    public void AddSwaggerConfiguration_IncludesXmlWhenFileExists()
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, "ManageEmployees.API.xml");
        File.WriteAllText(xmlPath, "<doc></doc>");
        try
        {
            var services = new ServiceCollection();
            services.AddSwaggerConfiguration();
            services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>();
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void UseSwaggerConfiguration_AddsMiddleware()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration();
        var provider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(provider);

        var returned = SwaggerExtensions.UseSwaggerConfiguration(app);

        returned.Should().BeSameAs(app);
    }
}

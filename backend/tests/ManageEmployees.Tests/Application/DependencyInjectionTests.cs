using FluentAssertions;
using ManageEmployees.Application;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Application.Validators;
using ManageEmployees.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ManageEmployees.Tests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersServicesAndValidators()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Mock.Of<IUnitOfWork>());
        services.AddSingleton(Mock.Of<IPasswordService>());
        services.AddSingleton(Mock.Of<IJwtTokenService>());

        services.AddApplication();
        var provider = services.BuildServiceProvider();

        provider.GetService<IAuthService>().Should().NotBeNull();
        provider.GetService<LoginValidator>().Should().NotBeNull();
    }
}

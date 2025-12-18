using FluentAssertions;
using ManageEmployees.Infrastructure.Services;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Services;

public class PasswordServiceTests
{
    [Fact]
    public void HashAndVerify_WorksForValidAndInvalidPasswords()
    {
        var service = new PasswordService();

        var hash = service.HashPassword("Strong@123");

        hash.Should().NotBeNullOrWhiteSpace();
        service.VerifyPassword("Strong@123", hash).Should().BeTrue();
        service.VerifyPassword("WrongPass", hash).Should().BeFalse();
    }
}

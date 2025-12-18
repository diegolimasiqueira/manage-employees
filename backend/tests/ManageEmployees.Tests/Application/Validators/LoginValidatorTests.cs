using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Validators;
using Xunit;

namespace ManageEmployees.Tests.Application.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Validate_ReturnsValidForProperData()
    {
        var request = new LoginRequest { Email = "user@test.com", Password = "123" };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsErrorsForInvalidData()
    {
        var request = new LoginRequest { Email = "", Password = "" };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Password));
    }
}

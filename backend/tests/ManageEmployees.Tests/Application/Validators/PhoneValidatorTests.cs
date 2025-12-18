using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Validators;
using Xunit;

namespace ManageEmployees.Tests.Application.Validators;

public class PhoneValidatorTests
{
    private readonly PhoneValidator _validator = new();

    [Fact]
    public void Validate_ReturnsValidForProperPhone()
    {
        var phone = new PhoneDto { Number = "+55 11 99999-9999", Type = "Mobile" };

        var result = _validator.Validate(phone);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsErrorsForInvalidPhone()
    {
        var phone = new PhoneDto { Number = "abc", Type = "Fax" };

        var result = _validator.Validate(phone);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PhoneDto.Number));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PhoneDto.Type));
    }
}

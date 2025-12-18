using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Validators;
using Xunit;

namespace ManageEmployees.Tests.Application.Validators;

public class RegisterDirectorValidatorTests
{
    private readonly RegisterFirstDirectorValidator _validator = new();

    [Fact]
    public void Validate_ReturnsValidForProperData()
    {
        var request = new RegisterFirstDirectorRequest
        {
            Name = "Lucas Pereira",
            Email = "lucas@empresa.com",
            DocumentNumber = "12345678900",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-35),
            Phones = [new PhoneDto { Number = "(11) 4444-4444", Type = "Work" }]
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsErrorsForInvalidData()
    {
        var request = new RegisterFirstDirectorRequest
        {
            Name = "",
            Email = "invalid",
            DocumentNumber = "12",
            Password = "weak",
            ConfirmPassword = "different",
            BirthDate = DateTime.Today.AddYears(-10),
            Phones = []
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterFirstDirectorRequest.ConfirmPassword));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterFirstDirectorRequest.BirthDate));
    }

    [Fact]
    public void Validate_FailsForBirthdayNotReachedYet()
    {
        var request = new RegisterFirstDirectorRequest
        {
            Name = "User",
            Email = "user@test.com",
            DocumentNumber = "12345678900",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-18).AddDays(1),
            Phones = [new PhoneDto { Number = "12345678", Type = "Mobile" }]
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}

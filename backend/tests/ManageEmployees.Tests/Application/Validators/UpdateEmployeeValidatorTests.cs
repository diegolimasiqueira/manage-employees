using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Validators;
using Xunit;

namespace ManageEmployees.Tests.Application.Validators;

public class UpdateEmployeeValidatorTests
{
    private readonly UpdateEmployeeValidator _validator = new();

    [Fact]
    public void Validate_ReturnsValidForProperData()
    {
        var request = new UpdateEmployeeRequest
        {
            Name = "Ana Souza",
            Email = "ana@exemplo.com",
            DocumentNumber = "12345678900",
            RoleId = Guid.NewGuid(),
            BirthDate = DateTime.Today.AddYears(-22),
            Phones = [new PhoneDto { Number = "(11) 3333-3333", Type = "Home" }]
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsErrorsForInvalidData()
    {
        var request = new UpdateEmployeeRequest
        {
            Name = "",
            Email = "invalid",
            DocumentNumber = "short",
            RoleId = Guid.Empty,
            BirthDate = DateTime.Today.AddYears(-15),
            Phones = []
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_FailsWhenBirthdayHasNotOccurredYet()
    {
        var request = new UpdateEmployeeRequest
        {
            Name = "Ana Souza",
            Email = "ana@exemplo.com",
            DocumentNumber = "12345678900",
            RoleId = Guid.NewGuid(),
            BirthDate = DateTime.Today.AddYears(-18).AddDays(1),
            Phones = [new PhoneDto { Number = "(11) 3333-3333", Type = "Home" }]
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.BirthDate));
    }
}

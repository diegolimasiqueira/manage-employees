using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Validators;
using Xunit;

namespace ManageEmployees.Tests.Application.Validators;

public class CreateEmployeeValidatorTests
{
    private readonly CreateEmployeeValidator _validator = new();

    [Fact]
    public void Validate_ReturnsValidForProperData()
    {
        var request = BuildRequest();

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsErrorsForInvalidData()
    {
        var request = new CreateEmployeeRequest
        {
            Name = string.Empty,
            Email = "invalid",
            DocumentNumber = "123",
            Password = "weak",
            BirthDate = DateTime.Today.AddYears(-10),
            RoleId = Guid.Empty,
            Phones = new List<PhoneDto> { new() { Number = "abc", Type = "Unknown" } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmployeeRequest.RoleId));
        result.Errors.Should().Contain(e => e.PropertyName == "Phones[0].Type");
    }

    [Fact]
    public void Validate_FailsWhenBirthdayNotReachedInYear()
    {
        var request = BuildRequest() with { BirthDate = DateTime.Today.AddYears(-18).AddDays(1) };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmployeeRequest.BirthDate));
    }

    private static CreateEmployeeRequest BuildRequest()
    {
        return new CreateEmployeeRequest
        {
            Name = "Maria Silva",
            Email = "maria@exemplo.com",
            DocumentNumber = "12345678900",
            Password = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-25),
            RoleId = Guid.NewGuid(),
            Phones =
            [
                new PhoneDto { Number = "+55 11 99999-9999", Type = "Mobile" },
                new PhoneDto { Number = "(11) 2222-2222", Type = "Home" }
            ]
        };
    }
}

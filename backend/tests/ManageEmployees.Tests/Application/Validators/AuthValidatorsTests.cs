using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Validators;
using Xunit;

namespace ManageEmployees.Tests.Application.Validators;

public class AuthValidatorsTests
{
    [Fact]
    public void SelfRegisterValidator_ValidatesSuccess()
    {
        var validator = new SelfRegisterValidator();
        var request = new SelfRegisterRequest
        {
            Name = "Bruno Silva",
            Email = "bruno@test.com",
            DocumentNumber = "98765432100",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-25),
            RoleId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            Phones = [new PhoneDto { Number = "11999999999", Type = "Mobile" }]
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SelfRegisterValidator_FailsOnYoungUserOrPasswordMismatch()
    {
        var validator = new SelfRegisterValidator();
        var request = new SelfRegisterRequest
        {
            Name = "X",
            Email = "invalid",
            DocumentNumber = "1",
            Password = "123",
            ConfirmPassword = "456",
            BirthDate = DateTime.Today.AddYears(-17),
            RoleId = Guid.Empty,
            Phones = []
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SelfRegisterRequest.ConfirmPassword));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SelfRegisterRequest.RoleId));
    }

    [Fact]
    public void SelfRegisterValidator_FailsWhenBirthdayNotReachedYet()
    {
        var validator = new SelfRegisterValidator();
        var request = new SelfRegisterRequest
        {
            Name = "Bruno Silva",
            Email = "bruno@test.com",
            DocumentNumber = "98765432100",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-18).AddDays(1),
            RoleId = Guid.NewGuid(),
            Phones = [new PhoneDto { Number = "11999999999", Type = "Mobile" }]
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ChangePasswordValidator_CoversRequiredRules()
    {
        var validator = new ChangePasswordValidator();
        var valid = new ChangePasswordRequest
        {
            CurrentPassword = "Old@123",
            NewPassword = "New@123",
            ConfirmPassword = "New@123"
        };

        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = valid with { NewPassword = "short", ConfirmPassword = "mismatch", CurrentPassword = "" };
        var invalidResult = validator.Validate(invalid);

        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.CurrentPassword));
        invalidResult.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }
}

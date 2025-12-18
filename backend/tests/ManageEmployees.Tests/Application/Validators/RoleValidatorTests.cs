using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Validators;
using Xunit;

namespace ManageEmployees.Tests.Application.Validators;

public class RoleValidatorTests
{
    [Fact]
    public void CreateRoleValidator_ValidatesProperly()
    {
        var validator = new CreateRoleValidator();
        var request = new CreateRoleRequest
        {
            Name = "Leader",
            Description = "Leads team",
            HierarchyLevel = 50,
            CanApproveRegistrations = true,
            CanCreateEmployees = true,
            CanDeleteEmployees = false,
            CanEditEmployees = true,
            CanManageRoles = false
        };

        validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateRoleValidator_FailsOnInvalidLevel()
    {
        var validator = new CreateRoleValidator();
        var request = new CreateRoleRequest { Name = "", HierarchyLevel = 0 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRoleRequest.HierarchyLevel));
    }

    [Fact]
    public void UpdateRoleValidator_UsesSameRules()
    {
        var validator = new UpdateRoleValidator();
        var request = new UpdateRoleRequest
        {
            Name = "Updated",
            Description = new string('x', 10),
            HierarchyLevel = 10,
            CanApproveRegistrations = false,
            CanCreateEmployees = false,
            CanDeleteEmployees = false,
            CanEditEmployees = false,
            CanManageRoles = true
        };

        validator.Validate(request).IsValid.Should().BeTrue();

        var invalid = request with { HierarchyLevel = 101, Name = "" };
        var invalidResult = validator.Validate(invalid);

        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateRoleRequest.HierarchyLevel));
    }
}

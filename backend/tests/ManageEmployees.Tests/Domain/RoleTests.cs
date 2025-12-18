using FluentAssertions;
using ManageEmployees.Domain.Entities;
using Xunit;

namespace ManageEmployees.Tests.Domain;

public class RoleTests
{
    [Fact]
    public void CanCreateEmployeeWithRole_WhenAllowedAndHigherHierarchy_ReturnsTrue()
    {
        var role = new Role { HierarchyLevel = 50, CanCreateEmployees = true };
        var target = new Role { HierarchyLevel = 10 };

        role.CanCreateEmployeeWithRole(target).Should().BeTrue();
    }

    [Fact]
    public void CanCreateEmployeeWithRole_WhenNotAllowedOrLower_ReturnsFalse()
    {
        var role = new Role { HierarchyLevel = 20, CanCreateEmployees = false };
        var higherTarget = new Role { HierarchyLevel = 30 };

        role.CanCreateEmployeeWithRole(higherTarget).Should().BeFalse();
    }

    [Fact]
    public void CanApproveEmployeeWithRole_WhenHierarchySufficient_ReturnsTrue()
    {
        var role = new Role { HierarchyLevel = 30, CanApproveRegistrations = true };
        var target = new Role { HierarchyLevel = 30 };

        role.CanApproveEmployeeWithRole(target).Should().BeTrue();
    }

    [Fact]
    public void CanApproveEmployeeWithRole_WhenHierarchyLower_ReturnsFalse()
    {
        var role = new Role { HierarchyLevel = 5, CanApproveRegistrations = true };
        var target = new Role { HierarchyLevel = 10 };

        role.CanApproveEmployeeWithRole(target).Should().BeFalse();
    }

    [Fact]
    public void CanApproveEmployeeWithRole_WhenApprovalNotAllowed_ReturnsFalse()
    {
        var role = new Role { HierarchyLevel = 100, CanApproveRegistrations = false };
        var target = new Role { HierarchyLevel = 1 };

        role.CanApproveEmployeeWithRole(target).Should().BeFalse();
    }
}

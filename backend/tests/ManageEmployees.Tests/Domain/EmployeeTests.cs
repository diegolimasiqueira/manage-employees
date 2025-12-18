using FluentAssertions;
using ManageEmployees.Domain.Entities;
using Xunit;

namespace ManageEmployees.Tests.Domain;

public class EmployeeTests
{
    [Fact]
    public void GetAge_WhenBirthdayHasPassed_ReturnsFullYears()
    {
        var birthDate = DateTime.UtcNow.AddYears(-30).AddDays(-1);
        var employee = new Employee { BirthDate = birthDate };

        employee.GetAge().Should().Be(30);
    }

    [Fact]
    public void GetAge_WhenBirthdayNotYetOccurred_SubtractsOneYear()
    {
        var birthDate = DateTime.UtcNow.AddYears(-30).AddDays(1);
        var employee = new Employee { BirthDate = birthDate };

        employee.GetAge().Should().Be(29);
    }

    [Fact]
    public void CanCreateEmployeeWithRole_DelegatesToRolePermissions()
    {
        var currentRole = new Role { HierarchyLevel = 50, CanCreateEmployees = true };
        var targetRole = new Role { HierarchyLevel = 10 };
        var employee = new Employee { Role = currentRole };

        employee.CanCreateEmployeeWithRole(targetRole).Should().BeTrue();
    }

    [Fact]
    public void CanCreateEmployeeWithRole_WhenRoleCannotCreate_ReturnsFalse()
    {
        var currentRole = new Role { HierarchyLevel = 50, CanCreateEmployees = false };
        var targetRole = new Role { HierarchyLevel = 10 };
        var employee = new Employee { Role = currentRole };

        employee.CanCreateEmployeeWithRole(targetRole).Should().BeFalse();
    }

    [Fact]
    public void CanApproveEmployee_UsesRoleApprovalRules()
    {
        var approverRole = new Role { HierarchyLevel = 100, CanApproveRegistrations = true };
        var approver = new Employee { Role = approverRole };
        var target = new Employee { Role = new Role { HierarchyLevel = 80 } };

        approver.CanApproveEmployee(target).Should().BeTrue();
    }

    [Fact]
    public void CanApproveEmployee_WhenHierarchyLower_ReturnsFalse()
    {
        var approverRole = new Role { HierarchyLevel = 10, CanApproveRegistrations = true };
        var approver = new Employee { Role = approverRole };
        var target = new Employee { Role = new Role { HierarchyLevel = 20 } };

        approver.CanApproveEmployee(target).Should().BeFalse();
    }
}

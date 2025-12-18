using FluentAssertions;
using ManageEmployees.Domain.Entities;
using Xunit;

namespace ManageEmployees.Tests.Domain;

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_DefaultsAndSetters_WorkAsExpected()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        employee.IsActive.Should().BeTrue();
        employee.Id.Should().NotBeEmpty();
        employee.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        employee.UpdatedAt.Should().NotBeNull();
    }
}

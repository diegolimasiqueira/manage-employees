using FluentAssertions;
using ManageEmployees.Domain.Entities;
using Xunit;

namespace ManageEmployees.Tests.Domain;

public class PhoneTests
{
    [Fact]
    public void PhoneProperties_PersistValues()
    {
        var employeeId = Guid.NewGuid();
        var employee = new Employee { Name = "Test User" };

        var phone = new Phone
        {
            Id = Guid.NewGuid(),
            Number = "123456789",
            Type = "Mobile",
            EmployeeId = employeeId,
            Employee = employee
        };

        phone.Employee.Should().Be(employee);
        phone.EmployeeId.Should().Be(employeeId);
        phone.Number.Should().Be("123456789");
        phone.Type.Should().Be("Mobile");
    }
}

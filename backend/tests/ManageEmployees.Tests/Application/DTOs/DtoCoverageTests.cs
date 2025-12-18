using FluentAssertions;
using ManageEmployees.Application.DTOs;
using Xunit;

namespace ManageEmployees.Tests.Application.DTOs;

public class DtoCoverageTests
{
    [Fact]
    public void RecordDtos_AssignAndExposeProperties()
    {
        var employeeId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var approve = new ApproveEmployeeRequest { EmployeeId = employeeId, Approve = true, RejectionReason = "ok" };
        approve.EmployeeId.Should().Be(employeeId);
        approve.Approve.Should().BeTrue();
        approve.RejectionReason.Should().Be("ok");
        var approveCopy = approve with { Approve = false };
        approveCopy.Approve.Should().BeFalse();

        var createRole = new CreateRoleRequest
        {
            Name = "Role",
            Description = "Desc",
            HierarchyLevel = 10,
            CanApproveRegistrations = true,
            CanCreateEmployees = true,
            CanEditEmployees = false,
            CanDeleteEmployees = false,
            CanManageRoles = true
        };
        createRole.Name.Should().Be("Role");
        createRole.Description.Should().Be("Desc");
        createRole.HierarchyLevel.Should().Be(10);
        createRole.CanApproveRegistrations.Should().BeTrue();
        createRole.CanCreateEmployees.Should().BeTrue();
        createRole.CanEditEmployees.Should().BeFalse();
        createRole.CanDeleteEmployees.Should().BeFalse();
        createRole.CanManageRoles.Should().BeTrue();
        var createRoleCopy = createRole with { Name = "RoleCopy" };
        createRoleCopy.Name.Should().Be("RoleCopy");

        var updateRole = new UpdateRoleRequest
        {
            Name = "Role2",
            Description = "Desc2",
            HierarchyLevel = 9,
            CanApproveRegistrations = false,
            CanCreateEmployees = false,
            CanEditEmployees = true,
            CanDeleteEmployees = true,
            CanManageRoles = false
        };
        updateRole.CanDeleteEmployees.Should().BeTrue();
        updateRole.HierarchyLevel.Should().Be(9);
        var updateRoleCopy = updateRole with { Name = "Role2Copy" };
        updateRoleCopy.Name.Should().Be("Role2Copy");

        var roleDto = new RoleDto
        {
            Id = roleId,
            Name = "Role",
            Description = "Desc",
            HierarchyLevel = 10,
            CanApproveRegistrations = true,
            CanCreateEmployees = true,
            CanEditEmployees = true,
            CanDeleteEmployees = true,
            CanManageRoles = true
        };
        roleDto.Id.Should().Be(roleId);
        roleDto.CanManageRoles.Should().BeTrue();
        roleDto.HierarchyLevel.Should().Be(10);
        var roleDtoCopy = roleDto with { Name = "RoleDtoCopy" };
        roleDtoCopy.Name.Should().Be("RoleDtoCopy");

        var roleSimple = new RoleSimpleDto { Id = roleId, Name = "Simple", HierarchyLevel = 5 };
        roleSimple.Name.Should().Be("Simple");
        roleSimple.HierarchyLevel.Should().Be(5);
        var roleSimpleCopy = roleSimple with { Name = "Clone" };
        roleSimpleCopy.Name.Should().Be("Clone");

        var employeeDto = new EmployeeDto
        {
            Id = employeeId,
            Name = "User",
            Email = "user@test.com",
            DocumentNumber = "123",
            BirthDate = DateTime.UtcNow.Date,
            Age = 30,
            Role = roleSimple,
            ManagerId = managerId,
            ManagerName = "Manager",
            Enabled = true,
            ApprovedAt = DateTime.UtcNow,
            ApprovedByName = "Approver",
            Phones = [new PhoneDto { Number = "1", Type = "Mobile" }],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        employeeDto.ManagerName.Should().Be("Manager");
        employeeDto.Phones.Should().HaveCount(1);
        employeeDto.Email.Should().Contain("@");
        employeeDto.DocumentNumber.Should().Be("123");
        employeeDto.Age.Should().Be(30);
        employeeDto.Enabled.Should().BeTrue();
        var employeeDtoCopy = employeeDto with { Name = "Copy" };
        employeeDtoCopy.Name.Should().Be("Copy");

        var employeeSimple = new EmployeeSimpleDto
        {
            Id = employeeId,
            Name = "Simple",
            Email = "simple@test.com",
            RoleName = "Role",
            Enabled = false
        };
        employeeSimple.Enabled.Should().BeFalse();
        employeeSimple.Email.Should().Be("simple@test.com");
        employeeSimple.RoleName.Should().Be("Role");
        var employeeSimpleCopy = employeeSimple with { Enabled = true };
        employeeSimpleCopy.Enabled.Should().BeTrue();

        var pending = new PendingApprovalDto
        {
            Id = employeeId,
            Name = "Pending",
            Email = "pending@test.com",
            DocumentNumber = "321",
            Role = roleSimple,
            CreatedAt = DateTime.UtcNow
        };
        pending.Role.Name.Should().Be(roleSimple.Name);
        pending.Email.Should().Contain("pending");
        pending.DocumentNumber.Should().Be("321");
        var pendingCopy = pending with { Name = "PendingCopy" };
        pendingCopy.Name.Should().Be("PendingCopy");

        var phone = new PhoneDto { Number = "999", Type = "Home" };
        phone.Type.Should().Be("Home");
        phone.Number.Should().Be("999");
        var phoneCopy = phone with { Type = "Work" };
        phoneCopy.Type.Should().Be("Work");

        var register = new RegisterFirstDirectorRequest
        {
            Name = "Director",
            Email = "director@test.com",
            DocumentNumber = "99999999999",
            Password = "pass",
            ConfirmPassword = "pass",
            BirthDate = DateTime.Today.AddYears(-30),
            Phones = [phone]
        };
        register.ConfirmPassword.Should().Be("pass");
        register.BirthDate.Should().BeBefore(DateTime.Today);
        var registerCopy = register with { Name = "Director Copy" };
        registerCopy.Name.Should().Be("Director Copy");

        var self = new SelfRegisterRequest
        {
            Name = "Self",
            Email = "self@test.com",
            DocumentNumber = "88888888888",
            Password = "pass",
            ConfirmPassword = "pass",
            BirthDate = DateTime.Today.AddYears(-22),
            RoleId = roleId,
            ManagerId = managerId,
            Phones = [phone]
        };
        self.ManagerId.Should().Be(managerId);
        self.RoleId.Should().Be(roleId);
        self.BirthDate.Should().BeBefore(DateTime.Today);
        var selfCopy = self with { Email = "copy@test.com" };
        selfCopy.Email.Should().Be("copy@test.com");

        var login = new LoginRequest { Email = "login@test.com", Password = "pwd" };
        login.Password.Should().Be("pwd");
        login.Email.Should().Be("login@test.com");
        var loginCopy = login with { Password = "pwd2" };
        loginCopy.Password.Should().Be("pwd2");

        var updateProfile = new UpdateProfileRequest
        {
            Name = "Profile",
            Email = "profile@test.com",
            Phones = [phone]
        };
        updateProfile.Email.Should().Be("profile@test.com");
        updateProfile.Phones.Should().ContainSingle();
        var updateProfileCopy = updateProfile with { Name = "Changed" };
        updateProfileCopy.Name.Should().Be("Changed");

        var managerOption = new ManagerOptionDto(managerId, "Manager", "Leader");
        managerOption.Id.Should().Be(managerId);
        managerOption.RoleName.Should().Be("Leader");

        var token = new TokenResponse
        {
            AccessToken = "token",
            TokenType = "Bearer",
            ExpiresIn = 1234,
            User = new UserInfoResponse
            {
                Id = employeeId,
                Name = "User",
                Email = "user@test.com",
                Role = roleSimple,
                CanApproveRegistrations = true,
                CanCreateEmployees = true,
                CanEditEmployees = true,
                CanDeleteEmployees = true,
                CanManageRoles = true,
                PendingApprovals = 1
            }
        };
        token.User.CanManageRoles.Should().BeTrue();
        token.AccessToken.Should().Be("token");
        token.ExpiresIn.Should().Be(1234);
        token.User.PendingApprovals.Should().Be(1);
        var tokenCopy = token with { TokenType = "Custom" };
        tokenCopy.TokenType.Should().Be("Custom");
        var userCopy = token.User with { PendingApprovals = 2 };
        userCopy.PendingApprovals.Should().Be(2);

        var updateEmployee = new UpdateEmployeeRequest
        {
            Name = "Updated",
            Email = "updated@test.com",
            DocumentNumber = "44444444444",
            BirthDate = DateTime.Today.AddYears(-20),
            RoleId = roleId,
            ManagerId = managerId,
            Phones = [phone]
        };
        updateEmployee.RoleId.Should().Be(roleId);
        updateEmployee.DocumentNumber.Should().Be("44444444444");
        var updateEmployeeCopy = updateEmployee with { Name = "Updated Copy" };
        updateEmployeeCopy.Name.Should().Be("Updated Copy");
    }
}

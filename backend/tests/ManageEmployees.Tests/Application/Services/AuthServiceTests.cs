using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Services;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Exceptions;
using ManageEmployees.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ManageEmployees.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly Mock<IJwtTokenService> _jwtServiceMock = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Employees).Returns(_employeeRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Roles).Returns(_roleRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new AuthService(
            _unitOfWorkMock.Object,
            _passwordServiceMock.Object,
            _jwtServiceMock.Object,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorized()
    {
        _employeeRepoMock.Setup(r => r.GetByEmailWithDetailsAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Email = "user@test.com", Password = "123" }));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ThrowsUnauthorized()
    {
        var employee = BuildEmployee(role: BuildRole(canApprove: false));
        _employeeRepoMock.Setup(r => r.GetByEmailWithDetailsAsync(employee.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _passwordServiceMock.Setup(p => p.VerifyPassword("wrong", employee.PasswordHash)).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Email = employee.Email, Password = "wrong" }));
    }

    [Fact]
    public async Task LoginAsync_WhenUserDisabled_ThrowsUnauthorized()
    {
        var employee = BuildEmployee(enabled: false, role: BuildRole(canApprove: true));
        _employeeRepoMock.Setup(r => r.GetByEmailWithDetailsAsync(employee.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _passwordServiceMock.Setup(p => p.VerifyPassword("123", employee.PasswordHash)).Returns(true);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Email = employee.Email, Password = "123" }));
    }

    [Fact]
    public async Task LoginAsync_WithApproverCalculatesPendingCount()
    {
        var role = BuildRole(canApprove: true, hierarchy: 80);
        var employee = BuildEmployee(role: role);
        _employeeRepoMock.Setup(r => r.GetByEmailWithDetailsAsync(employee.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _passwordServiceMock.Setup(p => p.VerifyPassword("123", employee.PasswordHash)).Returns(true);
        _employeeRepoMock.Setup(r => r.CountPendingApprovalForManagerAsync(employee.Id, role.HierarchyLevel, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        _jwtServiceMock.Setup(j => j.GenerateToken(employee)).Returns("jwt-token");

        var token = await _service.LoginAsync(new LoginRequest { Email = employee.Email, Password = "123" });

        token.AccessToken.Should().Be("jwt-token");
        token.User.PendingApprovals.Should().Be(2);
        token.User.CanApproveRegistrations.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_WhenCannotApprove_ReturnsZeroPending()
    {
        var role = BuildRole(canApprove: false, hierarchy: 10);
        var employee = BuildEmployee(role: role);
        _employeeRepoMock.Setup(r => r.GetByEmailWithDetailsAsync(employee.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _passwordServiceMock.Setup(p => p.VerifyPassword("123", employee.PasswordHash)).Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateToken(employee)).Returns("jwt");

        var token = await _service.LoginAsync(new LoginRequest { Email = employee.Email, Password = "123" });

        token.User.PendingApprovals.Should().Be(0);
        _employeeRepoMock.Verify(r => r.CountPendingApprovalForManagerAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterFirstDirectorAsync_WhenDirectorExists_ThrowsConflict()
    {
        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterFirstDirectorAsync(BuildRegisterRequest()));
    }

    [Fact]
    public async Task RegisterFirstDirectorAsync_WhenEmailExists_ThrowsConflict()
    {
        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync("director@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterFirstDirectorAsync(BuildRegisterRequest()));
    }

    [Fact]
    public async Task RegisterFirstDirectorAsync_WhenDocumentExists_ThrowsConflict()
    {
        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync("12345678900", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterFirstDirectorAsync(BuildRegisterRequest()));
    }

    [Fact]
    public async Task RegisterFirstDirectorAsync_WhenRoleNotFound_ThrowsNotFound()
    {
        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RegisterFirstDirectorAsync(BuildRegisterRequest()));
    }

    [Fact]
    public async Task RegisterFirstDirectorAsync_WhenValid_CreatesDirectorAndReturnsToken()
    {
        var request = BuildRegisterRequest();
        var role = BuildRole(canApprove: true, hierarchy: 100);
        var created = BuildEmployee(role: role);

        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(request.DocumentNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _passwordServiceMock.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created)
            .Callback<Employee, CancellationToken>((emp, _) => emp.Id = created.Id);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(created.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);
        _jwtServiceMock.Setup(j => j.GenerateToken(created)).Returns("jwt");

        var response = await _service.RegisterFirstDirectorAsync(request);

        response.AccessToken.Should().Be("jwt");
        response.User.Role.Name.Should().Be(role.Name);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SelfRegisterAsync_WhenEmailExists_ThrowsConflict()
    {
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.SelfRegisterAsync(BuildSelfRegisterRequest()));
    }

    [Fact]
    public async Task SelfRegisterAsync_WhenDocumentExists_ThrowsConflict()
    {
        var request = BuildSelfRegisterRequest();
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(request.DocumentNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.SelfRegisterAsync(request));
    }

    [Fact]
    public async Task SelfRegisterAsync_WhenRoleNotFound_ThrowsNotFound()
    {
        var request = BuildSelfRegisterRequest();
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.SelfRegisterAsync(request));
    }

    [Fact]
    public async Task SelfRegisterAsync_WhenRoleIsDirector_ThrowsForbidden()
    {
        var request = BuildSelfRegisterRequest(roleId: Guid.Parse("ea232c73-67eb-4c8d-920d-a935b3771ec0"));
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildRole(canApprove: true, hierarchy: 100, id: request.RoleId));

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.SelfRegisterAsync(request));
    }

    [Fact]
    public async Task SelfRegisterAsync_WhenManagerNotFound_ThrowsNotFound()
    {
        var request = BuildSelfRegisterRequest(managerId: Guid.NewGuid());
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildRole(canApprove: false, hierarchy: 10, id: request.RoleId));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(request.ManagerId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.SelfRegisterAsync(request));
    }

    [Fact]
    public async Task SelfRegisterAsync_WhenValid_ReturnsPendingEmployee()
    {
        var manager = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 50));
        var request = BuildSelfRegisterRequest(managerId: manager.Id);
        var role = BuildRole(canApprove: false, hierarchy: 10, id: request.RoleId);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(manager.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manager);
        _passwordServiceMock.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee e, CancellationToken _) => e);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var response = await _service.SelfRegisterAsync(request);

        response.Enabled.Should().BeFalse();
        response.ManagerName.Should().Be(manager.Name);
        response.Role.Name.Should().Be(role.Name);
    }

    [Fact]
    public async Task SelfRegisterAsync_WhenManagerNotProvided_ReturnsWithoutManager()
    {
        var request = BuildSelfRegisterRequest();
        var role = BuildRole(canApprove: false, hierarchy: 10, id: request.RoleId);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _passwordServiceMock.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee e, CancellationToken _) => e);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var response = await _service.SelfRegisterAsync(request);

        response.ManagerName.Should().BeNull();
        response.ManagerId.Should().BeNull();
    }

    [Fact]
    public async Task HasDirectorAsync_ReturnsRepositoryValue()
    {
        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.HasDirectorAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenNotFound_Throws()
    {
        var id = Guid.NewGuid();
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCurrentUserAsync(id));
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithApproverCountsPending()
    {
        var employee = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 80));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.CountPendingApprovalForManagerAsync(employee.Id, employee.Role.HierarchyLevel, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var result = await _service.GetCurrentUserAsync(employee.Id);

        result.PendingApprovals.Should().Be(3);
        result.Role.HierarchyLevel.Should().Be(employee.Role.HierarchyLevel);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenCannotApprove_SkipsCount()
    {
        var employee = BuildEmployee(role: BuildRole(canApprove: false, hierarchy: 10));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _service.GetCurrentUserAsync(employee.Id);

        result.PendingApprovals.Should().Be(0);
    }

    private static RegisterFirstDirectorRequest BuildRegisterRequest()
    {
        return new RegisterFirstDirectorRequest
        {
            Name = "Director User",
            Email = "director@test.com",
            DocumentNumber = "12345678900",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-40),
            Phones = [new PhoneDto { Number = "11987654321", Type = "Mobile" }]
        };
    }

    private static SelfRegisterRequest BuildSelfRegisterRequest(Guid? managerId = null, Guid? roleId = null)
    {
        return new SelfRegisterRequest
        {
            Name = "New Employee",
            Email = "user@test.com",
            DocumentNumber = "98765432100",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-20),
            RoleId = roleId ?? Guid.NewGuid(),
            ManagerId = managerId,
            Phones = [new PhoneDto { Number = "11999999999", Type = "Mobile" }]
        };
    }

    private static Role BuildRole(bool canApprove, int hierarchy = 10, Guid? id = null)
    {
        return new Role
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Role",
            HierarchyLevel = hierarchy,
            CanApproveRegistrations = canApprove,
            CanCreateEmployees = true,
            CanDeleteEmployees = true,
            CanEditEmployees = true,
            CanManageRoles = true
        };
    }

    private static Employee BuildEmployee(bool enabled = true, Role? role = null)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test.com",
            DocumentNumber = "11111111111",
            PasswordHash = "hash",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Role = role ?? BuildRole(canApprove: false),
            Enabled = enabled
        };
    }
}

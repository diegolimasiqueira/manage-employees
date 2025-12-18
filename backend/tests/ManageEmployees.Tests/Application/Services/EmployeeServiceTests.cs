using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Services;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Exceptions;
using ManageEmployees.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ManageEmployees.Tests.Application.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<ManageEmployees.Application.Interfaces.IPasswordService> _passwordServiceMock = new();
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Employees).Returns(_employeeRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Roles).Returns(_roleRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new EmployeeService(
            _unitOfWorkMock.Object,
            _passwordServiceMock.Object,
            NullLogger<EmployeeService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedEmployees()
    {
        var role = BuildRole(hierarchy: 10);
        _employeeRepoMock.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Email = "user@test.com",
                    DocumentNumber = "123",
                    BirthDate = DateTime.UtcNow.AddYears(-25),
                    Role = role,
                    Phones = new List<Phone> { new() { Number = "999", Type = "Mobile" } }
                }
            });

        var result = (await _service.GetAllAsync()).ToList();

        result.Should().HaveCount(1);
        result.First().Role.HierarchyLevel.Should().Be(role.HierarchyLevel);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_MapsManagerAndApprover()
    {
        var manager = BuildEmployee();
        var approver = BuildEmployee();
        var employee = BuildEmployee();
        employee.Manager = manager;
        employee.ApprovedBy = approver;
        employee.Phones = new List<Phone> { new Phone { Number = "1", Type = "Home" } };
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _service.GetByIdAsync(employee.Id);

        result!.ManagerName.Should().Be(manager.Name);
        result.ApprovedByName.Should().Be(approver.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenCurrentUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(BuildCreateRequest(), Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_WhenUserCannotCreate_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: false, hierarchy: 50));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.CreateAsync(BuildCreateRequest(), currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenRoleNotFound_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: true, hierarchy: 50));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(BuildCreateRequest(), currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenCannotCreateHigherRole_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: true, hierarchy: 10));
        var targetRole = BuildRole(hierarchy: 20);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.CreateAsync(BuildCreateRequest(roleId: targetRole.Id), currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenEmailExists_ThrowsConflict()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: true, hierarchy: 50));
        var targetRole = BuildRole(hierarchy: 10);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(BuildCreateRequest(roleId: targetRole.Id), currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenDocumentExists_ThrowsConflict()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: true, hierarchy: 50));
        var targetRole = BuildRole(hierarchy: 10);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(BuildCreateRequest(roleId: targetRole.Id), currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WithManager_Succeeds()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: true, hierarchy: 60));
        var manager = BuildEmployee(role: BuildRole(hierarchy: 55));
        var targetRole = BuildRole(hierarchy: 10);
        var request = BuildCreateRequest(roleId: targetRole.Id, managerId: manager.Id);
        var created = BuildEmployee(role: targetRole);
        created.ManagerId = manager.Id;
        created.Manager = manager;

        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(manager)
            .ReturnsAsync(created);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee e, CancellationToken _) =>
            {
                e.Id = created.Id;
                return e;
            });
        _passwordServiceMock.Setup(p => p.HashPassword(request.Password)).Returns("hashed");

        var result = await _service.CreateAsync(request, currentUser.Id);

        result.ManagerId.Should().Be(manager.Id);
    }

    [Fact]
    public async Task CreateAsync_WhenManagerNotFound_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: true, hierarchy: 50));
        var targetRole = BuildRole(hierarchy: 10);
        var request = BuildCreateRequest(managerId: Guid.NewGuid(), roleId: targetRole.Id);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(request.ManagerId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(request, currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_CreatesEmployee()
    {
        var currentUser = BuildEmployee(role: BuildRole(canCreate: true, hierarchy: 50));
        var targetRole = BuildRole(hierarchy: 10);
        var request = BuildCreateRequest(roleId: targetRole.Id);
        var created = BuildEmployee(role: targetRole);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created)
            .Callback<Employee, CancellationToken>((e, _) => e.Id = created.Id);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(created.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);
        _passwordServiceMock.Setup(p => p.HashPassword(request.Password)).Returns("hashed");

        var result = await _service.CreateAsync(request, currentUser.Id);

        result.Enabled.Should().BeTrue();
        result.ApprovedByName.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenCurrentUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), BuildUpdateRequest(), Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WhenTargetNotFound_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), BuildUpdateRequest(), currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleNotFound_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        var request = BuildUpdateRequest(roleId: Guid.NewGuid());
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(target.Id, request, currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenNoPermissionAndNotSelf_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: false, hierarchy: 20));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateAsync(target.Id, BuildUpdateRequest(roleId: target.RoleId), currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenSelfChangingRole_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildRole(hierarchy: 50));

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateAsync(currentUser.Id, BuildUpdateRequest(roleId: Guid.NewGuid()), currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenCannotAssignRole_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 20));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        var higherRole = BuildRole(hierarchy: 25);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);
        _roleRepoMock.Setup(r => r.GetByIdAsync(higherRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(higherRole);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateAsync(target.Id, BuildUpdateRequest(roleId: higherRole.Id), currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailConflict_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        var request = BuildUpdateRequest(email: "new@test.com", roleId: target.RoleId);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target.Role);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(target.Id, request, currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenDocumentConflict_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        var request = BuildUpdateRequest(document: "new-doc", roleId: target.RoleId);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target.Role);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(request.DocumentNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(target.Id, request, currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailAndDocumentUnchanged_SucceedsWithoutConflictChecks()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        var request = BuildUpdateRequest(email: target.Email, document: target.DocumentNumber, roleId: target.RoleId, managerId: null);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target)
            .ReturnsAsync(target);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target.Role);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(target.Id, request, currentUser.Id);

        result.Email.Should().Be(target.Email);
        _employeeRepoMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _employeeRepoMock.Verify(r => r.ExistsByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesEmployee()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        var request = BuildUpdateRequest(roleId: target.RoleId, managerId: currentUser.Id);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target)
            .ReturnsAsync(target);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target.Role);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.ExistsByDocumentAsync(request.DocumentNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(target.Id, request, currentUser.Id);

        result.ManagerId.Should().Be(request.ManagerId);
        result.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingRoleWithPermission_Succeeds()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        var targetRole = BuildRole(hierarchy: 10);
        var newRole = BuildRole(hierarchy: 5);
        var target = BuildEmployee(role: targetRole);
        var request = BuildUpdateRequest(email: target.Email, document: target.DocumentNumber, roleId: newRole.Id, managerId: null);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target)
            .ReturnsAsync(target);
        _roleRepoMock.Setup(r => r.GetByIdAsync(newRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRole);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask)
            .Callback<Employee, CancellationToken>((emp, _) => emp.Role = newRole);

        var result = await _service.UpdateAsync(target.Id, request, currentUser.Id);

        result.Role.Id.Should().Be(newRole.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenEditingSelfWithoutEditPermission_AllowsUpdate()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: false, hierarchy: 20));
        var request = BuildUpdateRequest(email: currentUser.Email, document: currentUser.DocumentNumber, roleId: currentUser.RoleId, managerId: null);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(currentUser)
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(request.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser.Role);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(currentUser.Id, request, currentUser.Id);

        result.Id.Should().Be(currentUser.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenCurrentUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_WhenNoPermission_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canDelete: false, hierarchy: 10));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAsync(Guid.NewGuid(), currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenTargetNotFound_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canDelete: true, hierarchy: 10));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid(), currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenHierarchyLower_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canDelete: true, hierarchy: 5));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAsync(target.Id, currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenSelfDeletion_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canDelete: true, hierarchy: 20));
        var target = BuildEmployee(role: BuildRole(hierarchy: 5));
        target.Id = currentUser.Id;
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAsync(currentUser.Id, currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEmployee()
    {
        var currentUser = BuildEmployee(role: BuildRole(canDelete: true, hierarchy: 50));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(target.Id, currentUser.Id);

        target.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPendingApprovalsAsync_WhenUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPendingApprovalsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPendingApprovalsAsync_WhenCannotApprove_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: false, hierarchy: 20));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetPendingApprovalsAsync(currentUser.Id));
    }

    [Fact]
    public async Task GetPendingApprovalsAsync_FiltersByHierarchy()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 50));
        var lower = BuildEmployee(role: BuildRole(hierarchy: 10));
        var equal = BuildEmployee(role: BuildRole(hierarchy: 50));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _employeeRepoMock.Setup(r => r.GetPendingApprovalAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { lower, equal });

        var result = (await _service.GetPendingApprovalsAsync(currentUser.Id)).ToList();

        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(lower.Id);
    }

    [Fact]
    public async Task ApproveEmployeeAsync_WhenUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ApproveEmployeeAsync(new ApproveEmployeeRequest(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ApproveEmployeeAsync_WhenCannotApprove_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: false, hierarchy: 20));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(BuildEmployee(role: BuildRole(hierarchy: 10)));

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ApproveEmployeeAsync(new ApproveEmployeeRequest { EmployeeId = Guid.NewGuid() }, currentUser.Id));
    }

    [Fact]
    public async Task ApproveEmployeeAsync_WhenTargetNotFound_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 20));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ApproveEmployeeAsync(new ApproveEmployeeRequest { EmployeeId = Guid.NewGuid() }, currentUser.Id));
    }

    [Fact]
    public async Task ApproveEmployeeAsync_WhenAlreadyEnabled_ThrowsConflict()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 20));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        target.Enabled = true;
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ConflictException>(() => _service.ApproveEmployeeAsync(new ApproveEmployeeRequest { EmployeeId = target.Id }, currentUser.Id));
    }

    [Fact]
    public async Task ApproveEmployeeAsync_WhenHierarchyInsufficient_Throws()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 10));
        var target = BuildEmployee(role: BuildRole(hierarchy: 20), enabled: false);
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ApproveEmployeeAsync(new ApproveEmployeeRequest { EmployeeId = target.Id }, currentUser.Id));
    }

    [Fact]
    public async Task ApproveEmployeeAsync_WhenApproving_SetsEnabled()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 50));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10), enabled: false);
        var request = new ApproveEmployeeRequest { EmployeeId = target.Id, Approve = true };
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target)
            .ReturnsAsync(target);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ApproveEmployeeAsync(request, currentUser.Id);

        result.Enabled.Should().BeTrue();
        result.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ApproveEmployeeAsync_WhenRejecting_DisablesEmployee()
    {
        var currentUser = BuildEmployee(role: BuildRole(canApprove: true, hierarchy: 50));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10), enabled: false);
        var request = new ApproveEmployeeRequest { EmployeeId = target.Id, Approve = false, RejectionReason = "Incomplete" };
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target)
            .ReturnsAsync(target);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ApproveEmployeeAsync(request, currentUser.Id);

        result.Enabled.Should().BeFalse();
        target.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetSubordinatesAsync_ReturnsMappedList()
    {
        var managerId = Guid.NewGuid();
        var subordinate = BuildEmployee(role: BuildRole(hierarchy: 5));
        _employeeRepoMock.Setup(r => r.GetByManagerIdAsync(managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { subordinate });

        var result = (await _service.GetSubordinatesAsync(managerId)).ToList();

        result.Should().HaveCount(1);
        result.Single().RoleName.Should().Be(subordinate.Role.Name);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ChangePasswordAsync(Guid.NewGuid(), new ChangePasswordRequest()));
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIncorrect_Throws()
    {
        var employee = BuildEmployee(role: BuildRole(hierarchy: 5));
        _employeeRepoMock.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _passwordServiceMock.Setup(p => p.VerifyPassword("old", employee.PasswordHash)).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.ChangePasswordAsync(employee.Id, new ChangePasswordRequest { CurrentPassword = "old" }));
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValid_UpdatesPasswordHash()
    {
        var employee = BuildEmployee(role: BuildRole(hierarchy: 5));
        _employeeRepoMock.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _passwordServiceMock.Setup(p => p.VerifyPassword("old", employee.PasswordHash)).Returns(true);
        _passwordServiceMock.Setup(p => p.HashPassword("new")).Returns("new-hash");
        _employeeRepoMock.Setup(r => r.UpdateAsync(employee, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.ChangePasswordAsync(employee.Id, new ChangePasswordRequest { CurrentPassword = "old", NewPassword = "new", ConfirmPassword = "new" });

        employee.PasswordHash.Should().Be("new-hash");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenConfirmationMismatch_ThrowsValidation()
    {
        var employee = BuildEmployee(role: BuildRole(hierarchy: 5));
        _employeeRepoMock.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _passwordServiceMock.Setup(p => p.VerifyPassword("old", employee.PasswordHash)).Returns(true);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ChangePasswordAsync(employee.Id, new ChangePasswordRequest
            {
                CurrentPassword = "old",
                NewPassword = "new",
                ConfirmPassword = "different"
            }));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserCannotEdit_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: false, hierarchy: 20));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ResetPasswordAsync(target.Id, currentUser.Id));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenCurrentUserNotFound_ThrowsNotFound()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ResetPasswordAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenEmployeeNotFound_ThrowsNotFound()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 20));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ResetPasswordAsync(Guid.NewGuid(), currentUser.Id));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenHierarchyInsufficient_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 10));
        var target = BuildEmployee(role: BuildRole(hierarchy: 50));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ResetPasswordAsync(target.Id, currentUser.Id));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenValid_ReturnsTemporaryPasswordAndHashes()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 100));
        var target = BuildEmployee(role: BuildRole(hierarchy: 10));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser)
            .ReturnsAsync(target);
        _employeeRepoMock.Setup(r => r.UpdateAsync(target, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        string? capturedTemp = null;
        _passwordServiceMock.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Callback<string>(pwd => capturedTemp = pwd)
            .Returns("hashed-temp");

        var tempPassword = await _service.ResetPasswordAsync(target.Id, currentUser.Id);

        tempPassword.Should().NotBeNullOrWhiteSpace();
        tempPassword.Should().Be(capturedTemp);
        target.PasswordHash.Should().Be("hashed-temp");
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenEmailAlreadyExists_ThrowsConflict()
    {
        var employee = BuildEmployee(role: BuildRole(hierarchy: 5));
        var request = new UpdateProfileRequest
        {
            Name = "New",
            Email = "new@test.com",
            Phones = [new PhoneDto { Number = "1", Type = "Mobile" }]
        };
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateProfileAsync(employee.Id, request));
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenValid_UpdatesPhonesAndEmail()
    {
        var employee = BuildEmployee(role: BuildRole(hierarchy: 5));
        employee.Phones.Add(new Phone { Id = Guid.NewGuid(), Number = "old", Type = "Home", CreatedAt = DateTime.UtcNow });
        var request = new UpdateProfileRequest
        {
            Name = "Updated",
            Email = "updated@test.com",
            Phones = [new PhoneDto { Number = "123", Type = "Mobile" }]
        };
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee)
            .ReturnsAsync(employee)
            .ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.UpdateAsync(employee, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateProfileAsync(employee.Id, request);

        result.Email.Should().Be(request.Email);
        result.Phones.Should().ContainSingle(p => p.Number == "123");
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenEmployeeNotFound_ThrowsNotFound()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileRequest()));
    }

    [Fact]
    public async Task GetAvailableManagersAsync_ShouldMapEnabledEmployees()
    {
        var manager = BuildEmployee(role: BuildRole(hierarchy: 50));
        _employeeRepoMock.Setup(r => r.GetEnabledWithRoleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { manager });

        var managers = (await _service.GetAvailableManagersAsync()).ToList();

        managers.Should().ContainSingle();
        managers.Single().Id.Should().Be(manager.Id);
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenNotAllowed_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: false, hierarchy: 10));
        var target = BuildEmployee(role: BuildRole(hierarchy: 5));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UploadPhotoAsync(target.Id, new FormFile(Stream.Null, 0, 0, "file", "photo.png"), currentUser.Id));
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenCurrentUserNotFound_ThrowsNotFound()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UploadPhotoAsync(Guid.NewGuid(), new FormFile(Stream.Null, 0, 0, "file", "photo.png"), Guid.NewGuid()));
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenEmployeeNotFound_ThrowsNotFound()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 50));
        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UploadPhotoAsync(Guid.NewGuid(), new FormFile(Stream.Null, 0, 0, "file", "photo.png"), currentUser.Id));
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldDeleteOldAndSaveNewFile()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 50));
        var target = BuildEmployee(role: BuildRole(hierarchy: 5));
        target.PhotoPath = "/uploads/photos/old.jpg";
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
        Directory.CreateDirectory(uploadsFolder);
        var oldPath = Path.Combine(uploadsFolder, "old.jpg");
        await File.WriteAllTextAsync(oldPath, "old");

        _employeeRepoMock.SetupSequence(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);
        _employeeRepoMock.Setup(r => r.UpdateAsync(target, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var file = new FormFile(stream, 0, stream.Length, "file", "photo.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var photoUrl = await _service.UploadPhotoAsync(target.Id, file, currentUser.Id);

        photoUrl.Should().Contain(target.Id.ToString());
        File.Exists(oldPath).Should().BeFalse();
        File.Exists(Path.Combine(uploadsFolder, $"{target.Id}.png")).Should().BeTrue();

        Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), true);
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenSelfWithoutOldPhoto_SavesFile()
    {
        var user = BuildEmployee(role: BuildRole(canEdit: false, hierarchy: 5));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _employeeRepoMock.Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using var stream = new MemoryStream(new byte[] { 9, 9, 9 });
        var file = new FormFile(stream, 0, stream.Length, "file", "self.png");

        var photoUrl = await _service.UploadPhotoAsync(user.Id, file, user.Id);

        photoUrl.Should().Contain(user.Id.ToString());
        Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), true);
    }

    [Fact]
    public async Task DeletePhotoAsync_WhenNotAllowed_ThrowsForbidden()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: false, hierarchy: 10));
        var target = BuildEmployee(role: BuildRole(hierarchy: 5));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeletePhotoAsync(target.Id, currentUser.Id));
    }

    [Fact]
    public async Task DeletePhotoAsync_WhenPhotoExists_ShouldRemoveFile()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 50));
        var target = BuildEmployee(role: BuildRole(hierarchy: 5));
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{target.Id}.png";
        var filePath = Path.Combine(uploadsFolder, fileName);
        await File.WriteAllTextAsync(filePath, "image");
        target.PhotoPath = $"/uploads/photos/{fileName}";

        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);
        _employeeRepoMock.Setup(r => r.UpdateAsync(target, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeletePhotoAsync(target.Id, currentUser.Id);

        target.PhotoPath.Should().BeNull();
        File.Exists(filePath).Should().BeFalse();
        Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), true);
    }

    [Fact]
    public async Task DeletePhotoAsync_WhenCurrentUserNotFound_ThrowsNotFound()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeletePhotoAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeletePhotoAsync_WhenEmployeeNotFound_ThrowsNotFound()
    {
        var currentUser = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 50));
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeletePhotoAsync(Guid.NewGuid(), currentUser.Id));
    }

    [Fact]
    public async Task DeletePhotoAsync_WhenSelfWithoutPhoto_ShouldSkipDeletion()
    {
        var user = BuildEmployee(role: BuildRole(canEdit: false, hierarchy: 5));
        user.PhotoPath = null;
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _service.DeletePhotoAsync(user.Id, user.Id);
    }

    [Fact]
    public async Task DeletePhotoAsync_WhenFileMissing_ShouldStillClearPath()
    {
        var user = BuildEmployee(role: BuildRole(canEdit: true, hierarchy: 50));
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{user.Id}.png";
        user.PhotoPath = $"/uploads/photos/{fileName}";

        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _employeeRepoMock.Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeletePhotoAsync(user.Id, user.Id);

        user.PhotoPath.Should().BeNull();
        Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), true);
    }

    private static CreateEmployeeRequest BuildCreateRequest(Guid? roleId = null, Guid? managerId = null)
    {
        return new CreateEmployeeRequest
        {
            Name = "New Employee",
            Email = "new@test.com",
            DocumentNumber = "12345678900",
            Password = "Strong@123",
            BirthDate = DateTime.Today.AddYears(-25),
            RoleId = roleId ?? Guid.NewGuid(),
            ManagerId = managerId,
            Phones = [new PhoneDto { Number = "11999999999", Type = "Mobile" }]
        };
    }

    private static UpdateEmployeeRequest BuildUpdateRequest(string? email = null, string? document = null, Guid? roleId = null, Guid? managerId = null)
    {
        return new UpdateEmployeeRequest
        {
            Name = "Updated User",
            Email = email ?? "updated@test.com",
            DocumentNumber = document ?? "99999999999",
            BirthDate = DateTime.Today.AddYears(-30),
            RoleId = roleId ?? Guid.NewGuid(),
            ManagerId = managerId,
            Phones = [new PhoneDto { Number = "1188888888", Type = "Work" }, new PhoneDto { Number = "1177777777", Type = "Home" }]
        };
    }

    private static Role BuildRole(bool canCreate = true, bool canEdit = true, bool canDelete = true, bool canApprove = true, int hierarchy = 10)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = "Role",
            HierarchyLevel = hierarchy,
            CanCreateEmployees = canCreate,
            CanEditEmployees = canEdit,
            CanDeleteEmployees = canDelete,
            CanApproveRegistrations = canApprove,
            CanManageRoles = true
        };
    }

    private static Employee BuildEmployee(Role? role = null, bool enabled = true)
    {
        role ??= BuildRole();
        return new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test.com",
            DocumentNumber = Guid.NewGuid().ToString(),
            BirthDate = DateTime.UtcNow.AddYears(-30),
            PasswordHash = "hash",
            RoleId = role.Id,
            Role = role,
            Enabled = enabled
        };
    }
}

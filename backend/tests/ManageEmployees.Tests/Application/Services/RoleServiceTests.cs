using FluentAssertions;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Services;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Exceptions;
using ManageEmployees.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ManageEmployees.Tests.Application.Services;

public class RoleServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly RoleService _service;

    public RoleServiceTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Employees).Returns(_employeeRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Roles).Returns(_roleRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new RoleService(_unitOfWorkMock.Object, NullLogger<RoleService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOrderedRoles()
    {
        _roleRepoMock.Setup(r => r.GetAllOrderedByHierarchyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { BuildRole("Leader") });

        var result = (await _service.GetAllAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Leader");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsDto()
    {
        var role = BuildRole("Leader", hierarchy: 20);
        _roleRepoMock.Setup(r => r.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await _service.GetByIdAsync(role.Id);

        result!.Name.Should().Be(role.Name);
    }

    [Fact]
    public async Task GetAssignableRolesAsync_WhenUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAssignableRolesAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAssignableRolesAsync_ReturnsLowerHierarchyRoles()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 50);
        var lowerRole = BuildRole("Junior", hierarchy: 10);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetRolesWithLowerHierarchyAsync(currentUser.Role.HierarchyLevel, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { lowerRole });

        var roles = (await _service.GetAssignableRolesAsync(currentUser.Id)).ToList();

        roles.Should().ContainSingle(r => r.Id == lowerRole.Id);
    }

    [Fact]
    public async Task CreateAsync_WhenUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(BuildCreateRoleRequest(), Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_WhenUserCannotManage_Throws()
    {
        var currentUser = BuildEmployeeWithRole(canManageRoles: false);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.CreateAsync(BuildCreateRoleRequest(), currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenNameExists_Throws()
    {
        var currentUser = BuildEmployeeWithRole();
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(BuildCreateRoleRequest(), currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenHierarchyNotAllowed_Throws()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 20);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        var request = BuildCreateRoleRequest(hierarchy: 25);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.CreateAsync(request, currentUser.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_CreatesRole()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 50);
        var request = BuildCreateRoleRequest(hierarchy: 10);
        var createdRole = BuildRole(request.Name, hierarchy: request.HierarchyLevel);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRole)
            .Callback<Role, CancellationToken>((role, _) => role.Id = createdRole.Id);

        var result = await _service.CreateAsync(request, currentUser.Id);

        result.Id.Should().Be(createdRole.Id);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new UpdateRoleRequest(), Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_WhenCannotManage_Throws()
    {
        var currentUser = BuildEmployeeWithRole(canManageRoles: false);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateAsync(Guid.NewGuid(), new UpdateRoleRequest(), currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleNotFound_Throws()
    {
        var currentUser = BuildEmployeeWithRole();
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new UpdateRoleRequest(), currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenHierarchyNotAllowed_Throws()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 10);
        var targetRole = BuildRole("Target", hierarchy: 20);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateAsync(targetRole.Id, BuildUpdateRoleRequest(), currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenNameConflict_Throws()
    {
        var currentUser = BuildEmployeeWithRole();
        var targetRole = BuildRole("Target", hierarchy: 5);
        var request = BuildUpdateRoleRequest(name: "NewName");
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);
        _roleRepoMock.Setup(r => r.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(targetRole.Id, request, currentUser.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesRole()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 50);
        var targetRole = BuildRole("Target", hierarchy: 10);
        var request = BuildUpdateRoleRequest(name: "Updated", hierarchy: 9);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(targetRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetRole);
        _roleRepoMock.Setup(r => r.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateAsync(targetRole.Id, request, currentUser.Id);

        result.Name.Should().Be("Updated");
        result.HierarchyLevel.Should().Be(request.HierarchyLevel);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_WhenCannotManage_Throws()
    {
        var currentUser = BuildEmployeeWithRole(canManageRoles: false);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAsync(Guid.NewGuid(), currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenRoleNotFound_Throws()
    {
        var currentUser = BuildEmployeeWithRole();
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid(), currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenHierarchyNotAllowed_Throws()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 10);
        var role = BuildRole("Target", hierarchy: 10);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAsync(role.Id, currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenRoleHasEmployees_ThrowsConflict()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 50);
        var role = BuildRole("Target", hierarchy: 10);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(role.Id, currentUser.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenValid_SoftDeletesRole()
    {
        var currentUser = BuildEmployeeWithRole(hierarchy: 50);
        var role = BuildRole("Target", hierarchy: 10);
        _employeeRepoMock.Setup(r => r.GetByIdWithDetailsAsync(currentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);
        _roleRepoMock.Setup(r => r.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _employeeRepoMock.Setup(r => r.ExistsWithRoleAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _roleRepoMock.Setup(r => r.UpdateAsync(role, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.DeleteAsync(role.Id, currentUser.Id);

        role.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateRoleRequest BuildCreateRoleRequest(int hierarchy = 10)
    {
        return new CreateRoleRequest
        {
            Name = "New Role",
            Description = "desc",
            HierarchyLevel = hierarchy,
            CanApproveRegistrations = true,
            CanCreateEmployees = true,
            CanDeleteEmployees = true,
            CanEditEmployees = true,
            CanManageRoles = true
        };
    }

    private static UpdateRoleRequest BuildUpdateRoleRequest(string name = "Updated Role", int hierarchy = 5)
    {
        return new UpdateRoleRequest
        {
            Name = name,
            Description = "updated",
            HierarchyLevel = hierarchy,
            CanApproveRegistrations = false,
            CanCreateEmployees = false,
            CanDeleteEmployees = false,
            CanEditEmployees = true,
            CanManageRoles = true
        };
    }

    private static Role BuildRole(string name, int hierarchy = 10)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            HierarchyLevel = hierarchy,
            CanManageRoles = true,
            CanCreateEmployees = true,
            CanApproveRegistrations = true,
            CanDeleteEmployees = true,
            CanEditEmployees = true
        };
    }

    private static Employee BuildEmployeeWithRole(bool canManageRoles = true, int hierarchy = 20)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            HierarchyLevel = hierarchy,
            CanManageRoles = canManageRoles,
            CanCreateEmployees = true,
            CanApproveRegistrations = true,
            CanDeleteEmployees = true,
            CanEditEmployees = true
        };

        return new Employee
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "user@test.com",
            DocumentNumber = "12345678901",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Role = role
        };
    }
}

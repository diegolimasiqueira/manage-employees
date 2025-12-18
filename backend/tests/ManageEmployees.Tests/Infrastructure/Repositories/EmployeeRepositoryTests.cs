using FluentAssertions;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Infrastructure.Data;
using ManageEmployees.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Repositories;

public class EmployeeRepositoryTests
{
    [Fact]
    public async Task GetByEmailAndDocument_ShouldBeCaseInsensitiveAndRespectIsActive()
    {
        using var setup = CreateContext();
        var repo = new EmployeeRepository(setup.Context);
        var active = BuildEmployee(setup.StaffRole, "user@test.com");
        var inactive = BuildEmployee(setup.StaffRole, "inactive@test.com", isActive: false, document: "000");
        setup.Context.Employees.AddRange(active, inactive);
        await setup.Context.SaveChangesAsync();

        var byEmail = await repo.GetByEmailAsync("USER@test.com");
        byEmail!.Id.Should().Be(active.Id);

        var existsEmail = await repo.ExistsByEmailAsync(inactive.Email);
        existsEmail.Should().BeFalse();

        var byDocument = await repo.GetByDocumentAsync(active.DocumentNumber);
        byDocument!.Id.Should().Be(active.Id);

        var existsDocument = await repo.ExistsByDocumentAsync(inactive.DocumentNumber);
        existsDocument.Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailWithDetails_ShouldLoadRelations()
    {
        using var setup = CreateContext();
        var repo = new EmployeeRepository(setup.Context);
        var manager = BuildEmployee(setup.DirectorRole, "manager@test.com");
        var employee = BuildEmployee(setup.StaffRole, "user@test.com", manager: manager);
        employee.Phones.Add(new Phone { Id = Guid.NewGuid(), Number = "999", Type = "Mobile", EmployeeId = employee.Id, CreatedAt = DateTime.UtcNow });
        setup.Context.Employees.AddRange(manager, employee);
        await setup.Context.SaveChangesAsync();

        var loaded = await repo.GetByEmailWithDetailsAsync(employee.Email);

        loaded!.Role.Should().NotBeNull();
        loaded.Manager!.Id.Should().Be(manager.Id);
        loaded.Phones.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdWithDetails_ShouldIncludeManagerRoleSubordinatesAndApprover()
    {
        using var setup = CreateContext();
        var repo = new EmployeeRepository(setup.Context);
        var approver = BuildEmployee(setup.DirectorRole, "approver@test.com");
        var manager = BuildEmployee(setup.DirectorRole, "manager@test.com");
        var employee = BuildEmployee(setup.StaffRole, "user@test.com", manager: manager, approvedBy: approver);
        var subordinate = BuildEmployee(setup.StaffRole, "sub@test.com", manager: employee);
        setup.Context.Employees.AddRange(approver, manager, employee, subordinate);
        await setup.Context.SaveChangesAsync();

        var loaded = await repo.GetByIdWithDetailsAsync(employee.Id);

        loaded!.Manager!.Role.Should().NotBeNull();
        loaded.Subordinates.Should().ContainSingle(s => s.Id == subordinate.Id);
        loaded.ApprovedBy!.Id.Should().Be(approver.Id);
    }

    [Fact]
    public async Task GetAllAndPendingAndEnabled_ShouldFilterAndOrderCorrectly()
    {
        using var setup = CreateContext();
        var repo = new EmployeeRepository(setup.Context);
        var pending = BuildEmployee(setup.StaffRole, "b@test.com", enabled: false);
        var active = BuildEmployee(setup.StaffRole, "a@test.com");
        var inactive = BuildEmployee(setup.StaffRole, "c@test.com", isActive: false, enabled: false);
        setup.Context.Employees.AddRange(pending, active, inactive);
        await setup.Context.SaveChangesAsync();

        var all = (await repo.GetAllWithDetailsAsync()).ToList();
        all.Select(e => e.Email).Should().Equal("a@test.com", "b@test.com");

        var pendingResult = (await repo.GetPendingApprovalAsync()).ToList();
        pendingResult.Should().HaveCount(1);
        pendingResult.Single().Id.Should().Be(pending.Id);

        var enabledResult = (await repo.GetEnabledWithRoleAsync()).ToList();
        enabledResult.Should().ContainSingle(e => e.Id == active.Id);
    }

    [Fact]
    public async Task GetByManagerAndRoleAndExistence_ShouldRespectFilters()
    {
        using var setup = CreateContext();
        var repo = new EmployeeRepository(setup.Context);
        var manager = BuildEmployee(setup.DirectorRole, "manager@test.com");
        var roleOwner = BuildEmployee(setup.StaffRole, "role@test.com", manager: manager);
        var otherRole = BuildEmployee(setup.DirectorRole, "other@test.com");
        otherRole.RoleId = setup.DirectorRole.Id;
        setup.Context.Employees.AddRange(manager, roleOwner, otherRole);
        await setup.Context.SaveChangesAsync();

        var byManager = (await repo.GetByManagerIdAsync(manager.Id)).ToList();
        byManager.Should().ContainSingle(e => e.Id == roleOwner.Id);

        var byRole = (await repo.GetByRoleIdAsync(setup.StaffRole.Id)).ToList();
        byRole.Should().ContainSingle(e => e.Id == roleOwner.Id);

        (await repo.ExistsWithRoleAsync(setup.StaffRole.Id)).Should().BeTrue();
        roleOwner.IsActive = false;
        await setup.Context.SaveChangesAsync();
        (await repo.ExistsWithRoleAsync(setup.StaffRole.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task CountPendingApprovalForManager_ShouldHonorHierarchyAndEnabled()
    {
        using var setup = CreateContext();
        var repo = new EmployeeRepository(setup.Context);
        var managerLevel = 50;
        var lower = BuildEmployee(setup.StaffRole, "low@test.com", enabled: false);
        var higher = BuildEmployee(setup.DirectorRole, "high@test.com", enabled: false);
        var enabled = BuildEmployee(setup.StaffRole, "enabled@test.com", enabled: true);
        setup.Context.Employees.AddRange(lower, higher, enabled);
        await setup.Context.SaveChangesAsync();

        var count = await repo.CountPendingApprovalForManagerAsync(Guid.NewGuid(), managerLevel);

        count.Should().Be(1);
    }

    private static Employee BuildEmployee(Role role, string email, bool enabled = true, bool isActive = true, Employee? manager = null, Employee? approvedBy = null, string? document = null)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            Name = email,
            Email = email,
            DocumentNumber = document ?? Guid.NewGuid().ToString(),
            PasswordHash = "hash",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            RoleId = role.Id,
            Role = role,
            Enabled = enabled,
            IsActive = isActive,
            ManagerId = manager?.Id,
            Manager = manager,
            ApprovedById = approvedBy?.Id,
            ApprovedBy = approvedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static RepositoryContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        var director = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Director",
            HierarchyLevel = 100,
            IsActive = true,
            CanManageRoles = true,
            CanApproveRegistrations = true,
            CanCreateEmployees = true,
            CanEditEmployees = true
        };
        var staff = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Staff",
            HierarchyLevel = 10,
            IsActive = true,
            CanCreateEmployees = false,
            CanEditEmployees = false,
            CanDeleteEmployees = false,
            CanManageRoles = false
        };

        context.Roles.AddRange(director, staff);
        context.SaveChanges();

        return new RepositoryContext(context, director, staff, connection);
    }

    private sealed record RepositoryContext(ApplicationDbContext Context, Role DirectorRole, Role StaffRole, SqliteConnection Connection) : IDisposable
    {
        public void Dispose()
        {
            Context.Dispose();
            Connection.Dispose();
        }
    }
}

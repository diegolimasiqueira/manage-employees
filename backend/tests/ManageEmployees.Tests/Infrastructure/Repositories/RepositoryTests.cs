using FluentAssertions;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Infrastructure.Data;
using ManageEmployees.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly Repository<Employee> _repository;
    private readonly Role _role;

    public RepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Role",
            HierarchyLevel = 10,
            IsActive = true,
            CanManageRoles = true
        };
        _context.Roles.Add(_role);
        _context.SaveChanges();

        _repository = new Repository<Employee>(_context);
    }

    [Fact]
    public async Task GetByIdAndGetAll_FilterInactiveEntities()
    {
        var active = BuildEmployee(email: "active@test.com");
        var inactive = BuildEmployee(email: "inactive@test.com", isActive: false);
        _context.Employees.AddRange(active, inactive);
        await _context.SaveChangesAsync();

        (await _repository.GetByIdAsync(active.Id)).Should().NotBeNull();
        (await _repository.GetByIdAsync(inactive.Id)).Should().BeNull();

        var all = (await _repository.GetAllAsync()).ToList();
        all.Should().ContainSingle(e => e.Id == active.Id);
    }

    [Fact]
    public async Task FindAndFirstOrDefault_ShouldRespectPredicateAndIsActive()
    {
        var matching = BuildEmployee(email: "match@test.com");
        var inactiveMatching = BuildEmployee(email: "match2@test.com", isActive: false);
        _context.Employees.AddRange(matching, inactiveMatching, BuildEmployee(email: "other@test.com"));
        await _context.SaveChangesAsync();

        var found = (await _repository.FindAsync(e => e.Email.Contains("match"))).ToList();
        found.Should().HaveCount(1).And.OnlyContain(e => e.Id == matching.Id);

        var first = await _repository.FirstOrDefaultAsync(e => e.Email.Contains("match"));
        first!.Id.Should().Be(matching.Id);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalseForInactiveEntities()
    {
        var inactive = BuildEmployee(email: "inactive@test.com", isActive: false);
        _context.Employees.Add(inactive);
        await _context.SaveChangesAsync();

        (await _repository.ExistsAsync(e => e.Email == inactive.Email)).Should().BeFalse();
    }

    [Fact]
    public async Task AddUpdateDeleteAndCount_ShouldWorkAcrossBranches()
    {
        var employee = BuildEmployee(email: "new@test.com");

        await _repository.AddAsync(employee);
        await _context.SaveChangesAsync();

        employee.Name = "Updated";
        await _repository.UpdateAsync(employee);
        await _context.SaveChangesAsync();

        (await _repository.CountAsync(e => e.Email == employee.Email)).Should().Be(1);

        await _repository.DeleteAsync(employee);
        await _context.SaveChangesAsync();

        (await _repository.CountAsync()).Should().Be(0);
        employee.IsActive.Should().BeFalse();
    }

    private Employee BuildEmployee(string email, bool isActive = true)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = email,
            DocumentNumber = Guid.NewGuid().ToString(),
            PasswordHash = "hash",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            RoleId = _role.Id,
            Role = _role,
            IsActive = isActive,
            Enabled = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}

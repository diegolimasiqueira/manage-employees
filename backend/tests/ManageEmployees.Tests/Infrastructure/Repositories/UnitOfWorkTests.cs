using FluentAssertions;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Infrastructure.Data;
using ManageEmployees.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly Role _role;

    public UnitOfWorkTests()
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
            CanCreateEmployees = true,
            CanManageRoles = true
        };
        _context.Roles.Add(_role);
        _context.SaveChanges();

        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNormalizeDateTimesAndSetUpdatedAt()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "user@test.com",
            DocumentNumber = "123",
            PasswordHash = "hash",
            BirthDate = DateTime.SpecifyKind(DateTime.Now.AddYears(-30), DateTimeKind.Unspecified),
            RoleId = _role.Id,
            Role = _role,
            CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
            ApprovedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
            IsActive = true,
            Enabled = true
        };

        _context.Employees.Add(employee);
        await _unitOfWork.SaveChangesAsync();

        employee.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        employee.ApprovedAt.Should().NotBeNull();

        employee.Name = "Updated";
        employee.ApprovedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        _context.Entry(employee).State = EntityState.Modified;

        await _unitOfWork.SaveChangesAsync();

        employee.UpdatedAt.Should().NotBeNull();
        employee.ApprovedAt.Should().NotBeNull();

        var alreadyUtc = new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Utc",
            Email = "utc@test.com",
            DocumentNumber = "444",
            PasswordHash = "hash",
            BirthDate = DateTime.UtcNow.AddYears(-20),
            RoleId = _role.Id,
            Role = _role,
            CreatedAt = DateTime.UtcNow,
            ApprovedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            IsActive = true,
            Enabled = true
        };
        _context.Employees.Add(alreadyUtc);
        await _unitOfWork.SaveChangesAsync();

        alreadyUtc.ApprovedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void RepositoryProperties_ShouldCacheInstances()
    {
        var firstEmployees = _unitOfWork.Employees;
        var secondEmployees = _unitOfWork.Employees;
        var firstRoles = _unitOfWork.Roles;
        var secondRoles = _unitOfWork.Roles;

        firstEmployees.Should().BeSameAs(secondEmployees);
        firstRoles.Should().BeSameAs(secondRoles);
    }

    [Fact]
    public async Task TransactionMethods_ShouldHandleBeginCommitAndRollback()
    {
        await _unitOfWork.CommitTransactionAsync(); // nothing to commit
        await _unitOfWork.RollbackTransactionAsync(); // nothing to rollback

        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.CommitTransactionAsync();

        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.RollbackTransactionAsync();
    }

    [Fact]
    public async Task Dispose_ShouldCleanOpenTransaction()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();
        var uow = new UnitOfWork(ctx);

        await uow.BeginTransactionAsync();
        uow.Dispose();
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _connection.Dispose();
    }
}

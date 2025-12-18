using FluentAssertions;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Infrastructure.Data;
using ManageEmployees.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Repositories;

public class RoleRepositoryTests
{
    [Fact]
    public async Task GetByName_AndExists_ShouldBeCaseInsensitive()
    {
        using var setup = CreateContext();
        var repo = new RoleRepository(setup.Context);
        setup.Context.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "Leader", HierarchyLevel = 50, IsActive = true });
        await setup.Context.SaveChangesAsync();

        var byName = await repo.GetByNameAsync("leader");
        byName!.Name.Should().Be("Leader");

        var exists = await repo.ExistsByNameAsync("LEADER");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllOrderedByHierarchy_ShouldReturnDescending()
    {
        using var setup = CreateContext();
        var repo = new RoleRepository(setup.Context);
        setup.Context.Roles.AddRange(
            new Role { Id = Guid.NewGuid(), Name = "Low", HierarchyLevel = 5, IsActive = true },
            new Role { Id = Guid.NewGuid(), Name = "High", HierarchyLevel = 100, IsActive = true });
        await setup.Context.SaveChangesAsync();

        var roles = (await repo.GetAllOrderedByHierarchyAsync()).ToList();

        roles.Select(r => r.HierarchyLevel).Should().Equal(100, 5);
    }

    [Fact]
    public async Task GetRolesWithLowerHierarchy_ShouldFilterByLevelAndIsActive()
    {
        using var setup = CreateContext();
        var repo = new RoleRepository(setup.Context);
        var manager = new Role { Id = Guid.NewGuid(), Name = "Manager", HierarchyLevel = 50, IsActive = true };
        var staff = new Role { Id = Guid.NewGuid(), Name = "Staff", HierarchyLevel = 10, IsActive = true };
        var inactive = new Role { Id = Guid.NewGuid(), Name = "Inactive", HierarchyLevel = 5, IsActive = false };
        setup.Context.Roles.AddRange(manager, staff, inactive);
        await setup.Context.SaveChangesAsync();

        var lower = (await repo.GetRolesWithLowerHierarchyAsync(60)).ToList();

        lower.Should().ContainSingle(r => r.Id == manager.Id);
        lower.Should().ContainSingle(r => r.Id == staff.Id);
        lower.Should().NotContain(r => r.Id == inactive.Id);
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
        return new RepositoryContext(context, connection);
    }

    private sealed record RepositoryContext(ApplicationDbContext Context, SqliteConnection Connection) : IDisposable
    {
        public void Dispose()
        {
            Context.Dispose();
            Connection.Dispose();
        }
    }
}

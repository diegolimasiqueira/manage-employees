using FluentAssertions;
using ManageEmployees.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Data;

public class ApplicationDbContextTests
{
    [Fact]
    public void DbSets_ShouldBeAvailable()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        using var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        context.Employees.Should().NotBeNull();
        context.Phones.Should().NotBeNull();
        context.Roles.Should().NotBeNull();
    }
}

using FluentAssertions;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Data;

public class DbSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldCreateDefaultRolesAndAdmin()
    {
        using var setup = CreateContext();
        var passwordService = new Mock<IPasswordService>();
        passwordService.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed");
        var seeder = new DbSeeder(setup.Context, passwordService.Object, NullLogger<DbSeeder>.Instance);

        await seeder.SeedAsync();

        setup.Context.Roles.Should().HaveCount(3);
        var admin = await setup.Context.Employees.Include(e => e.Phones).SingleAsync();
        admin.Email.Should().Be("admin@admin.com");
        admin.Phones.Should().ContainSingle();
    }

    [Fact]
    public async Task SeedAsync_WhenDataExists_ShouldSkipCreation()
    {
        using var setup = CreateContext();
        var adminRole = new Role { Id = DbSeeder.AdminRoleId, Name = "Administrador", HierarchyLevel = 100, IsActive = true };
        var gerenteRole = new Role { Id = DbSeeder.GerenteRoleId, Name = "Gerente", HierarchyLevel = 50, IsActive = true };
        var funcRole = new Role { Id = DbSeeder.FuncionarioRoleId, Name = "Funcionario", HierarchyLevel = 10, IsActive = true };
        setup.Context.Roles.AddRange(adminRole, gerenteRole, funcRole);
        await setup.Context.SaveChangesAsync();
        setup.Context.Employees.Add(new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@admin.com",
            DocumentNumber = "000",
            PasswordHash = "existing",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            RoleId = DbSeeder.AdminRoleId,
            Role = adminRole,
            Enabled = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await setup.Context.SaveChangesAsync();

        var passwordService = new Mock<IPasswordService>();
        var seeder = new DbSeeder(setup.Context, passwordService.Object, NullLogger<DbSeeder>.Instance);

        await seeder.SeedAsync();

        setup.Context.Roles.Should().HaveCount(3);
        setup.Context.Employees.Should().HaveCount(1);
        passwordService.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
    }

    private static SeederContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return new SeederContext(context, connection);
    }

    private sealed record SeederContext(ApplicationDbContext Context, SqliteConnection Connection) : IDisposable
    {
        public void Dispose()
        {
            Context.Dispose();
            Connection.Dispose();
        }
    }
}

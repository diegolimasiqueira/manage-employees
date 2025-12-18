using System.Reflection;
using FluentAssertions;
using ManageEmployees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Migrations;

public class MigrationTests
{
    [Fact]
    public void InitialCreate_Migration_ShouldExecuteUpAndDown()
    {
        var migrationType = typeof(ApplicationDbContext).Assembly.GetType("ManageEmployees.Infrastructure.Migrations.InitialCreate")!;
        var migration = (Migration)Activator.CreateInstance(migrationType, nonPublic: true)!;
        var builder = new MigrationBuilder("Npgsql");

        InvokeProtected(migration, "Up", builder);
        InvokeProtected(migration, "Down", builder);

        builder.Operations.Should().NotBeEmpty();
    }

    [Fact]
    public void AddPhotoPathToEmployee_Migration_ShouldExecuteUpAndDown()
    {
        var migrationType = typeof(ApplicationDbContext).Assembly.GetType("ManageEmployees.Infrastructure.Migrations.AddPhotoPathToEmployee")!;
        var migration = (Migration)Activator.CreateInstance(migrationType, nonPublic: true)!;
        var builder = new MigrationBuilder("Npgsql");

        InvokeProtected(migration, "Up", builder);
        InvokeProtected(migration, "Down", builder);

        builder.Operations.Should().NotBeEmpty();
    }

    [Fact]
    public void ApplicationDbContextModelSnapshot_ShouldBuildModel()
    {
        var snapshotType = typeof(ApplicationDbContext).Assembly.GetType("ManageEmployees.Infrastructure.Migrations.ApplicationDbContextModelSnapshot")!;
        var snapshot = Activator.CreateInstance(snapshotType, nonPublic: true)!;
        var conventions = new ConventionSet();
        var modelBuilder = new ModelBuilder(conventions);

        var buildModel = snapshotType.GetMethod("BuildModel", BindingFlags.Instance | BindingFlags.NonPublic)!;
        buildModel.Invoke(snapshot, new object[] { modelBuilder });

        modelBuilder.Model.GetEntityTypes().Should().NotBeEmpty();
    }

    private static void InvokeProtected(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(instance, args);
    }
}

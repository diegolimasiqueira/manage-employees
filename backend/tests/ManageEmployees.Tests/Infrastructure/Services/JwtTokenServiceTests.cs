using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ManageEmployees.Tests.Infrastructure.Services;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_ShouldIncludeCustomClaims()
    {
        var settings = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "super-secret-key-1234567890-abcdefghijklmnopqrstuvwxyz",
            ["JwtSettings:Issuer"] = "issuer",
            ["JwtSettings:Audience"] = "audience"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();

        var service = new JwtTokenService(configuration);
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            HierarchyLevel = 50,
            CanApproveRegistrations = true,
            CanCreateEmployees = true,
            CanEditEmployees = true,
            CanDeleteEmployees = false,
            CanManageRoles = false
        };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "user@test.com",
            Role = role,
            RoleId = role.Id
        };

        var tokenString = service.GenerateToken(employee);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == employee.Email);
        token.Claims.Should().Contain(c => c.Type == "HierarchyLevel" && c.Value == role.HierarchyLevel.ToString());
        token.Claims.Should().Contain(c => c.Type == "CanApproveRegistrations" && c.Value == "True");
        token.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }
}

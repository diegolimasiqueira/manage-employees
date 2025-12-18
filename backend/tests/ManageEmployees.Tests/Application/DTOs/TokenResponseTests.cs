using FluentAssertions;
using ManageEmployees.Application.DTOs;
using Xunit;

namespace ManageEmployees.Tests.Application.DTOs;

public class TokenResponseTests
{
    [Fact]
    public void TokenResponse_AllPropertiesCanBeSet()
    {
        var response = new TokenResponse
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            User = new UserInfoResponse
            {
                Id = Guid.NewGuid(),
                Name = "User Test",
                Email = "user@test.com",
                Role = new RoleSimpleDto { Id = Guid.NewGuid(), Name = "Director", HierarchyLevel = 100 },
                CanApproveRegistrations = true,
                CanCreateEmployees = true,
                CanEditEmployees = true,
                CanDeleteEmployees = true,
                CanManageRoles = true,
                PendingApprovals = 2
            }
        };

        response.AccessToken.Should().Be("token");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
        response.User.Email.Should().Be("user@test.com");
        response.User.PendingApprovals.Should().Be(2);
    }
}

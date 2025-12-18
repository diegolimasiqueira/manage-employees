using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ManageEmployees.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Employee employee)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.Email, employee.Email),
            new(ClaimTypes.Name, employee.Name),
            new("RoleId", employee.RoleId.ToString()),
            new("RoleName", employee.Role.Name),
            new("HierarchyLevel", employee.Role.HierarchyLevel.ToString()),
            new("CanApproveRegistrations", employee.Role.CanApproveRegistrations.ToString()),
            new("CanCreateEmployees", employee.Role.CanCreateEmployees.ToString()),
            new("CanEditEmployees", employee.Role.CanEditEmployees.ToString()),
            new("CanDeleteEmployees", employee.Role.CanDeleteEmployees.ToString()),
            new("CanManageRoles", employee.Role.CanManageRoles.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

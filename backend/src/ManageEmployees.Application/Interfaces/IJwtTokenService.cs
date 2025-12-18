using ManageEmployees.Domain.Entities;

namespace ManageEmployees.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Employee employee);
}

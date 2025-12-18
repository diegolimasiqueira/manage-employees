namespace ManageEmployees.Application.DTOs;

/// <summary>
/// DTO para requisição de login
/// </summary>
public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// DTO para registro do primeiro diretor
/// </summary>
public record RegisterFirstDirectorRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public List<PhoneDto> Phones { get; init; } = new();
}

/// <summary>
/// DTO para auto-registro de funcionário (pendente de aprovação)
/// </summary>
public record SelfRegisterRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public Guid RoleId { get; init; }
    public Guid? ManagerId { get; init; }
    public List<PhoneDto> Phones { get; init; } = new();
}

/// <summary>
/// DTO para resposta de autenticação
/// </summary>
public record TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public UserInfoResponse User { get; init; } = null!;
}

/// <summary>
/// DTO com informações do usuário logado
/// </summary>
public record UserInfoResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public RoleSimpleDto Role { get; init; } = null!;
    public bool CanApproveRegistrations { get; init; }
    public bool CanCreateEmployees { get; init; }
    public bool CanEditEmployees { get; init; }
    public bool CanDeleteEmployees { get; init; }
    public bool CanManageRoles { get; init; }
    public int PendingApprovals { get; init; }
}

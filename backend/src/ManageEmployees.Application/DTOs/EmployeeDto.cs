namespace ManageEmployees.Application.DTOs;

/// <summary>
/// DTO para exibição de funcionário
/// </summary>
public record EmployeeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public int Age { get; init; }
    public RoleSimpleDto Role { get; init; } = null!;
    public Guid? ManagerId { get; init; }
    public string? ManagerName { get; init; }
    public bool Enabled { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? ApprovedByName { get; init; }
    public List<PhoneDto> Phones { get; init; } = new();
    public string? PhotoUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO para criação de funcionário
/// </summary>
public record CreateEmployeeRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public Guid RoleId { get; init; }
    public Guid? ManagerId { get; init; }
    public List<PhoneDto> Phones { get; init; } = new();
}

/// <summary>
/// DTO para atualização de funcionário
/// </summary>
public record UpdateEmployeeRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public Guid RoleId { get; init; }
    public Guid? ManagerId { get; init; }
    public List<PhoneDto> Phones { get; init; } = new();
}

/// <summary>
/// DTO para alterar senha
/// </summary>
public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}

/// <summary>
/// DTO para atualização do próprio perfil (sem alterar cargo/gerente)
/// </summary>
public record UpdateProfileRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<PhoneDto> Phones { get; init; } = new();
}

/// <summary>
/// DTO para funcionário pendente de aprovação
/// </summary>
public record PendingApprovalDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public RoleSimpleDto Role { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO para aprovação de funcionário
/// </summary>
public record ApproveEmployeeRequest
{
    public Guid EmployeeId { get; init; }
    public bool Approve { get; init; }
    public string? RejectionReason { get; init; }
}

/// <summary>
/// DTO simplificado para listagens
/// </summary>
public record EmployeeSimpleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public bool Enabled { get; init; }
}

/// <summary>
/// DTO para opções de gerente (combobox)
/// </summary>
public record ManagerOptionDto(Guid Id, string Name = "", string RoleName = "");

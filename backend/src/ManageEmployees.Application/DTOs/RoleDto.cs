namespace ManageEmployees.Application.DTOs;

/// <summary>
/// DTO para exibição de cargo
/// </summary>
public record RoleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int HierarchyLevel { get; init; }
    public bool CanApproveRegistrations { get; init; }
    public bool CanCreateEmployees { get; init; }
    public bool CanEditEmployees { get; init; }
    public bool CanDeleteEmployees { get; init; }
    public bool CanManageRoles { get; init; }
    public int EmployeeCount { get; init; }
}

/// <summary>
/// DTO para criação de cargo
/// </summary>
public record CreateRoleRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int HierarchyLevel { get; init; }
    public bool CanApproveRegistrations { get; init; }
    public bool CanCreateEmployees { get; init; }
    public bool CanEditEmployees { get; init; }
    public bool CanDeleteEmployees { get; init; }
    public bool CanManageRoles { get; init; }
}

/// <summary>
/// DTO para atualização de cargo
/// </summary>
public record UpdateRoleRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int HierarchyLevel { get; init; }
    public bool CanApproveRegistrations { get; init; }
    public bool CanCreateEmployees { get; init; }
    public bool CanEditEmployees { get; init; }
    public bool CanDeleteEmployees { get; init; }
    public bool CanManageRoles { get; init; }
}

/// <summary>
/// DTO simplificado de cargo para listagens
/// </summary>
public record RoleSimpleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int HierarchyLevel { get; init; }
}




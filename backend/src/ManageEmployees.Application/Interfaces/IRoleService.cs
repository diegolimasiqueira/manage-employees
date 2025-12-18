using ManageEmployees.Application.DTOs;

namespace ManageEmployees.Application.Interfaces;

/// <summary>
/// Serviço para operações com cargos
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Lista todos os cargos
    /// </summary>
    Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca um cargo por ID
    /// </summary>
    Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca cargos que o usuário pode atribuir (hierarquia menor)
    /// </summary>
    Task<IEnumerable<RoleSimpleDto>> GetAssignableRolesAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria um novo cargo
    /// </summary>
    Task<RoleDto> CreateAsync(CreateRoleRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um cargo existente
    /// </summary>
    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove um cargo (soft delete)
    /// </summary>
    Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
}




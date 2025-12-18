using ManageEmployees.Domain.Entities;

namespace ManageEmployees.Domain.Interfaces;

/// <summary>
/// Repositório específico para operações com Roles
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Busca um cargo pelo nome
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca todos os cargos ordenados por nível hierárquico
    /// </summary>
    Task<IEnumerable<Role>> GetAllOrderedByHierarchyAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe algum cargo com o nome especificado
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca cargos com nível hierárquico menor que o especificado
    /// </summary>
    Task<IEnumerable<Role>> GetRolesWithLowerHierarchyAsync(int hierarchyLevel, CancellationToken cancellationToken = default);
}




using ManageEmployees.Domain.Entities;

namespace ManageEmployees.Domain.Interfaces;

/// <summary>
/// Repositório específico para operações com Employee
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Busca funcionário por e-mail
    /// </summary>
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca funcionário por número de documento
    /// </summary>
    Task<Employee?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um funcionário com o e-mail especificado
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um funcionário com o documento especificado
    /// </summary>
    Task<bool> ExistsByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca funcionário por e-mail incluindo Role e Manager
    /// </summary>
    Task<Employee?> GetByEmailWithDetailsAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca funcionário por ID incluindo todas as relações
    /// </summary>
    Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca todos os funcionários com suas relações
    /// </summary>
    Task<IEnumerable<Employee>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca funcionários pendentes de aprovação
    /// </summary>
    Task<IEnumerable<Employee>> GetPendingApprovalAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca funcionários subordinados a um gerente
    /// </summary>
    Task<IEnumerable<Employee>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca funcionários por cargo
    /// </summary>
    Task<IEnumerable<Employee>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe algum funcionário com um cargo específico
    /// </summary>
    Task<bool> ExistsWithRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Conta o número de funcionários com um cargo específico
    /// </summary>
    Task<int> CountByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Conta funcionários pendentes de aprovação que o usuário pode aprovar
    /// </summary>
    Task<int> CountPendingApprovalForManagerAsync(Guid managerId, int managerHierarchyLevel, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca funcionários habilitados (aprovados) com suas roles
    /// </summary>
    Task<IEnumerable<Employee>> GetEnabledWithRoleAsync(CancellationToken cancellationToken = default);
}

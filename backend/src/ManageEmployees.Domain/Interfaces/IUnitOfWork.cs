namespace ManageEmployees.Domain.Interfaces;

/// <summary>
/// Unit of Work para gerenciar transações e repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repositório de funcionários
    /// </summary>
    IEmployeeRepository Employees { get; }
    
    /// <summary>
    /// Repositório de cargos
    /// </summary>
    IRoleRepository Roles { get; }
    
    /// <summary>
    /// Salva todas as alterações pendentes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia uma transação
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirma a transação atual
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Desfaz a transação atual
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

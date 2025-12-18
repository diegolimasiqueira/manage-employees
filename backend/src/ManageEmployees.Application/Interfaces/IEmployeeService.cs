using ManageEmployees.Application.DTOs;

namespace ManageEmployees.Application.Interfaces;

/// <summary>
/// Serviço para operações com funcionários
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Lista todos os funcionários
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca um funcionário por ID
    /// </summary>
    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria um novo funcionário (criado por outro funcionário)
    /// </summary>
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um funcionário existente
    /// </summary>
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove um funcionário (soft delete)
    /// </summary>
    Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lista funcionários pendentes de aprovação
    /// </summary>
    Task<IEnumerable<PendingApprovalDto>> GetPendingApprovalsAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aprova ou rejeita um funcionário
    /// </summary>
    Task<EmployeeDto> ApproveEmployeeAsync(ApproveEmployeeRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lista subordinados de um funcionário
    /// </summary>
    Task<IEnumerable<EmployeeSimpleDto>> GetSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Altera a senha do próprio funcionário (requer senha atual)
    /// </summary>
    Task ChangePasswordAsync(Guid employeeId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reseta a senha de um funcionário (usado por gestores) - retorna a nova senha temporária
    /// </summary>
    Task<string> ResetPasswordAsync(Guid employeeId, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza o próprio perfil do funcionário
    /// </summary>
    Task<EmployeeDto> UpdateProfileAsync(Guid employeeId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lista gerentes disponíveis para seleção (endpoint público)
    /// </summary>
    Task<IEnumerable<ManagerOptionDto>> GetAvailableManagersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Faz upload da foto de perfil do funcionário
    /// </summary>
    Task<string> UploadPhotoAsync(Guid employeeId, Microsoft.AspNetCore.Http.IFormFile file, Guid currentUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove a foto de perfil do funcionário
    /// </summary>
    Task DeletePhotoAsync(Guid employeeId, Guid currentUserId, CancellationToken cancellationToken = default);
}

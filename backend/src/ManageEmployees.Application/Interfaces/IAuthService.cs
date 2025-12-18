using ManageEmployees.Application.DTOs;

namespace ManageEmployees.Application.Interfaces;

/// <summary>
/// Serviço de autenticação
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Realiza login do usuário
    /// </summary>
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra o primeiro diretor do sistema
    /// </summary>
    Task<TokenResponse> RegisterFirstDirectorAsync(RegisterFirstDirectorRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Auto-registro de funcionário (pendente de aprovação)
    /// </summary>
    Task<EmployeeDto> SelfRegisterAsync(SelfRegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se já existe um diretor cadastrado
    /// </summary>
    Task<bool> HasDirectorAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém informações do usuário logado
    /// </summary>
    Task<UserInfoResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

using System.Security.Claims;
using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageEmployees.API.Controllers;

/// <summary>
/// Controller de autenticação
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realiza login do usuário
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return OkResponse(result, "Login realizado com sucesso");
    }

    /// <summary>
    /// Registra o primeiro diretor do sistema
    /// </summary>
    [HttpPost("register-director")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> RegisterFirstDirector([FromBody] RegisterFirstDirectorRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterFirstDirectorAsync(request, cancellationToken);
        return CreatedResponse(result, "Diretor cadastrado com sucesso");
    }

    /// <summary>
    /// Auto-registro de funcionário (pendente de aprovação)
    /// </summary>
    [HttpPost("self-register")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> SelfRegister([FromBody] SelfRegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SelfRegisterAsync(request, cancellationToken);
        return CreatedResponse(result, "Cadastro realizado com sucesso. Aguarde a aprovação de um superior.");
    }

    /// <summary>
    /// Verifica se já existe um diretor cadastrado
    /// </summary>
    [HttpGet("has-director")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> HasDirector(CancellationToken cancellationToken)
    {
        var result = await _authService.HasDirectorAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Obtém informações do usuário logado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentUserAsync(CurrentUserId, cancellationToken);
        return OkResponse(result);
    }
}

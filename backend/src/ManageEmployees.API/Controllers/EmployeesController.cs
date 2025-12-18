using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageEmployees.API.Controllers;

/// <summary>
/// Controller de funcionários
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : BaseController
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Lista gerentes disponíveis para seleção (público - para registro)
    /// </summary>
    [HttpGet("managers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ManagerOptionDto>>), 200)]
    public async Task<IActionResult> GetAvailableManagers(CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetAvailableManagersAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Lista todos os funcionários
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetAllAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Busca um funcionário por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail("Funcionário não encontrado"));
        
        return OkResponse(result);
    }

    /// <summary>
    /// Cria um novo funcionário
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateAsync(request, CurrentUserId, cancellationToken);
        return CreatedResponse(result, "Funcionário criado com sucesso");
    }

    /// <summary>
    /// Atualiza um funcionário
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return OkResponse(result, "Funcionário atualizado com sucesso");
    }

    /// <summary>
    /// Remove um funcionário
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _employeeService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return NoContentResponse("Funcionário removido com sucesso");
    }

    /// <summary>
    /// Lista funcionários pendentes de aprovação
    /// </summary>
    [HttpGet("pending-approvals")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PendingApprovalDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetPendingApprovalsAsync(CurrentUserId, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Aprova ou rejeita um funcionário
    /// </summary>
    [HttpPost("approve")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> ApproveEmployee([FromBody] ApproveEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.ApproveEmployeeAsync(request, CurrentUserId, cancellationToken);
        var message = request.Approve ? "Funcionário aprovado com sucesso" : "Funcionário rejeitado";
        return OkResponse(result, message);
    }

    /// <summary>
    /// Lista subordinados de um funcionário
    /// </summary>
    [HttpGet("{id:guid}/subordinates")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeSimpleDto>>), 200)]
    public async Task<IActionResult> GetSubordinates(Guid id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetSubordinatesAsync(id, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Altera a senha do funcionário logado
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _employeeService.ChangePasswordAsync(CurrentUserId, request, cancellationToken);
        return NoContentResponse("Senha alterada com sucesso");
    }

    /// <summary>
    /// Reseta a senha de um funcionário (usado por gestores)
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var tempPassword = await _employeeService.ResetPasswordAsync(id, CurrentUserId, cancellationToken);
        return OkResponse(new { temporaryPassword = tempPassword }, "Senha resetada com sucesso");
    }

    /// <summary>
    /// Atualiza o próprio perfil do funcionário logado
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateProfileAsync(CurrentUserId, request, cancellationToken);
        return OkResponse(result, "Perfil atualizado com sucesso");
    }

    /// <summary>
    /// Obtém o perfil do funcionário logado
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetByIdAsync(CurrentUserId, cancellationToken);
        if (result == null)
            return NotFound();
        return OkResponse(result);
    }

    /// <summary>
    /// Faz upload da foto de perfil do funcionário
    /// </summary>
    [HttpPost("{id:guid}/photo")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Nenhum arquivo enviado"));

        // Validar extensão
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<object>.Fail("Formato de arquivo não permitido. Use: jpg, jpeg, png, gif ou webp"));

        // Validar tamanho (máximo 5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.Fail("Arquivo muito grande. Máximo permitido: 5MB"));

        var photoUrl = await _employeeService.UploadPhotoAsync(id, file, CurrentUserId, cancellationToken);
        return OkResponse(new { photoUrl }, "Foto atualizada com sucesso");
    }

    /// <summary>
    /// Remove a foto de perfil do funcionário
    /// </summary>
    [HttpDelete("{id:guid}/photo")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeletePhoto(Guid id, CancellationToken cancellationToken)
    {
        await _employeeService.DeletePhotoAsync(id, CurrentUserId, cancellationToken);
        return OkResponse<object>(null, "Foto removida com sucesso");
    }
}

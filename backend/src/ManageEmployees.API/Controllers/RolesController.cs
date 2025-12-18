using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageEmployees.API.Controllers;

/// <summary>
/// Controller de cargos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Lista todos os cargos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Lista todos os cargos (público - para auto-registro)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), 200)]
    public async Task<IActionResult> GetAllPublic(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Busca um cargo por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail("Cargo não encontrado"));
        
        return OkResponse(result);
    }

    /// <summary>
    /// Lista cargos que o usuário pode atribuir
    /// </summary>
    [HttpGet("assignable")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleSimpleDto>>), 200)]
    public async Task<IActionResult> GetAssignableRoles(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAssignableRolesAsync(CurrentUserId, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Cria um novo cargo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(request, CurrentUserId, cancellationToken);
        return CreatedResponse(result, "Cargo criado com sucesso");
    }

    /// <summary>
    /// Atualiza um cargo
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return OkResponse(result, "Cargo atualizado com sucesso");
    }

    /// <summary>
    /// Remove um cargo
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _roleService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return NoContentResponse("Cargo removido com sucesso");
    }
}

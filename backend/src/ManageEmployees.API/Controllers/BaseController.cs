using System.Security.Claims;
using ManageEmployees.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ManageEmployees.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
    
    protected string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    
    protected string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    
    protected IActionResult OkResponse<T>(T data, string message = "Operação realizada com sucesso.")
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }
    
    protected IActionResult CreatedResponse<T>(T data, string message = "Registro criado com sucesso.")
    {
        return StatusCode(201, ApiResponse<T>.Ok(data, message));
    }
    
    protected IActionResult NoContentResponse(string message = "Operação realizada com sucesso.")
    {
        return Ok(ApiResponse.Ok(message));
    }
}


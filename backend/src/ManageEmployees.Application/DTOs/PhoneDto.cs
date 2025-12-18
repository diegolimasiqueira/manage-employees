namespace ManageEmployees.Application.DTOs;

/// <summary>
/// DTO para telefone
/// </summary>
public record PhoneDto
{
    public string Number { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // Mobile, Home, Work
}

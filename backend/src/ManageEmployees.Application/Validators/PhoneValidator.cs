using FluentValidation;
using ManageEmployees.Application.DTOs;

namespace ManageEmployees.Application.Validators;

public class PhoneValidator : AbstractValidator<PhoneDto>
{
    public PhoneValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Número do telefone é obrigatório")
            .MinimumLength(8).WithMessage("Número do telefone deve ter no mínimo 8 caracteres")
            .MaximumLength(20).WithMessage("Número do telefone deve ter no máximo 20 caracteres")
            .Matches(@"^[\d\s\-\(\)\+]+$").WithMessage("Número do telefone contém caracteres inválidos");
        
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo do telefone é obrigatório")
            .Must(type => new[] { "Mobile", "Home", "Work", "Celular", "Residencial", "Comercial" }.Contains(type))
            .WithMessage("Tipo do telefone deve ser: Mobile, Home, Work, Celular, Residencial ou Comercial");
    }
}

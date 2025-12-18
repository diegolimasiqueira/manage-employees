using FluentValidation;
using ManageEmployees.Application.DTOs;

namespace ManageEmployees.Application.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter pelo menos 3 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido")
            .MaximumLength(200).WithMessage("E-mail deve ter no máximo 200 caracteres");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Documento é obrigatório")
            .MinimumLength(11).WithMessage("Documento deve ter pelo menos 11 caracteres")
            .MaximumLength(20).WithMessage("Documento deve ter no máximo 20 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória")
            .LessThan(DateTime.Today).WithMessage("Data de nascimento deve ser anterior a hoje")
            .Must(BeAtLeast18YearsOld).WithMessage("Funcionário deve ter pelo menos 18 anos");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Cargo é obrigatório");

        RuleForEach(x => x.Phones)
            .SetValidator(new PhoneValidator());
    }

    private bool BeAtLeast18YearsOld(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age >= 18;
    }
}

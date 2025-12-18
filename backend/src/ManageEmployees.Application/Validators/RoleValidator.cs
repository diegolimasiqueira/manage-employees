using FluentValidation;
using ManageEmployees.Application.DTOs;

namespace ManageEmployees.Application.Validators;

public class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter pelo menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.HierarchyLevel)
            .GreaterThan(0).WithMessage("Nível hierárquico deve ser maior que zero")
            .LessThanOrEqualTo(100).WithMessage("Nível hierárquico deve ser no máximo 100");
    }
}

public class UpdateRoleValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter pelo menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.HierarchyLevel)
            .GreaterThan(0).WithMessage("Nível hierárquico deve ser maior que zero")
            .LessThanOrEqualTo(100).WithMessage("Nível hierárquico deve ser no máximo 100");
    }
}




using FluentValidation;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ManageEmployees.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar validadores automaticamente
        services.AddValidatorsFromAssemblyContaining<Validators.CreateEmployeeValidator>();

        // Registrar serviços
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}

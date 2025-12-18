using ManageEmployees.Application.Interfaces;
using ManageEmployees.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManageEmployees.Infrastructure.Data;

public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<DbSeeder> _logger;

    // IDs fixos para os cargos padrão
    public static readonly Guid AdminRoleId = Guid.Parse("ea232c73-67eb-4c8d-920d-a935b3771ec0");
    public static readonly Guid GerenteRoleId = Guid.Parse("b8d5c3a1-2e4f-4a67-89ab-0c1d2e3f4a5b");
    public static readonly Guid FuncionarioRoleId = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");

    public DbSeeder(
        ApplicationDbContext context, 
        IPasswordService passwordService,
        ILogger<DbSeeder> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // 1. Criar os cargos padrão se não existirem
        
        // Administrador
        if (!await _context.Roles.AnyAsync(r => r.Id == AdminRoleId))
        {
            _logger.LogInformation("Criando cargo Administrador...");
            _context.Roles.Add(new Role
            {
                Id = AdminRoleId,
                Name = "Administrador",
                Description = "Administrador do sistema com acesso total",
                HierarchyLevel = 100,
                CanApproveRegistrations = true,
                CanCreateEmployees = true,
                CanEditEmployees = true,
                CanDeleteEmployees = true,
                CanManageRoles = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Gerente
        if (!await _context.Roles.AnyAsync(r => r.Id == GerenteRoleId))
        {
            _logger.LogInformation("Criando cargo Gerente...");
            _context.Roles.Add(new Role
            {
                Id = GerenteRoleId,
                Name = "Gerente",
                Description = "Gerente de equipe com permissões de gestão",
                HierarchyLevel = 50,
                CanApproveRegistrations = true,
                CanCreateEmployees = true,
                CanEditEmployees = true,
                CanDeleteEmployees = false,
                CanManageRoles = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Funcionário
        if (!await _context.Roles.AnyAsync(r => r.Id == FuncionarioRoleId))
        {
            _logger.LogInformation("Criando cargo Funcionário...");
            _context.Roles.Add(new Role
            {
                Id = FuncionarioRoleId,
                Name = "Funcionário",
                Description = "Funcionário comum sem permissões administrativas",
                HierarchyLevel = 10,
                CanApproveRegistrations = false,
                CanCreateEmployees = false,
                CanEditEmployees = false,
                CanDeleteEmployees = false,
                CanManageRoles = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        await _context.SaveChangesAsync();

        // 2. Criar o usuário admin se não existir
        var adminEmail = "admin@admin.com";
        var adminExists = await _context.Employees.AnyAsync(e => e.Email == adminEmail);
        
        if (!adminExists)
        {
            _logger.LogInformation("Criando usuário administrador...");
            
            var adminUser = new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Administrador",
                Email = adminEmail,
                PasswordHash = _passwordService.HashPassword("Master@123"),
                DocumentNumber = "00000000000",
                BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                RoleId = AdminRoleId,
                Enabled = true,
                IsActive = true,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            // Adicionar telefone padrão
            adminUser.Phones.Add(new Phone
            {
                Id = Guid.NewGuid(),
                Number = "(00) 00000-0000",
                Type = "Mobile",
                EmployeeId = adminUser.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            
            _context.Employees.Add(adminUser);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Usuário administrador criado com sucesso. Email: {Email}", adminEmail);
        }
    }
}

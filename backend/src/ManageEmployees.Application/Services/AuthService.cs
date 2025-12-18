using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Exceptions;
using ManageEmployees.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManageEmployees.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    // ID do cargo Diretor (seed data)
    // ID do cargo Administrador (criado pelo DbSeeder)
    private static readonly Guid AdminRoleId = Guid.Parse("ea232c73-67eb-4c8d-920d-a935b3771ec0");

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByEmailWithDetailsAsync(request.Email, cancellationToken);
        
        if (employee == null)
        {
            _logger.LogWarning("Tentativa de login com e-mail inexistente: {Email}", request.Email);
            throw new UnauthorizedException("E-mail ou senha inválidos");
        }

        if (!_passwordService.VerifyPassword(request.Password, employee.PasswordHash))
        {
            _logger.LogWarning("Tentativa de login com senha inválida para: {Email}", request.Email);
            throw new UnauthorizedException("E-mail ou senha inválidos");
        }

        if (!employee.Enabled)
        {
            _logger.LogWarning("Tentativa de login de usuário não aprovado: {Email}", request.Email);
            throw new UnauthorizedException("Seu cadastro ainda não foi aprovado. Aguarde a aprovação de um superior.");
        }

        var token = _jwtTokenService.GenerateToken(employee);
        var pendingApprovals = 0;
        
        if (employee.Role.CanApproveRegistrations)
        {
            pendingApprovals = await _unitOfWork.Employees.CountPendingApprovalForManagerAsync(
                employee.Id, employee.Role.HierarchyLevel, cancellationToken);
        }

        _logger.LogInformation("Login realizado com sucesso: {Email}", request.Email);

        return new TokenResponse
        {
            AccessToken = token,
            ExpiresIn = 3600,
            User = new UserInfoResponse
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Role = new RoleSimpleDto
                {
                    Id = employee.Role.Id,
                    Name = employee.Role.Name,
                    HierarchyLevel = employee.Role.HierarchyLevel
                },
                CanApproveRegistrations = employee.Role.CanApproveRegistrations,
                CanCreateEmployees = employee.Role.CanCreateEmployees,
                CanEditEmployees = employee.Role.CanEditEmployees,
                CanDeleteEmployees = employee.Role.CanDeleteEmployees,
                CanManageRoles = employee.Role.CanManageRoles,
                PendingApprovals = pendingApprovals
            }
        };
    }

    public async Task<TokenResponse> RegisterFirstDirectorAsync(RegisterFirstDirectorRequest request, CancellationToken cancellationToken = default)
    {
        // Verificar se já existe um diretor
        if (await HasDirectorAsync(cancellationToken))
        {
            throw new ConflictException("Já existe um diretor cadastrado no sistema");
        }

        // Verificar e-mail duplicado
        if (await _unitOfWork.Employees.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("E-mail já cadastrado");
        }

        // Verificar documento duplicado
        if (await _unitOfWork.Employees.ExistsByDocumentAsync(request.DocumentNumber, cancellationToken))
        {
            throw new ConflictException("Documento já cadastrado");
        }

        // Buscar o cargo de Diretor
        var directorRole = await _unitOfWork.Roles.GetByIdAsync(AdminRoleId, cancellationToken)
            ?? throw new NotFoundException("Role", AdminRoleId);

        var employee = new Employee
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            DocumentNumber = request.DocumentNumber,
            PasswordHash = _passwordService.HashPassword(request.Password),
            BirthDate = DateTime.SpecifyKind(request.BirthDate.Date, DateTimeKind.Utc),
            RoleId = directorRole.Id,
            ManagerId = null,
            Enabled = true, // Diretor já aprovado
            ApprovedAt = DateTime.UtcNow,
            Phones = request.Phones.Select(p => new Phone
            {
                Number = p.Number,
                Type = p.Type
            }).ToList()
        };

        await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarregar com Role
        employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(employee.Id, cancellationToken);

        _logger.LogInformation("Primeiro diretor cadastrado: {Email}", request.Email);

        var token = _jwtTokenService.GenerateToken(employee!);

        return new TokenResponse
        {
            AccessToken = token,
            ExpiresIn = 3600,
            User = new UserInfoResponse
            {
                Id = employee!.Id,
                Name = employee.Name,
                Email = employee.Email,
                Role = new RoleSimpleDto
                {
                    Id = employee.Role.Id,
                    Name = employee.Role.Name,
                    HierarchyLevel = employee.Role.HierarchyLevel
                },
                CanApproveRegistrations = employee.Role.CanApproveRegistrations,
                CanCreateEmployees = employee.Role.CanCreateEmployees,
                CanEditEmployees = employee.Role.CanEditEmployees,
                CanDeleteEmployees = employee.Role.CanDeleteEmployees,
                CanManageRoles = employee.Role.CanManageRoles,
                PendingApprovals = 0
            }
        };
    }

    public async Task<EmployeeDto> SelfRegisterAsync(SelfRegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Verificar e-mail duplicado
        if (await _unitOfWork.Employees.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("E-mail já cadastrado");
        }

        // Verificar documento duplicado
        if (await _unitOfWork.Employees.ExistsByDocumentAsync(request.DocumentNumber, cancellationToken))
        {
            throw new ConflictException("Documento já cadastrado");
        }

        // Buscar o cargo
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException("Role", request.RoleId);

        // Não permitir auto-registro como Diretor
        if (role.Id == AdminRoleId)
        {
            throw new ForbiddenException("Não é permitido auto-registro como Diretor");
        }

        // Verificar gerente se informado
        Employee? manager = null;
        if (request.ManagerId.HasValue)
        {
            manager = await _unitOfWork.Employees.GetByIdWithDetailsAsync(request.ManagerId.Value, cancellationToken)
                ?? throw new NotFoundException("Employee", request.ManagerId.Value);
        }

        var employee = new Employee
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            DocumentNumber = request.DocumentNumber,
            PasswordHash = _passwordService.HashPassword(request.Password),
            BirthDate = DateTime.SpecifyKind(request.BirthDate.Date, DateTimeKind.Utc),
            RoleId = role.Id,
            ManagerId = request.ManagerId,
            Enabled = false, // Aguardando aprovação
            Phones = request.Phones.Select(p => new Phone
            {
                Number = p.Number,
                Type = p.Type
            }).ToList()
        };

        await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Auto-registro realizado, aguardando aprovação: {Email}", request.Email);

        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            DocumentNumber = employee.DocumentNumber,
            BirthDate = employee.BirthDate,
            Age = employee.GetAge(),
            Role = new RoleSimpleDto
            {
                Id = role.Id,
                Name = role.Name,
                HierarchyLevel = role.HierarchyLevel
            },
            ManagerId = employee.ManagerId,
            ManagerName = manager?.Name,
            Enabled = employee.Enabled,
            Phones = employee.Phones.Select(p => new PhoneDto
            {
                Number = p.Number,
                Type = p.Type
            }).ToList(),
            CreatedAt = employee.CreatedAt
        };
    }

    public async Task<bool> HasDirectorAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Employees.ExistsWithRoleAsync(AdminRoleId, cancellationToken);
    }

    public async Task<UserInfoResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Employee", userId);

        var pendingApprovals = 0;
        if (employee.Role.CanApproveRegistrations)
        {
            pendingApprovals = await _unitOfWork.Employees.CountPendingApprovalForManagerAsync(
                employee.Id, employee.Role.HierarchyLevel, cancellationToken);
        }

        return new UserInfoResponse
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Role = new RoleSimpleDto
            {
                Id = employee.Role.Id,
                Name = employee.Role.Name,
                HierarchyLevel = employee.Role.HierarchyLevel
            },
            CanApproveRegistrations = employee.Role.CanApproveRegistrations,
            CanCreateEmployees = employee.Role.CanCreateEmployees,
            CanEditEmployees = employee.Role.CanEditEmployees,
            CanDeleteEmployees = employee.Role.CanDeleteEmployees,
            CanManageRoles = employee.Role.CanManageRoles,
            PendingApprovals = pendingApprovals
        };
    }
}

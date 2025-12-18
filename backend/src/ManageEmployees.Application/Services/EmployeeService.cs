using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Exceptions;
using ManageEmployees.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManageEmployees.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ILogger<EmployeeService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _unitOfWork.Employees.GetAllWithDetailsAsync(cancellationToken);
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(id, cancellationToken);
        return employee != null ? MapToDto(employee) : null;
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        if (!currentUser.Role.CanCreateEmployees)
        {
            throw new ForbiddenException("Você não tem permissão para criar funcionários");
        }

        // Verificar cargo
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException("Role", request.RoleId);

        // Verificar hierarquia
        if (!currentUser.CanCreateEmployeeWithRole(role))
        {
            throw new ForbiddenException("Você não pode criar funcionários com cargo igual ou superior ao seu");
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
            Enabled = true, // Criado por superior já está aprovado
            ApprovedAt = DateTime.UtcNow,
            ApprovedById = currentUserId,
            Phones = request.Phones.Select(p => new Phone
            {
                Number = p.Number,
                Type = p.Type
            }).ToList()
        };

        await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarregar com detalhes
        employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(employee.Id, cancellationToken);

        _logger.LogInformation("Funcionário criado: {Email} por {Creator}", request.Email, currentUser.Email);

        return MapToDto(employee!);
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Employee", id);

        // Verificar permissão de edição
        var canEdit = currentUser.Role.CanEditEmployees && 
                      currentUser.Role.HierarchyLevel > employee.Role.HierarchyLevel;
        var isSelf = id == currentUserId;

        if (!canEdit && !isSelf)
        {
            throw new ForbiddenException("Você não tem permissão para editar este funcionário");
        }

        // Verificar cargo
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException("Role", request.RoleId);

        // Só pode mudar cargo se tiver permissão e for hierarquia menor
        if (request.RoleId != employee.RoleId)
        {
            if (isSelf)
            {
                throw new ForbiddenException("Você não pode alterar seu próprio cargo");
            }
            if (!currentUser.CanCreateEmployeeWithRole(role))
            {
                throw new ForbiddenException("Você não pode atribuir este cargo");
            }
        }

        // Verificar e-mail duplicado
        if (employee.Email.ToLower() != request.Email.ToLower())
        {
            if (await _unitOfWork.Employees.ExistsByEmailAsync(request.Email, cancellationToken))
            {
                throw new ConflictException("E-mail já cadastrado");
            }
        }

        // Verificar documento duplicado
        if (employee.DocumentNumber != request.DocumentNumber)
        {
            if (await _unitOfWork.Employees.ExistsByDocumentAsync(request.DocumentNumber, cancellationToken))
            {
                throw new ConflictException("Documento já cadastrado");
            }
        }

        // Atualizar dados
        employee.Name = request.Name;
        employee.Email = request.Email.ToLower();
        employee.DocumentNumber = request.DocumentNumber;
        employee.BirthDate = DateTime.SpecifyKind(request.BirthDate.Date, DateTimeKind.Utc);
        employee.RoleId = request.RoleId;
        employee.ManagerId = request.ManagerId;

        // Atualizar telefones
        employee.Phones.Clear();
        foreach (var phone in request.Phones)
        {
            employee.Phones.Add(new Phone
            {
                Number = phone.Number,
                Type = phone.Type,
                EmployeeId = employee.Id
            });
        }

        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarregar
        employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(id, cancellationToken);

        _logger.LogInformation("Funcionário atualizado: {Email} por {Editor}", request.Email, currentUser.Email);

        return MapToDto(employee!);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        if (!currentUser.Role.CanDeleteEmployees)
        {
            throw new ForbiddenException("Você não tem permissão para excluir funcionários");
        }

        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Employee", id);

        if (currentUser.Role.HierarchyLevel <= employee.Role.HierarchyLevel)
        {
            throw new ForbiddenException("Você não pode excluir funcionários com cargo igual ou superior ao seu");
        }

        if (id == currentUserId)
        {
            throw new ForbiddenException("Você não pode excluir a si mesmo");
        }

        // Soft delete
        employee.IsActive = false;
        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Funcionário excluído: {Email} por {Deleter}", employee.Email, currentUser.Email);
    }

    public async Task<IEnumerable<PendingApprovalDto>> GetPendingApprovalsAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        if (!currentUser.Role.CanApproveRegistrations)
        {
            throw new ForbiddenException("Você não tem permissão para aprovar cadastros");
        }

        var pending = await _unitOfWork.Employees.GetPendingApprovalAsync(cancellationToken);
        
        // Filtrar apenas os que o usuário pode aprovar (hierarquia menor)
        return pending
            .Where(e => e.Role.HierarchyLevel < currentUser.Role.HierarchyLevel)
            .Select(e => new PendingApprovalDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                DocumentNumber = e.DocumentNumber,
                Role = new RoleSimpleDto
                {
                    Id = e.Role.Id,
                    Name = e.Role.Name,
                    HierarchyLevel = e.Role.HierarchyLevel
                },
                CreatedAt = e.CreatedAt
            });
    }

    public async Task<EmployeeDto> ApproveEmployeeAsync(ApproveEmployeeRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        if (!currentUser.Role.CanApproveRegistrations)
        {
            throw new ForbiddenException("Você não tem permissão para aprovar cadastros");
        }

        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        if (employee.Enabled)
        {
            throw new ConflictException("Funcionário já está aprovado");
        }

        if (!currentUser.CanApproveEmployee(employee))
        {
            throw new ForbiddenException("Você não pode aprovar funcionários com cargo igual ou superior ao seu");
        }

        if (request.Approve)
        {
            employee.Enabled = true;
            employee.ApprovedAt = DateTime.UtcNow;
            employee.ApprovedById = currentUserId;
            _logger.LogInformation("Funcionário aprovado: {Email} por {Approver}", employee.Email, currentUser.Email);
        }
        else
        {
            // Rejeitar = excluir
            employee.IsActive = false;
            _logger.LogInformation("Funcionário rejeitado: {Email} por {Rejector}. Motivo: {Reason}", 
                employee.Email, currentUser.Email, request.RejectionReason);
        }

        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(employee);
    }

    public async Task<IEnumerable<EmployeeSimpleDto>> GetSubordinatesAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        var subordinates = await _unitOfWork.Employees.GetByManagerIdAsync(managerId, cancellationToken);
        return subordinates.Select(e => new EmployeeSimpleDto
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            RoleName = e.Role.Name,
            Enabled = e.Enabled
        });
    }

    public async Task ChangePasswordAsync(Guid employeeId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", employeeId);

        if (!_passwordService.VerifyPassword(request.CurrentPassword, employee.PasswordHash))
        {
            throw new UnauthorizedException("Senha atual incorreta");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ValidationException("confirmPassword", "As senhas não coincidem");
        }

        employee.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Senha alterada para: {Email}", employee.Email);
    }

    public async Task<string> ResetPasswordAsync(Guid employeeId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(employeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", employeeId);

        // Verificar permissão
        if (!currentUser.Role.CanEditEmployees)
        {
            throw new ForbiddenException("Você não tem permissão para resetar senhas");
        }

        // Verificar hierarquia
        if (currentUser.Role.HierarchyLevel <= employee.Role.HierarchyLevel)
        {
            throw new ForbiddenException("Você não pode resetar a senha de funcionários com cargo igual ou superior ao seu");
        }

        // Gerar senha temporária
        var tempPassword = GenerateTemporaryPassword();
        
        employee.PasswordHash = _passwordService.HashPassword(tempPassword);
        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Senha resetada para: {Email} por {Resetter}", employee.Email, currentUser.Email);

        return tempPassword;
    }

    public async Task<EmployeeDto> UpdateProfileAsync(Guid employeeId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(employeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", employeeId);

        // Verificar e-mail duplicado
        if (employee.Email.ToLower() != request.Email.ToLower())
        {
            if (await _unitOfWork.Employees.ExistsByEmailAsync(request.Email, cancellationToken))
            {
                throw new ConflictException("E-mail já cadastrado");
            }
        }

        // Atualizar dados básicos (sem alterar cargo ou gerente)
        employee.Name = request.Name;
        employee.Email = request.Email.ToLower();

        // Atualizar telefones
        employee.Phones.Clear();
        foreach (var phone in request.Phones)
        {
            employee.Phones.Add(new Phone
            {
                Number = phone.Number,
                Type = phone.Type,
                EmployeeId = employee.Id
            });
        }

        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarregar
        employee = await _unitOfWork.Employees.GetByIdWithDetailsAsync(employeeId, cancellationToken);

        _logger.LogInformation("Perfil atualizado: {Email}", request.Email);

        return MapToDto(employee!);
    }

    private static string GenerateTemporaryPassword()
    {
        // Gera uma senha temporária segura: 8 caracteres alfanuméricos
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()) + "!";
    }

    public async Task<IEnumerable<ManagerOptionDto>> GetAvailableManagersAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _unitOfWork.Employees.GetEnabledWithRoleAsync(cancellationToken);
        return employees.Select(e => new ManagerOptionDto(e.Id, e.Name, e.Role.Name));
    }

    public async Task<string> UploadPhotoAsync(Guid employeeId, Microsoft.AspNetCore.Http.IFormFile file, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", employeeId);

        // Pode alterar própria foto ou de subordinados
        var isSelf = employeeId == currentUserId;
        var canEdit = currentUser.Role.CanEditEmployees;
        
        if (!isSelf && !canEdit)
        {
            throw new ForbiddenException("Você não tem permissão para alterar a foto deste funcionário");
        }

        // Criar diretório se não existir
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
        Directory.CreateDirectory(uploadsFolder);

        // Remover foto antiga se existir
        if (!string.IsNullOrEmpty(employee.PhotoPath))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employee.PhotoPath.TrimStart('/'));
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }

        // Salvar nova foto
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{employeeId}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Atualizar caminho no banco
        var photoUrl = $"/uploads/photos/{fileName}";
        employee.PhotoPath = photoUrl;
        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Foto atualizada para: {Email}", employee.Email);

        return photoUrl;
    }

    public async Task DeletePhotoAsync(Guid employeeId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", employeeId);

        var isSelf = employeeId == currentUserId;
        var canEdit = currentUser.Role.CanEditEmployees;
        
        if (!isSelf && !canEdit)
        {
            throw new ForbiddenException("Você não tem permissão para remover a foto deste funcionário");
        }

        if (!string.IsNullOrEmpty(employee.PhotoPath))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employee.PhotoPath.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            employee.PhotoPath = null;
            await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Foto removida para: {Email}", employee.Email);
        }
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
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
                Id = employee.Role.Id,
                Name = employee.Role.Name,
                HierarchyLevel = employee.Role.HierarchyLevel
            },
            ManagerId = employee.ManagerId,
            ManagerName = employee.Manager?.Name,
            Enabled = employee.Enabled,
            ApprovedAt = employee.ApprovedAt,
            ApprovedByName = employee.ApprovedBy?.Name,
            Phones = employee.Phones.Select(p => new PhoneDto
            {
                Number = p.Number,
                Type = p.Type
            }).ToList(),
            PhotoUrl = employee.PhotoPath,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }
}

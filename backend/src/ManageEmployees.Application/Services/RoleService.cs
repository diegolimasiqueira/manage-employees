using ManageEmployees.Application.DTOs;
using ManageEmployees.Application.Interfaces;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Exceptions;
using ManageEmployees.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManageEmployees.Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IUnitOfWork unitOfWork, ILogger<RoleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _unitOfWork.Roles.GetAllOrderedByHierarchyAsync(cancellationToken);
        return roles.Select(MapToDto);
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        return role != null ? MapToDto(role) : null;
    }

    public async Task<IEnumerable<RoleSimpleDto>> GetAssignableRolesAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        var roles = await _unitOfWork.Roles.GetRolesWithLowerHierarchyAsync(currentUser.Role.HierarchyLevel, cancellationToken);
        return roles.Select(r => new RoleSimpleDto
        {
            Id = r.Id,
            Name = r.Name,
            HierarchyLevel = r.HierarchyLevel
        });
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        if (!currentUser.Role.CanManageRoles)
        {
            throw new ForbiddenException("Você não tem permissão para gerenciar cargos");
        }

        // Verificar nome duplicado
        if (await _unitOfWork.Roles.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException("Já existe um cargo com este nome");
        }

        // Não pode criar cargo com hierarquia igual ou maior que a sua
        if (request.HierarchyLevel >= currentUser.Role.HierarchyLevel)
        {
            throw new ForbiddenException("Você não pode criar cargos com hierarquia igual ou superior à sua");
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            HierarchyLevel = request.HierarchyLevel,
            CanApproveRegistrations = request.CanApproveRegistrations,
            CanCreateEmployees = request.CanCreateEmployees,
            CanEditEmployees = request.CanEditEmployees,
            CanDeleteEmployees = request.CanDeleteEmployees,
            CanManageRoles = request.CanManageRoles
        };

        await _unitOfWork.Roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cargo criado: {Name} por {Creator}", request.Name, currentUser.Email);

        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        if (!currentUser.Role.CanManageRoles)
        {
            throw new ForbiddenException("Você não tem permissão para gerenciar cargos");
        }

        var role = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Role", id);

        // Não pode editar cargo com hierarquia igual ou maior que a sua
        if (role.HierarchyLevel >= currentUser.Role.HierarchyLevel)
        {
            throw new ForbiddenException("Você não pode editar cargos com hierarquia igual ou superior à sua");
        }

        // Verificar nome duplicado
        if (role.Name.ToLower() != request.Name.ToLower())
        {
            if (await _unitOfWork.Roles.ExistsByNameAsync(request.Name, cancellationToken))
            {
                throw new ConflictException("Já existe um cargo com este nome");
            }
        }

        role.Name = request.Name;
        role.Description = request.Description;
        role.HierarchyLevel = request.HierarchyLevel;
        role.CanApproveRegistrations = request.CanApproveRegistrations;
        role.CanCreateEmployees = request.CanCreateEmployees;
        role.CanEditEmployees = request.CanEditEmployees;
        role.CanDeleteEmployees = request.CanDeleteEmployees;
        role.CanManageRoles = request.CanManageRoles;

        await _unitOfWork.Roles.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cargo atualizado: {Name} por {Editor}", request.Name, currentUser.Email);

        return MapToDto(role);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUser = await _unitOfWork.Employees.GetByIdWithDetailsAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("Employee", currentUserId);

        if (!currentUser.Role.CanManageRoles)
        {
            throw new ForbiddenException("Você não tem permissão para gerenciar cargos");
        }

        var role = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Role", id);

        // Não pode excluir cargo com hierarquia igual ou maior que a sua
        if (role.HierarchyLevel >= currentUser.Role.HierarchyLevel)
        {
            throw new ForbiddenException("Você não pode excluir cargos com hierarquia igual ou superior à sua");
        }

        // Verificar se há funcionários com este cargo
        if (await _unitOfWork.Employees.ExistsWithRoleAsync(id, cancellationToken))
        {
            throw new ConflictException("Não é possível excluir cargo que possui funcionários vinculados");
        }

        // Soft delete
        role.IsActive = false;
        await _unitOfWork.Roles.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cargo excluído: {Name} por {Deleter}", role.Name, currentUser.Email);
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            HierarchyLevel = role.HierarchyLevel,
            CanApproveRegistrations = role.CanApproveRegistrations,
            CanCreateEmployees = role.CanCreateEmployees,
            CanEditEmployees = role.CanEditEmployees,
            CanDeleteEmployees = role.CanDeleteEmployees,
            CanManageRoles = role.CanManageRoles
        };
    }
}

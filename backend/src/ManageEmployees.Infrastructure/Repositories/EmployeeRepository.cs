using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Interfaces;
using ManageEmployees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ManageEmployees.Infrastructure.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && e.IsActive, cancellationToken);
    }

    public async Task<Employee?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.DocumentNumber == documentNumber && e.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(e => e.Email.ToLower() == email.ToLower() && e.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(e => e.DocumentNumber == documentNumber && e.IsActive, cancellationToken);
    }

    public async Task<Employee?> GetByEmailWithDetailsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Role)
            .Include(e => e.Manager)
            .Include(e => e.Phones)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && e.IsActive, cancellationToken);
    }

    public async Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Role)
            .Include(e => e.Manager)
                .ThenInclude(m => m!.Role)
            .Include(e => e.Phones)
            .Include(e => e.ApprovedBy)
            .Include(e => e.Subordinates)
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Role)
            .Include(e => e.Manager)
            .Include(e => e.Phones)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetPendingApprovalAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Role)
            .Include(e => e.Manager)
            .Include(e => e.Phones)
            .Where(e => e.IsActive && !e.Enabled)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Role)
            .Include(e => e.Phones)
            .Where(e => e.ManagerId == managerId && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Manager)
            .Include(e => e.Phones)
            .Where(e => e.RoleId == roleId && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.RoleId == roleId && e.IsActive, cancellationToken);
    }

    public async Task<int> CountPendingApprovalForManagerAsync(Guid managerId, int managerHierarchyLevel, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Role)
            .Where(e => e.IsActive && !e.Enabled && e.Role.HierarchyLevel < managerHierarchyLevel)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetEnabledWithRoleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Role)
            .Where(e => e.IsActive && e.Enabled)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }
}

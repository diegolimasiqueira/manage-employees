using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Interfaces;
using ManageEmployees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ManageEmployees.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower() && r.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllOrderedByHierarchyAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.HierarchyLevel)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.Name.ToLower() == name.ToLower() && r.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesWithLowerHierarchyAsync(int hierarchyLevel, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.HierarchyLevel < hierarchyLevel && r.IsActive)
            .OrderByDescending(r => r.HierarchyLevel)
            .ToListAsync(cancellationToken);
    }
}




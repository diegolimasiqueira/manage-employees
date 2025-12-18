using System.Linq.Expressions;
using ManageEmployees.Domain.Entities;
using ManageEmployees.Domain.Interfaces;
using ManageEmployees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ManageEmployees.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsActive, cancellationToken);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.IsActive).ToListAsync(cancellationToken);
    }
    
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).Where(e => e.IsActive).ToListAsync(cancellationToken);
    }
    
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.IsActive).FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.IsActive).AnyAsync(predicate, cancellationToken);
    }
    
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }
    
    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Soft delete
        entity.IsActive = false;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(e => e.IsActive);
        
        if (predicate != null)
            query = query.Where(predicate);
        
        return await query.CountAsync(cancellationToken);
    }
}


using HelpDeskSystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    IQueryable<T> Query();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly HelpDeskDbContext Db;
    private readonly DbSet<T> _set;

    public GenericRepository(HelpDeskDbContext db)
    {
        Db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);
    public IQueryable<T> Query() => _set.AsQueryable();
    public async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
}

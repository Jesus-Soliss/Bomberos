using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Infrastructure.Persistence.Repositories;

public class GenericRepository<T>(ApplicationContext context) : IGenericRepository<T> where T : class
{
    protected readonly ApplicationContext Context = context;

    public virtual async Task<T?> GetByIdAsync(int id) => await Context.Set<T>().FindAsync(id);

    public virtual Task<List<T>> GetAllAsync() => Context.Set<T>().ToListAsync();

    public IQueryable<T> Query() => Context.Set<T>().AsQueryable();

    public virtual async Task<T> AddAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
        await Context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
    }
}

public class UnitOfWork(ApplicationContext context) : IUnitOfWork
{
    public async Task EjecutarEnTransaccionAsync(Func<Task> operacion)
    {
        if (context.Database.CurrentTransaction is not null)
        {
            await operacion();
            return;
        }

        await using var transaccion = await context.Database.BeginTransactionAsync();
        await operacion();
        await transaccion.CommitAsync();
    }
}

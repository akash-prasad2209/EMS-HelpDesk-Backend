using HelpDeskSystem.Api.Data;
using HelpDeskSystem.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HelpDeskSystem.Api.Repositories;

public interface IUnitOfWork
{
    ITicketRepository Tickets { get; }
    IGenericRepository<Employee> Employees { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Department> Departments { get; }
    IGenericRepository<RequestType> RequestTypes { get; }
    IGenericRepository<Asset> Assets { get; }
    IGenericRepository<TicketComment> TicketComments { get; }
    IGenericRepository<TicketHistory> TicketHistories { get; }
    IGenericRepository<TicketAttachment> TicketAttachments { get; }
    IGenericRepository<Notification> Notifications { get; }

    Task<int> SaveChangesAsync();
    Task ExecuteInTransactionAsync(Func<Task> action);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly HelpDeskDbContext _db;

    public UnitOfWork(HelpDeskDbContext db)
    {
        _db = db;
        Tickets = new TicketRepository(db);
        Employees = new GenericRepository<Employee>(db);
        Categories = new GenericRepository<Category>(db);
        Departments = new GenericRepository<Department>(db);
        RequestTypes = new GenericRepository<RequestType>(db);
        Assets = new GenericRepository<Asset>(db);
        TicketComments = new GenericRepository<TicketComment>(db);
        TicketHistories = new GenericRepository<TicketHistory>(db);
        TicketAttachments = new GenericRepository<TicketAttachment>(db);
        Notifications = new GenericRepository<Notification>(db);
    }

    public ITicketRepository Tickets { get; }
    public IGenericRepository<Employee> Employees { get; }
    public IGenericRepository<Category> Categories { get; }
    public IGenericRepository<Department> Departments { get; }
    public IGenericRepository<RequestType> RequestTypes { get; }
    public IGenericRepository<Asset> Assets { get; }
    public IGenericRepository<TicketComment> TicketComments { get; }
    public IGenericRepository<TicketHistory> TicketHistories { get; }
    public IGenericRepository<TicketAttachment> TicketAttachments { get; }
    public IGenericRepository<Notification> Notifications { get; }

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            IDbContextTransaction transaction =
                await _db.Database.BeginTransactionAsync();

            try
            {
                await action();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}

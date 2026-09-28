using HelpDeskSystem.Api.Data;
using HelpDeskSystem.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Repositories;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    // Includes what the Policy pattern needs to join on (Category.DepartmentId)
    // plus what the DTO mapping needs to display, in one place.
    IQueryable<Ticket> QueryWithDetails();
    Task<Ticket?> GetWithDetailsAsync(int ticketId);
}

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(HelpDeskDbContext db) : base(db) { }

    public IQueryable<Ticket> QueryWithDetails() =>
        Query()
            .Include(t => t.Category).ThenInclude(c => c!.Department)
            .Include(t => t.RequestType)
            .Include(t => t.CreatedByEmployee)
            .Include(t => t.AssignedToEmployee)
            .Include(t => t.Asset);

    public async Task<Ticket?> GetWithDetailsAsync(int ticketId) =>
        await QueryWithDetails().FirstOrDefaultAsync(t => t.TicketId == ticketId);
}

using HelpDeskSystem.Api.Common;
using HelpDeskSystem.Api.Models.Dtos;
using HelpDeskSystem.Api.Models.Entities;
using HelpDeskSystem.Api.Repositories;

namespace HelpDeskSystem.Api.Patterns.Factory;

// FACTORY - the caller (TicketService) only says "raise a ticket from this DTO";
// it never assembles a Ticket by hand or worries about which fields must be
// set together. Adding a new required field later means editing this one method.
public class TicketFactory
{
    private readonly IUnitOfWork _uow;

    public TicketFactory(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Ticket> CreateAsync(int createdByEmployeeId, CreateTicketDto dto)
    {
        var category = await _uow.Categories.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException($"Category {dto.CategoryId} was not found.");
        _ = await _uow.RequestTypes.GetByIdAsync(dto.RequestTypeId)
            ?? throw new NotFoundException($"Request type {dto.RequestTypeId} was not found.");

        if (dto.AssetId.HasValue)
        {
            _ = await _uow.Assets.GetByIdAsync(dto.AssetId.Value)
                ?? throw new NotFoundException($"Asset {dto.AssetId} was not found.");
        }

        return Ticket.Raise(
            createdByEmployeeId,
            category.CategoryId,
            dto.RequestTypeId,
            dto.ShortDescription,
            dto.Description,
            dto.Impact,
            dto.Urgency,
            dto.AssetId);
    }
}

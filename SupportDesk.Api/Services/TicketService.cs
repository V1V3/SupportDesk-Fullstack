using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Contracts;
using SupportDesk.Api.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SupportDesk.Api.Services;

public class TicketService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TicketService> _logger;

    public TicketService(AppDbContext db, ILogger<TicketService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<TicketSummaryDto>> GetPagedAsync(GetTicketsQuery request)
    {
        var page = request.Page < 1 ? 1 : request.Page;

        const int maxPageSize = 100;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        if (pageSize > maxPageSize) pageSize = maxPageSize;

        var status = request.Status?.Trim();
        var search = request.Search?.Trim();
        var createdBy = request.CreatedBy?.Trim();
        var sortBy = request.SortBy?.Trim()?.ToLowerInvariant() ?? "createdatutc";
        var sortDirection = request.SortDirection?.Trim()?.ToLowerInvariant() ?? "desc";

        _logger.LogInformation(
            "Fetching tickets with filters. Page {Page}, PageSize {PageSize}, Status {Status}, Search {Search}, CreatedBy {CreatedBy}, SortBy {SortBy}, SortDirection {SortDirection}",
            page,
            pageSize,
            status,
            search,
            createdBy,
            sortBy,
            sortDirection);

        IQueryable<Ticket> query = _db.Tickets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Title.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(createdBy))
        {
            query = query.Where(t => t.CreatedBy == createdBy);
        }

        query = ApplySorting(query, sortBy, sortDirection);

        var totalCount = await query.CountAsync();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketSummaryDto(t.Id, t.Title, t.Status, t.CreatedBy))
            .ToListAsync();

        _logger.LogInformation(
            "Fetched {ItemCount} tickets for page {Page}. Total tickets: {TotalCount}, total pages: {TotalPages}",
            items.Count,
            page,
            totalCount,
            totalPages);

        return new PagedResult<TicketSummaryDto>(
            items,
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    private static IQueryable<Ticket> ApplySorting(
        IQueryable<Ticket> query,
        string sortBy,
        string sortDirection)
    {
        var descending = sortDirection != "asc";

        return (sortBy, descending) switch
        {
            ("title", false) => query.OrderBy(t => t.Title).ThenBy(t => t.Id),
            ("title", true) => query.OrderByDescending(t => t.Title).ThenByDescending(t => t.Id),

            ("status", false) => query.OrderBy(t => t.Status).ThenBy(t => t.Id),
            ("status", true) => query.OrderByDescending(t => t.Status).ThenByDescending(t => t.Id),

            ("createdby", false) => query.OrderBy(t => t.CreatedBy).ThenBy(t => t.Id),
            ("createdby", true) => query.OrderByDescending(t => t.CreatedBy).ThenByDescending(t => t.Id),

            ("id", false) => query.OrderBy(t => t.Id),
            ("id", true) => query.OrderByDescending(t => t.Id),

            ("createdatutc", false) => query.OrderBy(t => t.CreatedAtUtc).ThenBy(t => t.Id),
            _ => query.OrderByDescending(t => t.CreatedAtUtc).ThenByDescending(t => t.Id)
        };
    }

    public async Task<TicketSummaryDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching ticket with id {TicketId}", id);

        var ticket = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TicketSummaryDto(t.Id, t.Title, t.Status, t.CreatedBy))
            .FirstOrDefaultAsync();

        if (ticket is null)
        {
            _logger.LogWarning("Ticket with id {TicketId} was not found", id);
            return null;
        }

        _logger.LogInformation("Ticket with id {TicketId} was found", id);
        return ticket;
    }

    public async Task<TicketSummaryDto> CreateAsync(string title, string createdBy)
    {
        var ticket = new Ticket
        {
            Title = title.Trim(),
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
    "Created ticket with id {TicketId}, title {Title}, created by {CreatedBy}",
    ticket.Id,
    ticket.Title,
    ticket.CreatedBy);

        return new TicketSummaryDto(ticket.Id, ticket.Title, ticket.Status, ticket.CreatedBy);
    }

    public async Task<TicketOperationResult> UpdateStatusAsync(int id, string newStatus, string currentUsername, string? currentUserRole)
    {
        _logger.LogInformation(
            "User {Username} is attempting to update ticket {TicketId} to status {NewStatus}",
            currentUsername,
            id,
            newStatus);

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            _logger.LogWarning(
                "Could not update ticket {TicketId} because it was not found",
                id);

            return TicketOperationResult.NotFound;
        }

        var isOwner = string.Equals(ticket.CreatedBy, currentUsername, StringComparison.Ordinal);
        var isAdmin = string.Equals(currentUserRole, "Admin", StringComparison.Ordinal);

        if (!isOwner && !isAdmin)
        {
            _logger.LogWarning(
                "User {Username} with role {Role} is forbidden from modifying ticket {TicketId} owned by {Owner}",
                currentUsername,
                currentUserRole,
                id,
                ticket.CreatedBy);

            return TicketOperationResult.Forbidden;
        }

        ticket.Status = newStatus;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "User {Username} updated ticket {TicketId} to status {NewStatus}",
            currentUsername,
            id,
            newStatus);

        return TicketOperationResult.Success;
    }

    public async Task<TicketOperationResult> DeleteAsync(int id, string currentUsername, string? currentUserRole)
    {
        _logger.LogInformation(
            "User {Username} is attempting to delete ticket {TicketId}",
            currentUsername,
            id);

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            _logger.LogWarning(
                "Could not delete ticket {TicketId} because it was not found",
                id);

            return TicketOperationResult.NotFound;
        }

        var isOwner = string.Equals(ticket.CreatedBy, currentUsername, StringComparison.Ordinal);
        var isAdmin = string.Equals(currentUserRole, "Admin", StringComparison.Ordinal);

        if (!isOwner && !isAdmin)
        {
            _logger.LogWarning(
                "User {Username} with role {Role} is forbidden from modifying ticket {TicketId} owned by {Owner}",
                currentUsername,
                currentUserRole,
                id,
                ticket.CreatedBy);

            return TicketOperationResult.Forbidden;
        }

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "User {Username} deleted ticket {TicketId}",
            currentUsername,
            id);

        return TicketOperationResult.Success;
    }
}
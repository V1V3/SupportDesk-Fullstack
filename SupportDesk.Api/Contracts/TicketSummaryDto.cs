namespace SupportDesk.Api.Contracts;

public record TicketSummaryDto(int Id, string Title, string Status, string CreatedBy);